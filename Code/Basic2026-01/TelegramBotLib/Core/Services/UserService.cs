using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;
using TelegramBotLib.Infrastructure.DataAccess;

namespace TelegramBotLib.Core.Services
{
    internal class UserService : IUserService
    {
        private IUserRepository _inMemoryUserRepository = new InMemoryUserRepository();

        public async Task<ToDoUser?> GetUser(long telegramUserId, CancellationToken cancellationToken)
        {
            return _inMemoryUserRepository.GetUserByTelegramUserId(telegramUserId);
        }

        public async Task<ToDoUser> RegisterUser(long telegramUserId, string telegramUserName, CancellationToken cancellationToken)
        {
            var user = new ToDoUser(telegramUserName, telegramUserId);
            _inMemoryUserRepository.Add(user);

            return user;
        }
    }
}
