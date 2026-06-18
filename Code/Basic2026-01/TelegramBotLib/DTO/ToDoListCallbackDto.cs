
namespace TelegramBotLib.DTO
{
    internal class ToDoListCallbackDto : CallbackDto
    {
        public Guid? ToDoListId;

        /// <summary>
        /// Создать объект ToDoListCallbackDto.
        /// </summary>
        /// <param name="input">Строка ввида "{action}|{toDoListId}|{prop2}.</param>
        /// <returns>Объект ToDoListCallbackDto.</returns>
        public static new ToDoListCallbackDto FromString(string input)
        {
            var splitInput = input.Split('|');
            var toDoListCallbackDto = new ToDoListCallbackDto();
            toDoListCallbackDto.Action = splitInput.Length == 1 ? input : splitInput[0];
            toDoListCallbackDto.ToDoListId = splitInput.Length > 1 ? Guid.Parse(splitInput[1]) : null;

            return toDoListCallbackDto;
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{ToDoListId}";
        }
    }
}
