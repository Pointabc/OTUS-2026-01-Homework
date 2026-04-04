
namespace TelegramBotHomeFinancesLib
{
    internal class User
    {
        public User(string telegramUserName, long telegramUserId)
        {
            TelegramUserId = telegramUserId;
            UserId = Guid.NewGuid();
            TelegramUserName = telegramUserName;
            RegisteredAt = DateTime.Now;
        }

        Guid UserId;
        public long TelegramUserId { get; set; }
        public string TelegramUserName;
        DateTime RegisteredAt; // Когда зарегистрирован пользователь.
        string? Email; // Почта для отправки информации.
        string? TalkName; // Имя для общения.
    }
}
