using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Core.DataAccess
{
    internal interface IUserRepository
    {
        ToDoUser? GetUser(Guid userId);
        ToDoUser? GetUserByTelegramUserId(long telegramUserId);
        void Add(ToDoUser user);
    }
}
