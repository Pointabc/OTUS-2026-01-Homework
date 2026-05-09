using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Core.DataAccess
{
    internal interface IUserRepository
    {
        Task<ToDoUser?> GetUser(Guid userId, CancellationToken cancellationToken);
        Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken cancellationToken);
        Task Add(ToDoUser user, CancellationToken cancellationToken);
    }
}
