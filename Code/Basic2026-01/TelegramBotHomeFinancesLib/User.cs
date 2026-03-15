
namespace TelegramBotHomeFinancesLib
{
    internal class User
    {
        public User(string userName)
        {
            TelegramUserName = userName;
            UserId = Guid.NewGuid();
            RegisteredAt = DateTime.Now;
        }
        
        Guid UserId;
        public string TelegramUserName;
        DateTime RegisteredAt; // Когда зарегистрирован пользователь.
        string Email; // Почта для отправки информации.
        string TalkName; // Имя для общения.
    }
}
