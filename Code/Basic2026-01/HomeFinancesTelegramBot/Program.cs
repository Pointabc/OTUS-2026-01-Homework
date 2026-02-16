using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static System.Console;

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

static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
{
    // Only process Message updates: https://core.tetegram.org/bots/api#message
    if (update.Message is not { } message)
        return;

    // Only process text messages
    if (message.Text is not { } messageText)
        return;

    var chatId = message.Chat.Id;
    var username = message.From?.Username ?? "Unknown user";

    WriteLine($"Received a message from {username}: {messageText} : sent at {message.Date.ToLocalTime()}");

    // Echo received message text
    string responseText;

    // Process commands
    if (messageText.StartsWith("/"))
    {
        if (messageText.Equals("/start", StringComparison.OrdinalIgnoreCase))
        {
            responseText = $"Hello, {username}! I’m your bot. Send me any message and I'll respond.";
        }
        else if (messageText.Equals("/help", StringComparison.OrdinalIgnoreCase))
        {
            responseText = "Available commands:\n" +
            "/start - Start the bot\n" +
            "/help - Show available commands\n" +
            "/time - Show current time";
        }
        else if (messageText.Equals("/time", StringComparison.OrdinalIgnoreCase))
        {
            responseText = $"Current time is: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }
        else
        {
            responseText = "Unknown command. Type /help to see available commands.";
        }
    }
    else
    {
        // Echo the message with a little extra
        responseText = $"You said: {messageText}\n\nReply generated at {DateTime.Now:HH:mm:ss}";
    }

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