using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;
using TelegramBotLib.Infrastructure.DataAccess;
using TelegramBotLib.TelegramBot;

namespace TelegramBotLib.Core.Services
{
    internal class UserService : IUserService
    {
        //private IUserRepository _inMemoryUserRepository = new InMemoryUserRepository();
        private IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<ToDoUser?> GetUser(long telegramUserId, CancellationToken cancellationToken)
        {
            return await _userRepository.GetUserByTelegramUserId(telegramUserId, cancellationToken);
        }

        public async Task<ToDoUser> RegisterUser(long telegramUserId, string telegramUserName, CancellationToken cancellationToken)
        {
            var user = new ToDoUser
            {
                TelegramUserName = telegramUserName,
                TelegramUserId = telegramUserId,
                UserId = Guid.NewGuid(),
            };
            await _userRepository.Add(user, cancellationToken);

            return user;
        }
    }
}