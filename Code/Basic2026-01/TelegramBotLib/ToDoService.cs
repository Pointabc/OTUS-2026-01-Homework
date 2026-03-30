using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using static System.Console;

//enum AddTask { TaskNeedDiscription, TaskNull }

namespace TelegramBotLib
{
    internal class ToDoService : IToDoService
    {
        List<ToDoItem> _toDoItems = new List<ToDoItem>();
        long _taskCount = 0;

        static long _maxNumber = 2;
        static long _maxTaskDiscriptionLength = 2;

        /// <summary>
        /// Добавить задачу.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <param name="name">Описание задачи.</param>
        /// <returns>Задача.</returns>
        public ToDoItem Add(ToDoUser user, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            if (name.Length > _maxTaskDiscriptionLength)
                throw new TaskLengthLimitException(name.Length, _maxTaskDiscriptionLength);

            // Проверить на максимальное кол-во задач.
            _taskCount++;
            if (_taskCount < 1 || _taskCount > _maxNumber)
            {
                _taskCount--;
                throw new ArgumentException($"Можно создать задач: {_maxNumber}.");
            }

            var toDoItem = new ToDoItem(user, name);
            _toDoItems.Add(toDoItem);
            return toDoItem;
        }

        /// <summary>
        /// Удалить задачу.
        /// </summary>
        /// <param name="id">Guid задачи.</param>
        public void Delete(Guid id)
        {
            var task = _toDoItems.Where(x => x.Id == id).FirstOrDefault();
            if (task != null)
                _toDoItems.Remove(task);
        }

        /// <summary>
        /// Получить не завершенные задачи пользователя.
        /// </summary>
        /// <param name="userId">Guid пользователя.</param>
        /// <returns>Активные задачи.</returns>
        public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
        {
            return [.. _toDoItems.Where(x => x.State == ToDoItemState.Active)];
        }

        /// <summary>
        /// Получить все задачи пользователя.
        /// </summary>
        /// <param name="userId">Guid пользователя.</param>
        /// <returns>Задачи пользователя.</returns>
        public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
        {
            return _toDoItems;
        }

        /// <summary>
        /// Сделать задачу завершенной.
        /// </summary>
        /// <param name="id">Guid задачи.</param>
        public void MarkCompleted(Guid id)
        {
            var task = _toDoItems.Where(x => x.Id == id).FirstOrDefault();
            if (task != null)
            {
                task.State = ToDoItemState.Completed;
                task.StateChangedAt = DateTime.Now;
            }
        }
    }
}
