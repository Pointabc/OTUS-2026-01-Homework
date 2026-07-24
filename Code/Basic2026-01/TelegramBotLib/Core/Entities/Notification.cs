namespace TelegramBotLib.Core.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public ToDoUser User { get; set; }
    /// <summary>
    /// Тип нотификации. Например: DeadLine_{ToDoItem.Id}, Today_{DateOnly.FromDateTime(DateTime.UtcNow)}.
    /// </summary>
    public string Type { get; set; }
    /// <summary>
    /// Текст, который будет отправлен.
    /// </summary>
    public string Text { get; set; }
    /// <summary>
    /// Запланированная дата отправки.
    /// </summary>
    public DateTime ScheduledAt { get; set; }
    /// <summary>
    /// Флаг отправки.
    /// </summary>
    public bool IsNotified { get; set; }
    /// <summary>
    /// Фактическая дата отправки.
    /// </summary>
    public DateTime? NotifiedAt { get; set; }
}
