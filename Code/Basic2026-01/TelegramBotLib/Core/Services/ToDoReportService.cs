using TelegramBotLib.Core.DataAccess;

namespace TelegramBotLib.Core.Services
{
    internal class ToDoReportService : IToDoReportService
    {
        IToDoRepository _toDoRepository;
        public ToDoReportService(IToDoRepository toDoRepository)
        {
            _toDoRepository = toDoRepository;
        }
        public async Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStats(Guid userId, CancellationToken cancellationToken)
        {
            var userTasks = await _toDoRepository.GetAllByUserId(userId, cancellationToken);
            var totalTasks = userTasks.Count;
            var activeUserTasks = await _toDoRepository.GetActiveByUserId(userId, cancellationToken);
            var activeTasks = activeUserTasks.Count;

            return (totalTasks, totalTasks - activeTasks, activeTasks, DateTime.Now);
        }
    }
}
