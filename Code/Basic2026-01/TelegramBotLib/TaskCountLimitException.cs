
namespace TelegramBotLib
{
    internal class TaskCountLimitException : Exception
    {
        long _taskCountLimit;

        public TaskCountLimitException(long taskCountLimit)
        {
            _taskCountLimit = taskCountLimit;
        }

        public override string Message
        {
            get
            {
                return $"Превышено максимальное количество задач равное {_taskCountLimit}.";
            }
        }
    }
}
