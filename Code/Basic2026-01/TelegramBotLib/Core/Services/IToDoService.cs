using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Core.Services
{
    internal interface IToDoService
    {
        Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken);
        //Возвращает ToDoItem для UserId со статусом Active
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken);
        Task<ToDoItem> Add(ToDoUser user, string name, CancellationToken cancellationToken);
        Task MarkCompleted(Guid id, CancellationToken cancellationToken);
        Task Delete(Guid id, CancellationToken cancellationToken);
        Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken cancellationToken);
    }
}
