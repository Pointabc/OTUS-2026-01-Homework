using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Infrastructure.DataAccess
{
    internal class InMemoryUserRepository : IUserRepository
    {
        List<ToDoUser> _toDoUsers = new List<ToDoUser>();

        public void Add(ToDoUser user)
        {
            _toDoUsers.Add(user);
        }

        public ToDoUser? GetUser(Guid userId)
        {
            return _toDoUsers.Where(x => x.UserId == userId).FirstOrDefault();
        }

        public ToDoUser? GetUserByTelegramUserId(long telegramUserId)
        {
            return _toDoUsers.Where(x => x.TelegramUserId == telegramUserId).FirstOrDefault();
        }
    }
}
