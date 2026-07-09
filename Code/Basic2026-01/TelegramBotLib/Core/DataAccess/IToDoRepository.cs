using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Core.DataAccess
{
    internal interface IToDoRepository
    {
        /// <summary>
        /// Получить все задачи пользователя.
        /// </summary>
        /// <param name="userId">ИД пользователя.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Задачи пользователя (активные и завершенные).</returns>
        Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct);

        /// <summary>
        /// Получить активные задачи пользователя.
        /// </summary>
        /// <param name="userId">ИД пользователя.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Активные задачи пользователя.</returns>
        Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct);

        /// <summary>
        /// Получить задачу пользователя по ИД задачи.
        /// </summary>
        /// <param name="id">ИД задачи.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Задача, иначе null.</returns>
        Task<ToDoItem?> Get(Guid id, CancellationToken ct);

        /// <summary>
        /// Добавить задачу.
        /// </summary>
        /// <param name="item">Задача.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        Task Add(ToDoItem item, CancellationToken ct);

        /// <summary>
        /// Обновить статус задачи (на Задача выполнена).
        /// </summary>
        /// <param name="item">Задача.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        Task Update(ToDoItem item, CancellationToken ct);

        /// <summary>
        /// Удалить задачу.
        /// </summary>
        /// <param name="id">Гуид задачи.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        Task Delete(Guid id, CancellationToken ct);

        /// <summary>
        /// Есть ли задача пользователя с имененем.
        /// </summary>
        /// <param name="userId">ИД пользователя.</param>
        /// <param name="name">Название задачи.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>True - задача есть, иначе задача не найдена.</returns>
        Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct);

        /// <summary>
        /// Получить количество активных задач пользователя.
        /// </summary>
        /// <param name="userId">ИД пользователя.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Количество активных задач пользователя.</returns>
        Task<int> CountActive(Guid userId, CancellationToken ct);

        /// <summary>
        /// Найти задачи пользователя.
        /// </summary>
        /// <param name="userId">ИД пользователя.</param>
        /// <param name="predicate">Условие поиска задачи пользователя.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>Задачи пользователя.</returns>
        Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct);
    }
}
