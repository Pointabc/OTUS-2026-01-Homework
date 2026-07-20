using Telegram.Bot;
using TelegramBotLib.Core.Scenarios;
using TelegramBotLib.Core.Services;
using TelegramBotLib.Infrastructure;
using TelegramBotLib.TelegramBot;

namespace TelegramBotLib.Core.BackgroundTasks;

internal class NotificationBackgroundTask : BackgroundTask
{
    ITelegramBotClient _bot { get; set; }
    INotificationService _notificationService { get; set; }
    TimeSpan _resetScenarioTimeout { get; set; }

    public NotificationBackgroundTask(
        TimeSpan resetScenarioTimeout,
        INotificationService notificationService,
        ITelegramBotClient bot)
        : base(resetScenarioTimeout, nameof(NotificationBackgroundTask))
    {
        _resetScenarioTimeout = resetScenarioTimeout;
        _notificationService = notificationService;
        _bot = bot;
    }

    protected override async Task Execute(CancellationToken ct)
    {
        // Получить нотификации, которые нужно отправить.
        var notifications = await _notificationService.GetScheduledNotification(DateTime.UtcNow, ct);
        // Отправить нотификации через ITelegramBotClient.
        foreach (var notification in notifications)
        {
            var keyboardMarkup = await UpdateHandler.CreateKeyboardMarkupDefault();
            await _bot.SendMessage(
                notification.User.TelegramUserId,
                $"{notification.Text}",
                replyMarkup: keyboardMarkup,
                cancellationToken: CancellationToken.None);

            // Пометить нотификации отправленными MarkNotified.
            await _notificationService.MarkNotified(notification.Id, ct);
        }
    }
}
