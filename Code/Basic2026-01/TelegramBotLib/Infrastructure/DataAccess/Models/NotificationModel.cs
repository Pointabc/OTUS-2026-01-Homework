using LinqToDB.Mapping;
using TelegramBotLib.Core.DataAccess.Models;
using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Infrastructure.DataAccess.Models;

[Table("Notification")]
public class NotificationModel
{
    [PrimaryKey]
    public Guid Id { get; set; }
    [Column]
    public Guid UserId { get; set; }
    [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.UserId))]
    public ToDoUser User { get; set; }
    [Column]
    public string Type { get; set; }
    [Column]
    public string Text { get; set; }
    [Column]
    public DateTime ScheduledAt { get; set; }
    [Column]
    public bool IsNotified { get; set; }
    [Column]
    public DateTime? NotifiedAt { get; set; }
}
