
namespace TelegramBotLib.Core.Entities
{
    internal class ToDoList
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public ToDoUser User { get; set; }
        public DateTime CreatedAt { get; set; }

        public ToDoList(string name, ToDoUser user)
        {
            Id = Guid.NewGuid();
            User = user;
            Name = name;
            CreatedAt = DateTime.Now;
        }
    }
}
