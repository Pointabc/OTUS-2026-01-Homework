using LinqToDB.Mapping;

namespace TelegramBotLib.Core.DataAccess.Models
{
    [Table("ToDoUser")]
    public class ToDoUserModel
    {
        [PrimaryKey]
        public Guid UserId { get; set; }
        [Column]
        public long TelegramUserId { get; set; }
        [Column]
        public string TelegramUserName { get; set; }
        [Column]
        public DateTime RegisteredAt { get; set; }
    }
}
