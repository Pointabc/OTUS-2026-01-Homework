
namespace TelegramBotLib
{
    internal class UserService : IUserService
    {
        List<ToDoUser> _toDoUsers = new List<ToDoUser>();

        public ToDoUser? GetUser(long telegramUserId)
        {
            return _toDoUsers.Where(x => x.TelegramUserId == telegramUserId).FirstOrDefault();
        }

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            var user = new ToDoUser(telegramUserName, telegramUserId);
            _toDoUsers.Add(user);

            return user;
        }
    }
}
