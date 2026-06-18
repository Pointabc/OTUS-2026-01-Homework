
namespace TelegramBotLib.DTO
{
    internal class ToDoItemCallbackDto : CallbackDto
    {
        public Guid ToDoItemId { get; set; }

        /// <summary>
        /// Создать объект ToDoItemCallbackDto.
        /// </summary>
        /// <param name="input">Строка ввида "{action}|{toDoListId}|{prop2}.</param>
        /// <returns>Объект ToDoItemCallbackDto.</returns>
        public static new ToDoItemCallbackDto FromString(string input)
        {
            var splitInput = input.Split('|');
            var toDoItemCallbackDto = new ToDoItemCallbackDto();
            toDoItemCallbackDto.Action = splitInput.Length > 1 ? splitInput[0] : input;
            toDoItemCallbackDto.ToDoItemId = splitInput.Length > 2 ? Guid.Parse(splitInput[1]) : Guid.Empty;

            return toDoItemCallbackDto;
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoItemId}";
        }
    }
}
