using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Infrastructure.DataAccess
{
    internal class InMemoryToDoRepository : IToDoRepository
    {
        List<ToDoItem> _toDoItems = new List<ToDoItem>();

        public async Task Add(ToDoItem item, CancellationToken cancellationToken)
        {
            _toDoItems.Add(item);
        }

        public async Task<int> CountActive(Guid userId, CancellationToken cancellationToken)
        {
            return _toDoItems.Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active).Count();
        }

        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            var toDoItem = await Get(id, cancellationToken);
            if (toDoItem != null)
                _toDoItems.Remove(toDoItem);
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken cancellationToken)
        {
            return _toDoItems.Where(x => x.User.UserId == userId && x.Name == name).Any();
        }

        public async Task<ToDoItem?> Get(Guid id, CancellationToken cancellationToken)
        {
            return _toDoItems.Where(x => x.Id == id).FirstOrDefault();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken)
        {
            return _toDoItems.Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active).ToList();
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
        {
            return _toDoItems.Where(x => x.User.UserId == userId).ToList();
        }

        public async Task Update(ToDoItem item, CancellationToken cancellationToken)
        {
            var toDoItem = _toDoItems.Where(x => Equals(x, item)).FirstOrDefault();
            if (toDoItem != null)
            {
                toDoItem.State = ToDoItemState.Completed;
                toDoItem.StateChangedAt = DateTime.Now;
            }
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
        {
            return _toDoItems.Where(x => x.User.UserId == userId && predicate(x)).ToList();
        }
    }
}