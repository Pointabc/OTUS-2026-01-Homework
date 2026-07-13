using System.Text.Json;
using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Infrastructure.DataAccess
{
    internal class FileToDoRepository : IToDoRepository
    {
        /// <summary>
        /// Папка пользователя для хранения задач пользователя.
        /// </summary>
        string _toDoItemRepositoryFolder;
        IToDoRepositoryIndex _toDoRepositoryIndex;

        public FileToDoRepository(string toDoItemRepositoryFolder, IToDoRepositoryIndex toDoRepositoryIndex)
        {
            if (!Directory.Exists(toDoItemRepositoryFolder))
                throw new ArgumentException($"Папка {toDoItemRepositoryFolder} для репозитория задач не создана.");

            _toDoItemRepositoryFolder = toDoItemRepositoryFolder;
            _toDoRepositoryIndex = toDoRepositoryIndex;
        }

        public async Task Add(ToDoItem item, CancellationToken ct)
        {
            // Проверить есть ли папка для задач пользователя, при отсутствии создать.
            var userFolderForToDoItems = Path.Combine(_toDoItemRepositoryFolder, $"{item.User.UserId}");
            if (!Path.Exists(userFolderForToDoItems))
                Directory.CreateDirectory(userFolderForToDoItems);

            string filePath = Path.Combine(userFolderForToDoItems, $"{item.Id}.json");
            string jsonString = JsonSerializer.Serialize(item);

            await File.WriteAllTextAsync(filePath, jsonString, ct);
            await _toDoRepositoryIndex.Add(item, ct);
        }

        public async Task<int> CountActive(Guid userId, CancellationToken ct)
        {
            var tasks = await Find(userId, x => x.State == ToDoItemState.Active, ct);
            return tasks.Count;
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            var fileIndex = ((FileToDoRepositoryIndex)_toDoRepositoryIndex).GetFileIndexName();
            var files = Directory.GetFiles(_toDoItemRepositoryFolder, "*.json", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (file.EndsWith(fileIndex))
                    continue;

                string json = await File.ReadAllTextAsync(file, ct);
                var toDoItem = JsonSerializer.Deserialize<ToDoItem>(json);

                if (toDoItem != null && toDoItem.Id == id)
                {
                    File.Delete(file);
                    await _toDoRepositoryIndex.Delete(id, ct);
                    break;
                }
            }
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            var toDoItems = await Find(userId, x => x.Name == name, ct);
            return toDoItems.Any();
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            var toDoItems = new List<ToDoItem>();
            var userFolderForToDoItems = Path.Combine(_toDoItemRepositoryFolder, $"{userId}");
            if (!Path.Exists(userFolderForToDoItems))
                return toDoItems;

            var files = Directory.GetFiles(userFolderForToDoItems, "*.json");
            foreach (var file in files)
            {
                string json = File.ReadAllText(file);
                var toDoItem = JsonSerializer.Deserialize<ToDoItem>(json);

                if (toDoItem != null && toDoItem.User.UserId == userId && predicate(toDoItem))
                    toDoItems.Add(toDoItem);
            }

            return toDoItems;
        }

        public async Task<ToDoItem?> Get(Guid id, CancellationToken ct)
        {
            var files = Directory.GetFiles(_toDoItemRepositoryFolder, "*.json", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (file.EndsWith("fileIndex.json"))
                    continue;

                string json = File.ReadAllText(file);
                var toDoItem = JsonSerializer.Deserialize<ToDoItem>(json);

                if (toDoItem != null && toDoItem.Id == id)
                    return toDoItem;
            }

            return null;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            var toDoItems = await Find(userId, x => x.State == ToDoItemState.Active, ct);
            return toDoItems;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            var toDoItems = await Find(userId, x => true, ct);
            return toDoItems;
        }

        public async Task Update(ToDoItem item, CancellationToken ct)
        {
            // TODO VS Искать задачи только в папке пользователя, сейчас ищет во всех папках.
            var files = Directory.GetFiles(_toDoItemRepositoryFolder, "*.json", SearchOption.AllDirectories);
            var fileIndex = ((FileToDoRepositoryIndex)_toDoRepositoryIndex).GetFileIndexName();
            foreach (var file in files)
            {
                if (file.EndsWith(fileIndex))
                    continue;

                string json = File.ReadAllText(file);
                var toDoItem = JsonSerializer.Deserialize<ToDoItem>(json);

                if (toDoItem != null && toDoItem.Id == item.Id)
                {
                    toDoItem.State = ToDoItemState.Completed;
                    toDoItem.StateChangedAt = DateTime.Now;

                    // Перезаписать файл.
                    string jsonString = JsonSerializer.Serialize(toDoItem);
                    await File.WriteAllTextAsync(file, jsonString, ct);
                    break;
                }
            }
        }
    }
}
