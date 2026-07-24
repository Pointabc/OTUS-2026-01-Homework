using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Core.Services;

public interface INotificationService
{
    /// <summary>
    /// Создает уведомление.
    /// </summary>
    /// <param name="userId">ИД пользователя.</param>
    /// <param name="type">Тип нотификации.</param>
    /// <param name="text">Текст, который будет отправлен.</param>
    /// <param name="scheduledAt">Запланированная дата отправки.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>False - если запись с userId и type уже есть, иначе создать уведомление и вернуть True.</returns>
    Task<bool> ScheduleNotification(
        Guid userId,
        string type,
        string text,
        DateTime scheduledAt,
        CancellationToken ct);

    /// <summary>
    /// Возвращает уведомления, у которых IsNotified = false && ScheduledAt <= scheduledBefore.
    /// </summary>
    /// <param name="scheduledBefore">Запланированная дата отправки.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Уведомления.</returns>
    Task<IReadOnlyList<Notification>> GetScheduledNotification(DateTime scheduledBefore, CancellationToken ct);

    /// <summary>
    /// Установить уведомление.
    /// </summary>
    /// <param name="notificationId">ИД уведомления.</param>
    /// <param name="ct">Токен отмены.</param>
    Task MarkNotified(Guid notificationId, CancellationToken ct);
}
