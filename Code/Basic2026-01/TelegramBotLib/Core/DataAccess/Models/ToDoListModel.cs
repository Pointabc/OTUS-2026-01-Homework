using LinqToDB.Mapping;
using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Core.DataAccess.Models
{
    [Table("ToDoList")]
    public class ToDoListModel
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        [Column]
        public string Name { get; set; }
        [Column]
        public Guid UserId { get; set; }
        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.UserId))]
        public ToDoUser User { get; set; }
        [Column]
        public DateTime CreatedAt { get; set; }
    }
}
