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
        public async Task<ToDoItem> Add(ToDoUser user, string name, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"Описание задачи не должно быть пустым.");

            if (name.Length > _maxTaskDiscriptionLength)
                throw new TaskLengthLimitException(name.Length, _maxTaskDiscriptionLength);

            // Проверить на дубликаты.
            if (await _toDoRepository.ExistsByName(user.UserId, name, cancellationToken))
                throw new DuplicateTaskException(name);

            // Проверить на максимальное кол-во задач.
            _taskCount++;
            if (_taskCount < 1 || _taskCount > _maxNumber)
            {
                _taskCount--;
                throw new ArgumentException($"Можно создать задач: {_maxNumber}.");
            }

            var toDoItem = new ToDoItem(user, name);
            await _toDoRepository.Add(toDoItem, cancellationToken);

            return toDoItem;
        }

        /// <summary>
        /// Удалить задачу.
        /// </summary>
        /// <param name="id">Guid задачи.</param>
        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            await _toDoRepository.Delete(id, cancellationToken);
        }

        /// <summary>
        /// Получить не завершенные задачи пользователя.
        /// </summary>
        /// <param name="userId">Guid пользователя.</param>
        /// <returns>Активные задачи.</returns>
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken)
        {
            return [.. await _toDoRepository.GetActiveByUserId(userId, cancellationToken)];
        }

        /// <summary>
        /// Получить все задачи пользователя.
        /// </summary>
        /// <param name="userId">Guid пользователя.</param>
        /// <returns>Задачи пользователя.</returns>
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
        {
            return await _toDoRepository.GetAllByUserId(userId, cancellationToken);
        }

        /// <summary>
        /// Сделать задачу завершенной.
        /// </summary>
        /// <param name="id">Guid задачи.</param>
        public async Task MarkCompleted(Guid id, CancellationToken cancellationToken)
        {
            var toDoItem = await _toDoRepository.Get(id, cancellationToken);
            await _toDoRepository.Update(toDoItem, cancellationToken);
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken cancellationToken)
        {
            var userTasks = await _toDoRepository.GetAllByUserId(user.UserId, cancellationToken);
            return await _toDoRepository.Find(user.UserId, (x) => { return x.Name.StartsWith(namePrefix) && x.State == ToDoItemState.Active; }, cancellationToken);
        }
    }
}
