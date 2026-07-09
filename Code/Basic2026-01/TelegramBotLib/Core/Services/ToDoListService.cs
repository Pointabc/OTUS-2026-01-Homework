using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;
using TelegramBotLib.Core.Exceptions;

namespace TelegramBotLib.Core.Services
{
    internal class ToDoListService : IToDoListService
    {
        IToDoListRepository _toDoListRepository;
        long _maxToDoListNameLength = 10;

        public ToDoListService(IToDoListRepository toDoListRepository)
        {
            _toDoListRepository = toDoListRepository;
        }

        public async Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException($"Наименование списка (категории) для задачи не должно быть пустым.");

            // Проверить на длину наименования списка (категории) для задач.
            if (name.Length > _maxToDoListNameLength)
                throw new ArgumentException($"Длина наименования списка (категории) для задачи не должно быть более {_maxToDoListNameLength} симаолов.");

            // Проверить на дубликаты.
            if (await _toDoListRepository.ExistsByName(user.UserId, name, ct))
                throw new ArgumentException($"Cписка (категории) для задач с наименованием {name} уже создан.");

            var toDoList = new ToDoList
            {
                Name = name,
                User = user
            };
            await _toDoListRepository.Add(toDoList, ct);

            return toDoList;
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            await _toDoListRepository.Delete(id, ct);
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            return await _toDoListRepository.Get(id, ct);
        }

        public async Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct)
        {
            return await _toDoListRepository.GetByUserId(userId, ct);
        }
    }
}
