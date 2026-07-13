using Telegram.Bot.Types;
using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;
using TelegramBotLib.Core.Exceptions;

namespace TelegramBotLib.Core.Services
{
    internal class ToDoService : IToDoService
    {
        IToDoRepository _toDoRepository;
        IToDoListService _toDoListService;
        long _taskCount = 0;
        long _maxNumber = 20;
        long _maxTaskDiscriptionLength = 100;

        public ToDoService(IToDoRepository toDoRepository, IToDoListService toDoListService)
        {
            _toDoRepository = toDoRepository;
            _toDoListService = toDoListService;
        }

        public async Task<ToDoItem> Add(ToDoUser user, string name, DateTime deadline, ToDoList? list, CancellationToken cancellationToken)
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

            var toDoItem = new ToDoItem
            {
                User = user,
                Name = name,
                StateChangedAt = DateTime.Now,
                Id = Guid.NewGuid(),
            };
            await _toDoRepository.Add(toDoItem, cancellationToken);

            return toDoItem;
        }

        public async Task<ToDoItem> Add(ToDoItem toDoItem, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(toDoItem.Name))
                throw new ArgumentException($"Описание задачи не должно быть пустым.");

            if (toDoItem.Name.Length > _maxTaskDiscriptionLength)
                throw new TaskLengthLimitException(toDoItem.Name.Length, _maxTaskDiscriptionLength);

            // Проверить на дубликаты.
            if (await _toDoRepository.ExistsByName(toDoItem.User.UserId, toDoItem.Name, ct))
                throw new DuplicateTaskException(toDoItem.Name);

            // Проверить на максимальное кол-во задач.
            _taskCount++;
            if (_taskCount < 1 || _taskCount > _maxNumber)
            {
                _taskCount--;
                throw new ArgumentException($"Можно создать задач: {_maxNumber}.");
            }

            await _toDoRepository.Add(toDoItem, ct);

            return toDoItem;
        }

        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            await _toDoRepository.Delete(id, cancellationToken);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken)
        {
            return [.. await _toDoRepository.GetActiveByUserId(userId, cancellationToken)];
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
        {
            return await _toDoRepository.GetAllByUserId(userId, cancellationToken);
        }

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

        public async Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct)
        {
            var userLists = await _toDoListService.GetUserLists(userId, ct);
            var userList = userLists.Where(x => x.Id == listId).FirstOrDefault();
            return await _toDoRepository.Find(userId, x => x.List != null && userList != null && userList.Id == x.List.Id, ct);
        }

        public async Task<ToDoItem?> Get(Guid toDoItemId, CancellationToken ct)
        {
            return await _toDoRepository.Get(toDoItemId, ct);
        }
    }
}
