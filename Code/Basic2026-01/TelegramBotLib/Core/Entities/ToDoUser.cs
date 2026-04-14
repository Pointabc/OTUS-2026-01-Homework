namespace TelegramBotLib.Core.Entities
{
    internal class ToDoUser
    {
        public long TelegramUserId { get; set; }
        public Guid UserId { get; set; }
        public string TelegramUserName { get; set; }
        public DateTime RegisteredAt { get; set; }

        public ToDoUser(string telegramUserName, long telegramUserId)
        {
            TelegramUserId = telegramUserId;
            UserId = Guid.NewGuid();
            TelegramUserName = telegramUserName;
            RegisteredAt = DateTime.Now;
        }
    }
}
