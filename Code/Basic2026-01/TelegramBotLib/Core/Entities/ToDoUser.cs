namespace TelegramBotLib.Core.Entities
{
    public class ToDoUser
    {
        public long TelegramUserId { get; set; }
        public Guid UserId { get; set; }
        public string TelegramUserName { get; set; }
        public DateTime RegisteredAt { get; set; }

    }
}
