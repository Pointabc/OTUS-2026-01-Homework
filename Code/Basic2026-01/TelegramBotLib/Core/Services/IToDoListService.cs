using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Core.Services
{
    internal interface IToDoListService
    {
        /// <summary>
        /// Добавить список (категорию) для задач.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <param name="name">Название списка (категории) для задач.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns></returns>
        Task<ToDoList> Add(ToDoUser user, string name, CancellationToken ct);

        /// <summary>
        /// Получить список (категорию) для задач.
        /// </summary>
        /// <param name="id">ИД списка (категории) для задач.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Список (категория) для задач.</returns>
        Task<ToDoList?> Get(Guid id, CancellationToken ct);

        /// <summary>
        /// Удалить список (категорию) для задач.
        /// </summary>
        /// <param name="id">ИД списка (категории) для задач.</param>
        /// <param name="ct">Токен отмены.</param>
        Task Delete(Guid id, CancellationToken ct);

        /// <summary>
        /// Получить списки (категории) для задачь пользователя.
        /// </summary>
        /// <param name="userId">ИД пользователя.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns>Список списков (категорий) для задач пользователя.</returns>
        Task<IReadOnlyList<ToDoList>> GetUserLists(Guid userId, CancellationToken ct);
    }
}
