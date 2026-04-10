using TelegramBotLib.DataAccess;
using TelegramBotLib.Entities;

namespace TelegramBotLib.Services
{
    internal class UserService : IUserService
    {
        private IUserRepository _inMemoryUserRepository = new DataAccess.InMemoryUserRepository();

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
