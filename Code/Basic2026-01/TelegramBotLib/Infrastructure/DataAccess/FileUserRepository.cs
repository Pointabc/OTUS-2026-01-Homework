using System.Text.Json;
using Telegram.Bot.Types;
using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Infrastructure.DataAccess
{
    internal class FileUserRepository : IUserRepository
    {
        string _userRepositoryFolder;

        public FileUserRepository(string userRepositoryFolder)
        {
            if (string.IsNullOrWhiteSpace(userRepositoryFolder))
                throw new ArgumentNullException("Некорректное имя папки для репозитория ToDoUser.");

            if (!Directory.Exists(userRepositoryFolder))
                Directory.CreateDirectory(userRepositoryFolder);

            string currentDirectory = Environment.CurrentDirectory;
            _userRepositoryFolder = Path.Combine(currentDirectory, userRepositoryFolder);

            ClearUserRepository();
        }

        /// <summary>
        /// Удалить хранилище для хранения пользователей.
        /// </summary>
        private void ClearUserRepository()
        {
            var files = Directory.GetFiles(_userRepositoryFolder);
            foreach (var file in files)
                File.Delete(file);
        }

        public async Task Add(ToDoUser user, CancellationToken cancellationToken)
        {
            string filePath = Path.Combine(_userRepositoryFolder, $"{user.UserId}.json");
            string jsonString = JsonSerializer.Serialize(user);

            await File.WriteAllTextAsync(filePath, jsonString, cancellationToken);
        }

        public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken cancellationToken)
        {
            var files = Directory.GetFiles(_userRepositoryFolder, "*.json");
            foreach (var file in files)
            {
                string json = await File.ReadAllTextAsync(file, cancellationToken);
                var toDoUser = JsonSerializer.Deserialize<ToDoUser>(json);

                if (toDoUser != null && toDoUser.UserId == userId)
                    return toDoUser;
            }

            return null;
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken cancellationToken)
        {
            var files = Directory.GetFiles(_userRepositoryFolder, "*.json");
            foreach (var file in files)
            {
                string json = await File.ReadAllTextAsync(file, cancellationToken);
                var toDoUser = JsonSerializer.Deserialize<ToDoUser>(json);

                if (toDoUser != null && toDoUser.TelegramUserId == telegramUserId)
                    return toDoUser;
            }

            return null;
        }
    }
}
