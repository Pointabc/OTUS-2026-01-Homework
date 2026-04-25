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
        public async Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStats(Guid userId, CancellationToken cancellationToken)
        {
            var userTasks = await _iToDoRepository.GetAllByUserId(userId, cancellationToken);
            var totalTasks = userTasks.Count;
            var activeUserTasks = await _iToDoRepository.GetActiveByUserId(userId, cancellationToken);
            var activeTasks = activeUserTasks.Count;

            return (totalTasks, totalTasks - activeTasks, activeTasks, DateTime.Now);
        }
    }
}
