using System.Text.Json;
using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Infrastructure.DataAccess
{
    internal class FileToDoListRepository : IToDoListRepository
    {
        string _toDoListRepositoryFolder;

        public FileToDoListRepository(string toDolistRepositoryFolder)
        {
            if (!Directory.Exists(toDolistRepositoryFolder))
                throw new ArgumentException($"Папка {toDolistRepositoryFolder} для списков (категорий) задач пользователя не создана.");

            _toDoListRepositoryFolder = toDolistRepositoryFolder;
        }

        public async Task Add(ToDoList list, CancellationToken ct)
        {
            string filePath = Path.Combine(_toDoListRepositoryFolder, $"{list.Id}.json");
            string jsonString = JsonSerializer.Serialize(list);

            await File.WriteAllTextAsync(filePath, jsonString, ct);
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            string filePath = Path.Combine(_toDoListRepositoryFolder, $"{id}.json");
            File.Delete(filePath);
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            var files = Directory.GetFiles(_toDoListRepositoryFolder, "*.json", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                string json = await File.ReadAllTextAsync(file, ct);
                var toDoList = JsonSerializer.Deserialize<ToDoList>(json);

                if (toDoList != null && toDoList.User.UserId == userId && toDoList.Name == name)
                    return true;
            }

            return false;
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            var files = Directory.GetFiles(_toDoListRepositoryFolder, "*.json", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                string json = await File.ReadAllTextAsync(file, ct);
                var toDoList = JsonSerializer.Deserialize<ToDoList>(json);

                if (toDoList != null && toDoList.Id == id)
                    return toDoList;
            }

            return null;
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            var toDoLists = new List<ToDoList>();
            var files = Directory.GetFiles(_toDoListRepositoryFolder, "*.json", SearchOption.AllDirectories);

            foreach (var file in files)
            {
                string json = await File.ReadAllTextAsync(file, ct);
                var toDoList = JsonSerializer.Deserialize<ToDoList>(json);

                if (toDoList != null && toDoList.User.UserId == userId)
                    toDoLists.Add(toDoList);
            }

            return toDoLists;
        }
    }
}
