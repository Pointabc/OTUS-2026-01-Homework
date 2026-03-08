
namespace TelegramBotLib
{
    internal class DuplicateTaskException : Exception
    {
        string _taskDescription;

        public DuplicateTaskException(string taskDescription)
        {
            _taskDescription = taskDescription;
        }

        public override string Message
        {
            get
            {
                return $"Задача {_taskDescription} уже существует.";
            }
        }
    }
}
