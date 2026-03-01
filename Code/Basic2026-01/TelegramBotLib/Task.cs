
namespace TelegramBotLib
{
    internal class Task
    {
        public Task(string description, long taskNumber)
        {
            Description = description;
            Number = taskNumber;
        }

        public string Description { get; set; }
        public long Number { get; set; }
    }
}
