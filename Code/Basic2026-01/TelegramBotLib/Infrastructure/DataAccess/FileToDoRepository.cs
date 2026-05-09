using System.Text.Json;
using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Infrastructure.DataAccess
{
    internal class FileToDoRepository : IToDoRepository
    {
        string _repositoryFolder;

        public FileToDoRepository(string toDoItemRepositoryFolder)
        {
            if (string.IsNullOrWhiteSpace(toDoItemRepositoryFolder))
                throw new ArgumentNullException("Некорректное имя папки для репозитория ToDoItem.");

            if (!Directory.Exists(toDoItemRepositoryFolder))
                Directory.CreateDirectory(toDoItemRepositoryFolder);

            string currentDirectory = Environment.CurrentDirectory;
            _repositoryFolder = Path.Combine(currentDirectory, toDoItemRepositoryFolder);

            ClearToDoItemRepository();
        }

        /// <summary>
        /// Удалить хранилище для хранения задач.
        /// </summary>
        private void ClearToDoItemRepository()
        {
            var files = Directory.GetFiles(_repositoryFolder);
            foreach (var file in files)
                File.Delete(file);
        }

        public async Task Add(ToDoItem item, CancellationToken cancellationToken)
        {
            string filePath = Path.Combine(_repositoryFolder, $"{item.Id}.json");
            string jsonString = JsonSerializer.Serialize(item);

            await File.WriteAllTextAsync(filePath, jsonString, cancellationToken);
        }

        public async Task<int> CountActive(Guid userId, CancellationToken cancellationToken)
        {
            var tasks = await Find(userId, x => x.State == ToDoItemState.Active && x.User.UserId == userId, cancellationToken);
            return tasks.Count;
        }

        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            var files = Directory.GetFiles(_repositoryFolder, "*.json");
            foreach (var file in files)
            {
                string json = await File.ReadAllTextAsync(file, cancellationToken);
                var toDoItem = JsonSerializer.Deserialize<ToDoItem>(json);

                if (toDoItem != null && toDoItem.Id == id)
                {
                    File.Delete(file);
                    break;
                }
            }
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken cancellationToken)
        {
            var toDoItems = await Find(userId, x => x.Name == name, cancellationToken);
            return toDoItems.Any();
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken cancellationToken)
        {
            var toDoItems = new List<ToDoItem>();
            var files = Directory.GetFiles(_repositoryFolder, "*.json");
            foreach (var file in files)
            {
                string json = File.ReadAllText(file);
                var toDoItem = JsonSerializer.Deserialize<ToDoItem>(json);

                if (toDoItem != null && toDoItem.User.UserId == userId && predicate(toDoItem))
                    toDoItems.Add(toDoItem);
            }

            return toDoItems;
        }

        public async Task<ToDoItem?> Get(Guid id, CancellationToken cancellationToken)
        {
            var files = Directory.GetFiles(_repositoryFolder, "*.json");
            foreach (var file in files)
            {
                string json = File.ReadAllText(file);
                var toDoItem = JsonSerializer.Deserialize<ToDoItem>(json);

                if (toDoItem != null && toDoItem.Id == id)
                    return toDoItem;
            }

            return null;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken)
        {
            var toDoItems = await Find(userId, x => x.State == ToDoItemState.Active, cancellationToken);
            return toDoItems;
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken)
        {
            var toDoItems = await Find(userId, x => true, cancellationToken);
            return toDoItems;
        }

        public async Task Update(ToDoItem item, CancellationToken cancellationToken)
        {
            var files = Directory.GetFiles(_repositoryFolder, "*.json");
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
