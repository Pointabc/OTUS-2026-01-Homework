
namespace TelegramBotLib
{
    internal class TaskLengthLimitException : Exception
    {
        long _taskLength;
        long _taskLengthLimit;

        public TaskLengthLimitException(long taskLength, long taskLengthLimit)
        {
            _taskLength = taskLength;
            _taskLengthLimit = taskLengthLimit;
        }

        public override string Message
        {
            get
            {
                return $"Длина задачи {_taskLength} превышает максимально допустимое значение {_taskLengthLimit}.";
            }
        }
    }
}
