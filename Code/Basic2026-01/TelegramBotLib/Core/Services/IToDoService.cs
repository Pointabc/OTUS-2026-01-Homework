using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Core.Services
{
    internal interface IToDoService
    {
        /// <summary>
        /// Получить задачи пользователя.
        /// </summary>
        /// <param name="userId">Id пользователя в Telegram.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Задачи пользователя.</returns>
        Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken cancellationToken);

        /// <summary>
        /// Получить задачи пользователя со статусом в работе.
        /// </summary>
        /// <param name="userId">Id пользователя в Telegram.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Возвращает задачи пользователя со статусом в работе.</returns>
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken cancellationToken);

        /// <summary>
        /// Добавить задачу.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <param name="name">Название задачи.</param>
        /// <param name="deadline">Крайний срок выполнения задачи.</param>
        /// <param name="deadline">Список задачи.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Задача.</returns>
        Task<ToDoItem> Add(ToDoUser user, string name, DateTime deadline, ToDoList? list, CancellationToken cancellationToken);

        /// <summary>
        /// Установить статус задачи на завершено.
        /// </summary>
        /// <param name="id">Guid задачи.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        Task MarkCompleted(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Удалить задачу.
        /// </summary>
        /// <param name="id">Guid задачи.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        Task Delete(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Получить задачи пользователя начинающиеся на префикс.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <param name="namePrefix">Задача начинается на префикс.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Задачи пользователя начинающиеся на префикс.</returns>
        Task<IReadOnlyList<ToDoItem>> Find(ToDoUser user, string namePrefix, CancellationToken cancellationToken);

        /// <summary>
        /// Получить задачи пользователя по списку (категории) для задач.
        /// </summary>
        /// <param name="userId">Гуид пользователя.</param>
        /// <param name="listId">Гуид списка (категории) для задач.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Задачи пользователя по списку (категории) для задач.</returns>
        Task<IReadOnlyList<ToDoItem>> GetByUserIdAndList(Guid userId, Guid? listId, CancellationToken ct);
    }
}
