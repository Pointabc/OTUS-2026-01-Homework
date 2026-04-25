namespace TelegramBotLib.Core.Services
{
    internal interface IToDoReportService
    {
        Task<(int total, int completed, int active, DateTime generatedAt)> GetUserStats(Guid userId, CancellationToken cancellationToken);
    }
}
