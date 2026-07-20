using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;
using TelegramBotLib.Core.Services;

namespace TelegramBotLib.Core.BackgroundTasks;

internal class TodayBackgroundTask : BackgroundTask
{
    TimeSpan _resetScenarioTimeout { get; set; }
    INotificationService _notificationService { get; set; }
    IUserRepository _userRepository { get; set; }
    IToDoRepository _toDoRepository { get; set; }

    public TodayBackgroundTask(
        TimeSpan resetScenarioTimeout,
        INotificationService notificationService,
        IUserRepository userRepository,
        IToDoRepository toDoRepository)
        : base(resetScenarioTimeout, nameof(TodayBackgroundTask))
    {
        _notificationService = notificationService;
        _userRepository = userRepository;
        _toDoRepository = toDoRepository;
    }

    protected override async Task Execute(CancellationToken ct)
    {
        // Получить список всех пользователей
        var users = await _userRepository.GetUsers(ct);
        // Для каждого пользователя получить набор задач на сегодня
        // Для каждого пользователя создать нотификацию с типом $"Today_{DateOnly.FromDateTime(DateTime.UtcNow)}".В тексте перечислить список задач
        foreach (var user in users)
        {
            var activeToDoItems = await _toDoRepository.Find(user.UserId, (x) => { return x.Deadline.Date == DateTime.UtcNow.Date && x.State == ToDoItemState.Active; }, ct);
            
            // Для каждой задачи создать нотификацию с типом.
            foreach (var toDoItem in activeToDoItems)
            {
                
                await _notificationService.ScheduleNotification(
                    user.UserId,
                    $"Today_{DateOnly.FromDateTime(DateTime.UtcNow)}",
                    $"Задачу {toDoItem.Name} нужно выполнить сегодня.",
                    DateTime.UtcNow, // TODO VS Запланированная дата отправки, как определить?.
                    ct);
            }
        }
    }
}
