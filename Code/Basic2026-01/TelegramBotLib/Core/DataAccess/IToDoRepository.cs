using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Core.DataAccess
{
    internal interface IToDoRepository
    {
        Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken);
        //Возвращает ToDoItem для UserId со статусом Active
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken);
        Task<ToDoItem?> Get(Guid id, CancellationToken cancellationToken);
        Task Add(ToDoItem item, CancellationToken cancellationToken);
        Task Update(ToDoItem item, CancellationToken cancellationToken);
        Task Delete(Guid id, CancellationToken cancellationToken);
        //Проверяет есть ли задача с таким именем у пользователя
        Task<bool> ExistsByName(Guid userId, string name, CancellationToken cancellationToken);
        //Возвращает количество активных задач у пользователя
        Task<int> CountActive(Guid userId, CancellationToken cancellationToken);
        Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken);
    }
}
