
namespace TelegramBotLib.Core.Entities
{
    /// <summary>
    /// Индекс для хранилища в виде json.
    /// </summary>
    internal class ToDoItemIndex
    {
        public Guid ToDoItemId { get; set; }
        public Guid UserId { get; set; }
    }
}
