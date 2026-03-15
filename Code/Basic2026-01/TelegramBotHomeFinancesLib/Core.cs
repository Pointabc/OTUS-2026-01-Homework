using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static System.Console;

namespace TelegramBotHomeFinancesLib
{
    public class Core
    {
        Chat chat;
        User user;

        public async Task StartAsync()
        {
            try
            {
                // Get token from environment variable
                string? token = Environment.GetEnvironmentVariable("TelegramBotTokenOTUSBasic", EnvironmentVariableTarget.User);

                if (string.IsNullOrWhiteSpace(token))
                {
                    WriteLine("Bot token not found. Please set the TELEGRAM_BOT_TOKEN environment variable.");
                    return;
                }

                var botClient = new TelegramBotClient(token);

                // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
                ReceiverOptions receiveroptions = new()
                {
                    AllowedUpdates = Array.Empty<UpdateType>()    // receive all update types
                };

                using CancellationTokenSource cts = new();

                botClient.StartReceiving(
                    updateHandler: HandleUpdateAsync,
                    errorHandler: HandlePollingErrorAsync,
                    receiverOptions: receiveroptions,
                    cancellationToken: cts.Token);

                // Basic bot initialization
                var me = await botClient.GetMe();
                WriteLine($"Bot started: @{me.Username}");
                WriteLine("Press Ctrl+C to exit");

                // Wait for Ctrl+C to be pressed
                CancelKeyPress += (sender, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    cts.Cancel();
                    WriteLine("Bot stopping...");
                };

                // Keep the application running
                await Task.Delay(Timeout.Infinite, cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Бот остановлен");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                throw;
            }
        }

        async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.tetegram.org/bots/api#message
            if (update.Message is not { } message)
                return;

            // Only process text messages
            if (message.Text is not { } messageText)
                return;

            // Тут вроде как обновляется id чата и имя пользователя, но нужно ли это?
            chat.Id = message.Chat.Id;
            var chatId = chat.Id;
            user.TelegramUserName = message.From?.Username ?? "Неизвестный пользователь";
            var username = user.TelegramUserName;

            // TODO 15032026 эту информацию нужно в логи писать.
            WriteLine($"Received a message from {username}: {messageText} : sent at {message.Date.ToLocalTime()}");

            // Echo received message text
            string responseText = string.Empty;

            #region Обработка команд.

            // Process commands
            if (messageText.StartsWith("/"))
            {
                switch (messageText.ToLower())
                {
                    case Constants.CommandStart:
                        responseText = CommandStart(username);
                        break;
                    case Constants.CommandHelp:
                        responseText = CommandHelp();
                        break;
                    case Constants.CommandInfo:
                        responseText = CommandInfo();
                        break;
                    default:
                        WriteLine(Constants.UnknownCommand);
                        CommandHelp();
                        break;
                }
            }
            else
            {
                responseText = $"Нет такой команды: {messageText}\n\nОтвет создан {DateTime.Now:HH:mm:ss}";
            }

            #endregion

            // Send the response
            Message sentMessage = await botClient.SendMessage(
                chatId: chatId,
                text: responseText,
                cancellationToken: cancellationToken);
        }

        static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegran API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            WriteLine(ErrorMessage);

            return Task.CompletedTask;
        }

        #region Обработка команд.

        string CommandStart(string username)
        {
            return $"Привет, {username}! Я твой бот. Начнем работать с домашними финансами. Набери {Constants.CommandHelp}, чтобы получить доступные команды.";
        }

        string CommandHelp()
        {
            var responseText = new StringBuilder();
            responseText.AppendLine("Cписок команд:");
            responseText.AppendLine($"{Constants.CommandStart} - Начать работать с ботом.");
            responseText.AppendLine($"{Constants.CommandHelp} - Вывести команды.");
            responseText.AppendLine($"{Constants.CommandInfo} - Вывести информацию о Telegram боте.");

            return responseText.ToString();
        }

        string CommandInfo()
        {
            return $"Информация о программе.\nВерсия бота 0.0.1. Дата создания {Constants.CreatedDate}";
        }

        #endregion
    }
}
