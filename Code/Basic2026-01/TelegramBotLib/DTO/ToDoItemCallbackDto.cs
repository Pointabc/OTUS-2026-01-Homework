
namespace TelegramBotLib.DTO
{
    internal class ToDoItemCallbackDto : CallbackDto
    {
        public Guid? ToDoItemId { get; set; }

        /// <summary>
        /// Создать объект ToDoItemCallbackDto.
        /// </summary>
        /// <param name="input">Строка ввида "{action}|{toDoListId}|{prop2}.</param>
        /// <returns>Объект ToDoItemCallbackDto.</returns>
        public static new ToDoItemCallbackDto FromString(string input)
        {
            var splitInput = input.Split('|');
            var toDoItemCallbackDto = new ToDoItemCallbackDto();
            toDoItemCallbackDto.Action = splitInput.Length == 1 ? input : splitInput[0];
            toDoItemCallbackDto.ToDoItemId = splitInput.Length > 1 && splitInput[1] != string.Empty ? Guid.Parse(splitInput[1]) : null;

            return toDoItemCallbackDto;
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoItemId}";
        }
    }
}
