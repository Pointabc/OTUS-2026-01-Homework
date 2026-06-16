
namespace TelegramBotLib.DTO
{
    internal class ToDoListCallbackDto : CallbackDto
    {
        Guid? ToDoListId;

        /// <summary>
        /// Создать объект ToDoListCallbackDto.
        /// </summary>
        /// <param name="input">Строка ввида "{action}|{toDoListId}|{prop2}.</param>
        /// <returns>Объект ToDoListCallbackDto.</returns>
        public static new ToDoListCallbackDto FromString(string input)
        {
            var splitInput = input.Split('|');
            var toDoListCallbackDto = new ToDoListCallbackDto();
            toDoListCallbackDto.Action = splitInput.Length > 1 ? splitInput[0] : input;
            toDoListCallbackDto.ToDoListId = splitInput.Length > 2 ? Guid.Parse(splitInput[1]) : Guid.Empty;

            return toDoListCallbackDto;
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoListId}";
        }
    }
}
