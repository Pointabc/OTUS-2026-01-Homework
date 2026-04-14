using TelegramBotLib.Core.DataAccess;

namespace TelegramBotLib.Core.Services
{
    internal class ToDoReportService : IToDoReportService
    {
        IToDoRepository _iToDoRepository;
        public ToDoReportService(IToDoRepository iToDoRepository)
        {
            _iToDoRepository = iToDoRepository;
        }
        public (int total, int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
        {
            var totalTasks = _iToDoRepository.GetAllByUserId(userId).Count;
            var activeTasks = _iToDoRepository.GetActiveByUserId(userId).Count;

            return (totalTasks, totalTasks - activeTasks, activeTasks, DateTime.Now);
        }
    }
}
