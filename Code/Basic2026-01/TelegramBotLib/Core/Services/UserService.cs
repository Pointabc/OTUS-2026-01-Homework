using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;
using TelegramBotLib.Infrastructure.DataAccess;

namespace TelegramBotLib.Core.Services
{
    internal class UserService : IUserService
    {
        private IUserRepository _inMemoryUserRepository = new InMemoryUserRepository();

        public ToDoUser? GetUser(long telegramUserId)
        {
            return _inMemoryUserRepository.GetUserByTelegramUserId(telegramUserId);
        }

        public ToDoUser RegisterUser(long telegramUserId, string telegramUserName)
        {
            var user = new ToDoUser(telegramUserName, telegramUserId);
            _inMemoryUserRepository.Add(user);

            return user;
        }
    }
}
