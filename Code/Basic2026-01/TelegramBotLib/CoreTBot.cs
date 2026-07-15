using System.Net;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TelegramBotLib.Core.BackgroundTasks;
using TelegramBotLib.Core.Scenarios;
using TelegramBotLib.Infrastructure.DataAccess;
using TelegramBotLib.TelegramBot;
using static System.Console;

namespace TelegramBotLib
{
    public class CoreTBot
    {
        public async Task Start()
        {
            try
            {
                #region WebProxy

                // Добавить прокси.
                string? webProxy = Environment.GetEnvironmentVariable("WebProxyForTelegramBot", EnvironmentVariableTarget.User);

                if (string.IsNullOrWhiteSpace(webProxy))
                {
                    WriteLine("Environment not found. Please set the WebProxyForTelegramBot environment variable.");
                    return;
                }
                var proxy = new WebProxy(webProxy);
                var httpClientHandler = new SocketsHttpHandler
                {
                    Proxy = proxy,
                    UseProxy = true
                };

                var httpClient = new HttpClient(httpClientHandler);

                #endregion

                #region Создать структуру хранилища

                // Создать папку для хранения задач.
                var toDoItemRepositoryFolder = BotConstants.FileToDoItemRepositoryFolderName;
                if (!Directory.Exists(toDoItemRepositoryFolder))
                    Directory.CreateDirectory(toDoItemRepositoryFolder);

                // Создать папку для храниния пользователей.
                var userRepositoryFolder = BotConstants.FileUserRepositoryFolderName;
                if (!Directory.Exists(userRepositoryFolder))
                    Directory.CreateDirectory(userRepositoryFolder);

                // Создать файл-индекс.
                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var fileIndex = Path.Combine(basePath, toDoItemRepositoryFolder, "fileIndex.json");
                if (!File.Exists(fileIndex))
                    using (File.Create(fileIndex)) { }

                // Создать папку для хранения списков (категорий) для задач пользователей.
                var listRepositoryFolder = BotConstants.FileListRepositoryFolderName;
                if (!Directory.Exists(listRepositoryFolder))
                    Directory.CreateDirectory(listRepositoryFolder);

                #endregion

                #region Создать инфраструктуру для сценариев.

                IEnumerable<IScenario> scenarios = new List<IScenario>();
                var contextRepository = new InMemoryScenarioContextRepository();

                #endregion

                #region Создать botClient

                var toDoRepositoryIndex = new FileToDoRepositoryIndex(fileIndex);
                await toDoRepositoryIndex.UpdateFileIndex();

                // Get token from environment variable
                string? token = Environment.GetEnvironmentVariable("ToDoTelegramBotTokenOTUSBasic", EnvironmentVariableTarget.User);
                if (string.IsNullOrWhiteSpace(token))
                {
                    WriteLine("Bot token not found. Please set the TELEGRAM_BOT_TOKEN environment variable.");
                    return;
                }

                var cancellationTokenSource = new CancellationTokenSource();
                var ct = cancellationTokenSource.Token;
                var botClient = new TelegramBotClient(token, httpClient, ct);

                using var backgroundTaskRunner = new BackgroundTaskRunner();
                backgroundTaskRunner.AddTask(new ResetScenarioBackgroundTask(TimeSpan.FromHours(1), contextRepository, botClient));
                // Запустить фоновые задачи.
                backgroundTaskRunner.StartTasks(ct);

                var handler = new UpdateHandler(
                    scenarios,
                    contextRepository,
                    botClient);

                #endregion

                #region Создать меню и описание команд.

                // Создаем список команд
                var commands = new List<BotCommand>
                {
                    new BotCommand { Command = "start", Description = "Начать работать с ботом." },
                    new BotCommand { Command = "help", Description = "Вывести команды." },
                    new BotCommand { Command = "addtask", Description = "Добавить задчу." },
                    new BotCommand { Command = "show", Description = "Вывести задачи в работе." },
                    new BotCommand { Command = "report", Description = "Вывести отчет по задачам." },
                };

                // Устанавливаем команды
                await botClient.SetMyCommands(commands);

                #endregion

                // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
                ReceiverOptions receiveroptions = new()
                {
                    //AllowedUpdates = Array.Empty<UpdateType>()    // receive all update types
                    AllowedUpdates = new UpdateType[]
                    {
                        UpdateType.Message,          // для обычных сообщений.
                        UpdateType.CallbackQuery,    // для нажатий на кнопки.
                        UpdateType.EditedMessage,    // для отредактированных сообщений.
                        // ... добавьте другие типы, если они нужны
                    }
                };

                botClient.StartReceiving(handler, receiveroptions, ct);
                var me = await botClient.GetMe();
                WriteLine($"{me.FirstName} запущен!");
                WriteLine("Нажмите клавишу A для выхода.");

                // Отмена асинхронных операции и остановка приложения при нажатии клавиши A.
                await Task.Run(async () =>
                {
                    while (true)
                    {
                        var key = ReadKey(true);
                        if (key.Key == ConsoleKey.A)
                        {
                            await backgroundTaskRunner.StopTasks(ct);
                            WriteLine("Фоновые задачи остановлены.");
                            cancellationTokenSource.Cancel();
                            WriteLine("Bot stopping...");
                            break;
                        }
                        else
                        {
                            WriteLine($"Id телеграм бота: {me.Id}.");
                        }
                    }
                });

                try
                {
                    await Task.Delay(Timeout.Infinite, ct); // Устанавливаем бесконечную задержку.
                }
                catch (TaskCanceledException)
                {
                    // Ожидаемое исключение - просто игнорируем
                    WriteLine("Bot stopped.");
                }
            }
            catch (Exception e)
            {
                WriteLine("Произошла непредвиденная ошибка: ");
                WriteLine($"Type of exception: {e.GetType()}");
                WriteLine($"Message: {e.Message}");
                WriteLine($"StackTrace: {e.StackTrace}");
                WriteLine($"InnerException: {e.InnerException}");
            }
        }
    }
}
