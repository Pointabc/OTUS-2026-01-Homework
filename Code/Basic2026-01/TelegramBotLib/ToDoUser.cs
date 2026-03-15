
namespace TelegramBotLib
{
    internal class ToDoUser
    {
        public Guid UserId;
        public string TelegramUserName;
        public DateTime RegisteredAt;

        public ToDoUser(string telegramUserName)
        {
            UserId = Guid.NewGuid();
            TelegramUserName = telegramUserName;
            RegisteredAt = DateTime.Now;
        }
    }
}
