
namespace TelegramBotLib
{
    public enum ToDoItemState { Active, Completed }

    internal class ToDoItem
    {
        public static long maxNumber = 100;
        public static long maxTaskDiscriptionLength = 100;

        public long Number { get; set; }
        public Guid Id;
        ToDoUser User;
        public string Name;
        public DateTime CreatedAt;
        public ToDoItemState State;
        public DateTime? StateChangedAt;

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
