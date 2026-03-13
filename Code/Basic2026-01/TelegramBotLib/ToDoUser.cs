
namespace TelegramBotLib
{
    internal class ToDoUser
    {
        Guid UserId;
        public string TelegramUserName;
        DateTime RegisteredAt;

        public ToDoUser(string telegramUserName)
        {
            UserId = Guid.NewGuid();
            TelegramUserName = telegramUserName;
            RegisteredAt = DateTime.Now;
        }
    }
}
