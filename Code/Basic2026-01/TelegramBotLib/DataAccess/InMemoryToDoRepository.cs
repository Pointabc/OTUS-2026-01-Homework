using TelegramBotLib.Entities;
using TelegramBotLib.DataAccess;

namespace TelegramBotLib.DataAccess
{
    internal class InMemoryToDoRepository : IToDoRepository
    {
        List<ToDoItem> _toDoItems = new List<ToDoItem>();

        public void Add(ToDoItem item)
        {
            _toDoItems.Add(item);
        }

        public int CountActive(Guid userId)
        {
            return _toDoItems.Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active).Count();
        }

        public void Delete(Guid id)
        {
            var toDoItem = Get(id);
            if (toDoItem != null)
                _toDoItems.Remove(toDoItem);
        }

        public bool ExistsByName(Guid userId, string name)
        {
            return _toDoItems.Where(x => x.User.UserId == userId && x.Name == name).Any();
        }

        public ToDoItem? Get(Guid id)
        {
            return _toDoItems.Where(x => x.Id == id).FirstOrDefault();
        }

        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return _toDoItems.Where(x => x.User.UserId == userId && x.State == ToDoItemState.Active).ToList();
        }

        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _toDoItems.Where(x => x.User.UserId == userId).ToList();
        }

        public void Update(ToDoItem item)
        {
            var toDoItem = _toDoItems.Where(x => Equals(x, item)).FirstOrDefault();
            if (toDoItem != null)
            {
                toDoItem.State = ToDoItemState.Completed;
                toDoItem.StateChangedAt = DateTime.Now;
            }
        }

        public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
        {
            return _toDoItems.Where(x => x.User.UserId == userId && predicate(x)).ToList();
        }
    }
}
