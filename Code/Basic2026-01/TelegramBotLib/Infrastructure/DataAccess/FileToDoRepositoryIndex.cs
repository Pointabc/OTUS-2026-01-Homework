using System.Text.Json;
using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Infrastructure.DataAccess
{
    internal class FileToDoRepositoryIndex : IToDoRepositoryIndex
    {
        /// <summary>
        /// Полное имя файла-индекса.
        /// </summary>
        string _fileIndex;

        public FileToDoRepositoryIndex(string pathToFileIndex)
        {
            if (!File.Exists(pathToFileIndex))
                throw new ArgumentException($"Файл-индекс: {pathToFileIndex} не создан.");

            _fileIndex = pathToFileIndex;
        }
        public async Task Add(ToDoItem item, CancellationToken cancellationToken)
        {
            // Проверить есть ли задача в файл-индекс, если нет добавить.
            if (await Find(item.User.UserId, x => x.UserId == item.Id, cancellationToken))
                return;

            using (var writer = new StreamWriter(_fileIndex, true))
            {
                string jsonString = JsonSerializer.Serialize(new ToDoItemIndex { ToDoItemId = item.Id, UserId = item.User.UserId});
                writer.WriteLine(jsonString);
            }    
        }

        public async Task<bool> Find(Guid userId, Func<ToDoItemIndex, bool> predicate, CancellationToken cancellationToken)
        {
            using (var reader = new StreamReader(_fileIndex))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    var toDoItemIndex = JsonSerializer.Deserialize<ToDoItemIndex>(line);
                    if (toDoItemIndex != null && toDoItemIndex.UserId == userId && predicate(toDoItemIndex))
                        return true;
                }
            }

            return false;
        }

        public async Task Delete(Guid id, CancellationToken cancellationToken)
        {
            // Перемоздаем файл.
            if (File.Exists(_fileIndex))
            {
                File.Delete(_fileIndex);
                using (File.Create(_fileIndex)) { }
            }

            // Перестраиваем индекс
            await UpdateFileIndex();
        }

        /// <summary>
        /// Обновить файл-индекс.
        /// </summary>
        public async Task UpdateFileIndex()
        {
            var toDoItemIndex = new List<ToDoItemIndex>();
            var toDoItemRepositoryFolder = Path.GetDirectoryName(_fileIndex);
            var toDoItemsFiles = Directory.GetFiles(toDoItemRepositoryFolder, "*.json", SearchOption.AllDirectories);
            foreach (var file in toDoItemsFiles)
            {
                if (file == _fileIndex)
                    continue;

                string json = File.ReadAllText(file);
                var toDoItem = JsonSerializer.Deserialize<ToDoItem>(json);

                if (await Find(toDoItem.User.UserId, x => x.ToDoItemId == toDoItem.Id, CancellationToken.None))
                    continue;

                toDoItemIndex.Add(new ToDoItemIndex { ToDoItemId = toDoItem.Id, UserId = toDoItem.User.UserId });
            }

            if (!toDoItemIndex.Any())
                return;

            using (var writer = new StreamWriter(_fileIndex, true))
            {
                foreach (var item in toDoItemIndex)
                {
                    string jsonString = JsonSerializer.Serialize(item);
                    writer.WriteLine(jsonString);
                }
            }
        }

        /// <summary>
        /// Получить имя файла-индекса.
        /// </summary>
        /// <returns>Имя файла-индекса.</returns>
        public string GetFileIndexName()
        {
            return Path.GetFileName(_fileIndex);
        }
    }
}
