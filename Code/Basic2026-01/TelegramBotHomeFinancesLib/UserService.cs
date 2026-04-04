
namespace TelegramBotHomeFinancesLib
{
    internal class UserService : IUserService
    {
        List<User> _toDoUsers = new List<User>();

        public User? GetUser(long telegramUserId)
        {
            return _toDoUsers.Where(x => x.TelegramUserId == telegramUserId).FirstOrDefault();
        }

        public User RegisterUser(long telegramUserId, string telegramUserName)
        {
            var user = new User(telegramUserName, telegramUserId);
            _toDoUsers.Add(user);

            return user;
        }
    }
}
