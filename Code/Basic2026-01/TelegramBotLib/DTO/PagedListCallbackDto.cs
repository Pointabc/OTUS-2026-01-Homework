
namespace TelegramBotLib.DTO
{
    internal class PagedListCallbackDto : ToDoListCallbackDto
    {
        public int Page { get; set; }

        public static new PagedListCallbackDto FromString(string input)
        {
            var splitInput = input.Split('|');
            var pagedListCallbackDto = new PagedListCallbackDto();
            pagedListCallbackDto.Action = splitInput.Length == 1 ? input : splitInput[0];
            pagedListCallbackDto.ToDoListId = splitInput.Length > 1 ? Guid.Parse(splitInput[1]) : null;
            pagedListCallbackDto.Page = splitInput.Length > 2 ? Convert.ToInt32(splitInput[2]) : 0;

            return pagedListCallbackDto;
        }

        public override string ToString()
        {
            return $"{base.ToString()}|{Page}";
        }
    }
}
