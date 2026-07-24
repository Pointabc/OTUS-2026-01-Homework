using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;
using TelegramBotLib.Core.Services;

namespace TelegramBotLib.Core.BackgroundTasks;

internal class DeadlineBackgroundTask : BackgroundTask
{
    TimeSpan _resetScenarioTimeout { get; set; }
    INotificationService _notificationService { get; set; }
    IUserRepository _userRepository { get; set; }
    IToDoRepository _toDoRepository { get; set; }

    public DeadlineBackgroundTask(
        TimeSpan resetScenarioTimeout,
        INotificationService notificationService,
        IUserRepository userRepository,
        IToDoRepository toDoRepository)
        : base(resetScenarioTimeout, nameof(DeadlineBackgroundTask))
    {
        _resetScenarioTimeout = resetScenarioTimeout != TimeSpan.Zero
            ? resetScenarioTimeout
            : throw new ArgumentNullException();
        _notificationService = notificationService ?? throw new ArgumentNullException();
        _userRepository = userRepository ?? throw new ArgumentNullException();
        _toDoRepository = toDoRepository ?? throw new ArgumentNullException();
    }

    protected override async Task Execute(CancellationToken ct)
    {
        // Получить список всех пользователей
        var users = await _userRepository.GetUsers(ct);

        // Для каждого пользователя получить набор просроченных задач.
        foreach (var user in users)
        {
            var activeToDoItems = await _toDoRepository.GetActiveWithDeadline(user.UserId, DateTime.UtcNow.AddDays(-1).Date, DateTime.UtcNow.Date, ct);
            if (!activeToDoItems.Any())
                return;

            // Для каждой задачи создать нотификацию с типом.
            foreach (var toDoItem in activeToDoItems)
            {
                await _notificationService.ScheduleNotification(
                    user.UserId,
                    $"DeadLine_{toDoItem.Id}, Today_{DateOnly.FromDateTime(DateTime.UtcNow)}",
                    $"Ой! Вы пропустили дедлайн по задаче {toDoItem.Name}",
                    DateTime.UtcNow, // TODO VS Запланированная дата отправки, как определить?.
                    ct);
            }
        }
    }
}
