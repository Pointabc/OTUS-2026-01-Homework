
namespace TelegramBotHomeFinancesLib
{
    internal class UserService : IUserService
    {
        List<User> _users = new List<User>();

        public User? GetUser(long telegramUserId)
        {
            return _users.Where(x => x.TelegramUserId == telegramUserId).FirstOrDefault();
        }

        public User RegisterUser(long telegramUserId, string telegramUserName)
        {
            var user = new User(telegramUserName, telegramUserId);
            _users.Add(user);

            return user;
        }
    }
}
