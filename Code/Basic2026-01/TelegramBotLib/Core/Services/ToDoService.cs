using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;
using TelegramBotLib.Core.Exceptions;

namespace TelegramBotLib.Core.Services
{
    internal class ToDoService : IToDoService
    {
        IToDoRepository _toDoRepository;
        long _taskCount = 0;
        long _maxNumber = 20;
        long _maxTaskDiscriptionLength = 100;

        public ToDoService(IToDoRepository toDoRepository)
        {
            _toDoRepository = toDoRepository;
        }

        /// <summary>
        /// Добавить задачу.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <param name="name">Описание задачи.</param>
        /// <returns>Задача.</returns>
        public ToDoItem Add(ToDoUser user, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"Описание задачи не должно быть пустым.");

            if (name.Length > _maxTaskDiscriptionLength)
                throw new TaskLengthLimitException(name.Length, _maxTaskDiscriptionLength);

            // Проверить на дубликаты.
            if (_toDoRepository.ExistsByName(user.UserId, name))
                throw new DuplicateTaskException(name);

            // Проверить на максимальное кол-во задач.
            _taskCount++;
            if (_taskCount < 1 || _taskCount > _maxNumber)
            {
                _taskCount--;
                throw new ArgumentException($"Можно создать задач: {_maxNumber}.");
            }

            var toDoItem = new ToDoItem(user, name);
            _toDoRepository.Add(toDoItem);

            return toDoItem;
        }

        /// <summary>
        /// Удалить задачу.
        /// </summary>
        /// <param name="id">Guid задачи.</param>
        public void Delete(Guid id)
        {
            _toDoRepository.Delete(id);
        }

        /// <summary>
        /// Получить не завершенные задачи пользователя.
        /// </summary>
        /// <param name="userId">Guid пользователя.</param>
        /// <returns>Активные задачи.</returns>
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return [.. _toDoRepository.GetActiveByUserId(userId)];
        }

        /// <summary>
        /// Получить все задачи пользователя.
        /// </summary>
        /// <param name="userId">Guid пользователя.</param>
        /// <returns>Задачи пользователя.</returns>
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _toDoRepository.GetAllByUserId(userId);
        }

        /// <summary>
        /// Сделать задачу завершенной.
        /// </summary>
        /// <param name="id">Guid задачи.</param>
        public void MarkCompleted(Guid id)
        {
            var toDoItem = _toDoRepository.Get(id);
            //var task = _toDoItems.Where(x => x.Id == id).FirstOrDefault();
            _toDoRepository.Update(toDoItem);
        }

        public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
        {
            var userTasks = _toDoRepository.GetAllByUserId(user.UserId);
            return _toDoRepository.Find(user.UserId, (x) => { return x.Name.StartsWith(namePrefix) && x.State == ToDoItemState.Active; });
        }
    }
}
