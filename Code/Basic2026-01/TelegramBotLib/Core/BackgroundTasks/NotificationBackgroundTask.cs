using Telegram.Bot;
using TelegramBotLib.Core.Services;
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
        _resetScenarioTimeout = resetScenarioTimeout != TimeSpan.Zero
            ? resetScenarioTimeout
            : throw new ArgumentNullException();
        _notificationService = notificationService ?? throw new ArgumentNullException();
        _bot = bot ?? throw new ArgumentNullException();
    }

    protected override async Task Execute(CancellationToken ct)
    {
        // Получить нотификации, которые нужно отправить.
        var notifications = await _notificationService.GetScheduledNotification(DateTime.UtcNow, ct);
        if (!notifications.Any())
            return;

        // Отправить нотификации через ITelegramBotClient.
        foreach (var notification in notifications)
        {
            try
            {
                var keyboardMarkup = await UpdateHandler.CreateKeyboardMarkupDefault();
                await _bot.SendMessage(
                    notification.User.TelegramUserId,
                    $"{notification.Text}",
                    replyMarkup: keyboardMarkup,
                    cancellationToken: ct);

                // Пометить нотификации отправленными MarkNotified.
                await _notificationService.MarkNotified(notification.Id, ct);
            }
            catch (Exception ex)
            {
                throw new Exception("Возникла ошибка при отправке уведомления или записи в БД.");
            }
        }
    }
}
