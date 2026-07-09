using LinqToDB.Mapping;
using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Core.DataAccess.Models
{
    [Table("ToDoList")]
    public class ToDoListModel
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Name { get; set; }
        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUser.UserId))]
        public Guid UserId { get; set; }
        public ToDoUser User { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
