using TelegramBotLib.Core.Entities;
using TelegramBotLib.Infrastructure.DataAccess;

namespace TelegramBotLib.Core.DataAccess
{
    internal interface IToDoRepositoryIndex
    {
        Task Add(ToDoItem item, CancellationToken cancellationToken);
        Task<bool> Find(Guid userId, Func<ToDoItemIndex, bool> predicate, CancellationToken cancellationToken);
        Task Delete(Guid id, CancellationToken cancellationToken);
    }
}
