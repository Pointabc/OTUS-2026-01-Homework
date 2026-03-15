
namespace TelegramBotLib
{
    public enum ToDoItemState { Active, Completed }

    internal class ToDoItem
    {
        public long Number { get; set; }
        public Guid Id { get; set; }
        ToDoUser User { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public ToDoItemState State { get; set; }
        public DateTime? StateChangedAt { get; set; }

        public ToDoItem(ToDoUser user, string name, long number)
        {
            Id = Guid.NewGuid();
            User = user;
            Name = name;
            CreatedAt = DateTime.Now;
            State = ToDoItemState.Active;
            Number = number;
        }
    }
}
