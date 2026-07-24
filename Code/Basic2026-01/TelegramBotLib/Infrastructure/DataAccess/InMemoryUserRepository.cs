using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Infrastructure.DataAccess
{
    internal class InMemoryUserRepository : IUserRepository
    {
        List<ToDoUser> _toDoUsers = new List<ToDoUser>();

        public async Task Add(ToDoUser user, CancellationToken cancellationToken)
        {
            _toDoUsers.Add(user);
        }

        public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken cancellationToken)
        {
            return _toDoUsers.Where(x => x.UserId == userId).FirstOrDefault();
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken cancellationToken)
        {
            return _toDoUsers.Where(x => x.TelegramUserId == telegramUserId).FirstOrDefault();
        }

        public async Task<IReadOnlyList<ToDoUser>> GetUsers(CancellationToken ct)
        {
            return _toDoUsers;
        }
    }
}
