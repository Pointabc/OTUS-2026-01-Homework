using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Core.DataAccess
{
    internal interface IToDoListRepository
    {
        /// <summary>
        /// Получить список (категорию) для задач.
        /// </summary>
        /// <param name="id">Id списка (категории) задач.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Список (категория) для задач или если списка (категории) нет, то возвращает null.</returns>
        Task<ToDoList?> Get(Guid id, CancellationToken ct);

        /// <summary>
        /// Получить списки (категории) пользователя по Guid.
        /// </summary>
        /// <param name="userId">Id пользователя в Telegram.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Списки (категории) пользователя.</returns>
        Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct);

        /// <summary>
        /// Добавить список (категорию) для задач.
        /// </summary>
        /// <param name="list">Список (категории) для задач.</param>
        /// <param name="ct">Токен отмены.</param>
        Task Add(ToDoList list, CancellationToken ct);

        /// <summary>
        /// Удалить список (категорию) для задач.
        /// </summary>
        /// <param name="id">Id списка (категории) задач.</param>
        /// <param name="ct">Токен отмены.</param>
        Task Delete(Guid id, CancellationToken ct);

        /// <summary>
        /// Проверяет, если ли у пользователя список (категорию) для задач с таким именем.
        /// </summary>
        /// <param name="userId">Id пользователя в Telegram.</param>
        /// <param name="name">Название списка (категории) для задач.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns></returns>
        Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct);
    }
}