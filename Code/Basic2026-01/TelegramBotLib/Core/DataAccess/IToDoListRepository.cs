using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Core.DataAccess
{
    internal interface IToDoListRepository
    {
        /// <summary>
        /// Получить список для задач.
        /// </summary>
        /// <param name="id">Id списка задач.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Список для задач или если спика нет, то возвращает null.</returns>
        Task<ToDoList?> Get(Guid id, CancellationToken ct);

        /// <summary>
        /// Получить пользователя по Guid.
        /// </summary>
        /// <param name="userId">Id пользователя в Telegram.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Списки пользователя.</returns>
        Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct);

        /// <summary>
        /// Добавить список для задач.
        /// </summary>
        /// <param name="list">Список для задач.</param>
        /// <param name="ct">Токен отмены.</param>
        Task Add(ToDoList list, CancellationToken ct);

        /// <summary>
        /// Удалить список для задач.
        /// </summary>
        /// <param name="id">Id списка задач.</param>
        /// <param name="ct">Токен отмены.</param>
        Task Delete(Guid id, CancellationToken ct);

        /// <summary>
        /// Проверяет, если ли у пользователя список для задач с таким именем.
        /// </summary>
        /// <param name="userId">Id пользователя в Telegram.</param>
        /// <param name="name">Название списка для задач.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns></returns>
        Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct);
    }
}