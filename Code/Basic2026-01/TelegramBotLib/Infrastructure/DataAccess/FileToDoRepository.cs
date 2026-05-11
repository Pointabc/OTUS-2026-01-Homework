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

        /// <summary>
        /// Добавить задачу.
        /// </summary>
        /// <param name="item">Задача.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        public async Task Add(ToDoItem item, CancellationToken cancellationToken)
        {
            // Проверить есть ли папка для задач пользователя, при отсутствии создать.
            var userFolderForToDoItems = Path.Combine(_toDoItemRepositoryFolder, $"{item.User.UserId}");
            if (!Path.Exists(userFolderForToDoItems))
                Directory.CreateDirectory(userFolderForToDoItems);

            string filePath = Path.Combine(userFolderForToDoItems, $"{item.Id}.json");
            string jsonString = JsonSerializer.Serialize(item);

            await File.WriteAllTextAsync(filePath, jsonString, cancellationToken);
            await _toDoRepositoryIndex.Add(item, cancellationToken);
        }

        /// <summary>
        /// Получить активные задачи пользователя.
        /// </summary>
        /// <param name="userId">ИД пользователя.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Количество активных задач пользователя.</returns>
        public async Task<int> CountActive(Guid userId, CancellationToken cancellationToken)
        {
            var tasks = await Find(userId, x => x.State == ToDoItemState.Active, cancellationToken);
            return tasks.Count;
        }

        /// <summary>
        /// Удалить задачу.
        /// </summary>
        /// <param name="id">Гуид задачи.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            var fileIndex = ((FileToDoRepositoryIndex)_toDoRepositoryIndex).GetFileIndexName();
            var files = Directory.GetFiles(_toDoItemRepositoryFolder, "*.json", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (file.EndsWith(fileIndex))
                    continue;

                string json = await File.ReadAllTextAsync(file, cancellationToken);
                var toDoItem = JsonSerializer.Deserialize<ToDoItem>(json);

                if (toDoItem != null && toDoItem.Id == id)
                {
                    File.Delete(file);
                    await _toDoRepositoryIndex.Delete(id, cancellationToken);
                    break;
                }
            }
        }

        /// <summary>
        /// Есть ли задача пользователя с имененем.
        /// </summary>
        /// <param name="userId">ИД пользователя.</param>
        /// <param name="name">Название задачи.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>True - задача есть, иначе задача не найдена.</returns>
        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken cancellationToken)
        {
            var toDoItems = await Find(userId, x => x.Name == name, cancellationToken);
            return toDoItems.Any();
        }

        /// <summary>
        /// Найти задачи пользователя.
        /// </summary>
        /// <param name="userId">ИД пользователя.</param>
        /// <param name="predicate">Условие поиска задачи пользователя.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Задачи пользователя.</returns>
        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
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

        /// <summary>
        /// Получить задачу пользователя по ИД задачи.
        /// </summary>
        /// <param name="id">ИД задачи.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Задача, иначе null.</returns>
        public async Task<ToDoItem?> Get(Guid id, CancellationToken cancellationToken)
        {
            var files = Directory.GetFiles(_toDoItemRepositoryFolder, "*.json");
            foreach (var file in files)
            {
                string json = File.ReadAllText(file);
                var toDoItem = JsonSerializer.Deserialize<ToDoItem>(json);

                if (toDoItem != null && toDoItem.Id == id)
                    return toDoItem;
            }

            return null;
        }

        /// <summary>
        /// Получить активные задачи пользователя.
        /// </summary>
        /// <param name="userId">ИД пользователя.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Активные задачи пользователя.</returns>
        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken)
        {
            var toDoItems = await Find(userId, x => x.State == ToDoItemState.Active, cancellationToken);
            return toDoItems;
        }

        /// <summary>
        /// Получить все задачи пользователя.
        /// </summary>
        /// <param name="userId">ИД пользователя.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Задачи пользователя (активные и завершенные).</returns>
        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
        {
            var toDoItems = await Find(userId, x => true, cancellationToken);
            return toDoItems;
        }

        /// <summary>
        /// Обновить задачу.
        /// </summary>
        /// <param name="item">Задача.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Обновленная задача пользователя.</returns>
        public async Task Update(ToDoItem item, CancellationToken cancellationToken)
        {
            var files = Directory.GetFiles(_toDoItemRepositoryFolder, "*.json");
            foreach (var file in files)
            {
                string json = File.ReadAllText(file);
                var toDoItem = JsonSerializer.Deserialize<ToDoItem>(json);

                if (toDoItem != null && toDoItem.Id == item.Id)
                {
                    toDoItem.State = ToDoItemState.Completed;
                    toDoItem.StateChangedAt = DateTime.Now;

                    // Перезаписать файл.
                    string jsonString = JsonSerializer.Serialize(toDoItem);
                    await File.WriteAllTextAsync(file, jsonString, cancellationToken);
                    break;
                }
            }
        }
    }
}
