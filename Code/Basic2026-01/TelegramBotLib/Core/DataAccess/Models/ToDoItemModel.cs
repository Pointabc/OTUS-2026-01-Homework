using LinqToDB.Mapping;
using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Core.DataAccess.Models
{
    [Table("ToDoItem")]
    public class ToDoItemModel
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUser.UserId))]
        public Guid UserId { get; set; }
        public ToDoUser User { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; }
        public DateTime Deadline { get; set; }
        [Association(ThisKey = nameof(ListId), OtherKey = nameof(ToDoList.Id))]
        public Guid ListId { get; set; }
        public ToDoList? List { get; set; }
    }
}
