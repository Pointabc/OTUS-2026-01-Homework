using LinqToDB.Mapping;
using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Core.DataAccess.Models
{
    [Table("ToDoItem")]
    public class ToDoItemModel
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        [Column]
        public Guid UserId { get; set; }
        [Association(ThisKey = nameof(UserId), OtherKey = nameof(ToDoUserModel.UserId))]
        public ToDoUser User { get; set; }
        [Column]
        public string Name { get; set; }
        [Column]
        public DateTime CreatedAt { get; set; }
        [Column]
        public ToDoItemState State { get; set; }
        [Column]
        public DateTime? StateChangedAt { get; set; }
        [Column]
        public DateTime Deadline { get; set; }
        [Column]
        public Guid? ListId { get; set; }
        [Association(ThisKey = nameof(ListId), OtherKey = nameof(ToDoListModel.Id))]
        public ToDoList? List { get; set; }
    }
}
