using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Core.Services
{
    internal interface IUserService
    {
        Task<ToDoUser> RegisterUser(long telegramUserId, string telegramUserName, CancellationToken cancellationToken);
        Task<ToDoUser?> GetUser(long telegramUserId, CancellationToken cancellationToken);
    }
}
