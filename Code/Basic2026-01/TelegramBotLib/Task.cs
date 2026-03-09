
namespace TelegramBotLib
{
    internal class Task
    {
        public static long maxNumber = 100;
        public static long maxTaskDiscriptionLength = 100;

        public string Description { get; set; }
        public long Number { get; set; }

        public Task(string description, long taskNumber)
        {
            Description = description;
            Number = taskNumber;
        }
    }
}
