namespace TelegramBotLib.Core.BackgroundTasks;

public interface IBackgroundTask
{
    Task Start(CancellationToken ct);
}
