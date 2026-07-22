using LinqToDB;
using LinqToDB.Async;
using TelegramBotLib.Core.Entities;
using TelegramBotLib.Core.Services;
using TelegramBotLib.Infrastructure.DataAccess;
using TelegramBotLib.Infrastructure.DataAccess.Models;

namespace TelegramBotLib.Infrastructure;

internal class NotificationService : INotificationService
{
    IDataContextFactory<ToDoDataContext> _factory;

    public NotificationService(IDataContextFactory<ToDoDataContext> factory)
    {
        _factory = factory ?? throw new ArgumentNullException();
    }

    public async Task<IReadOnlyList<Notification>> GetScheduledNotification(DateTime scheduledBefore, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            var models = await dbContext.Notifications
                .LoadWith(i => i.User)
                .Where(x => !x.IsNotified && x.ScheduledAt <= scheduledBefore)
                .ToListAsync(ct);

            return models.Select(ModelMapper.MapFromModel).ToList();
        }
    }

    public async Task MarkNotified(Guid notificationId, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            // Загружаем существующую запись
            var notifiedModelFinded = await dbContext.Notifications
                .LoadWith(i => i.User)
                .FirstOrDefaultAsync(x => x.Id == notificationId, ct);

            if (notifiedModelFinded == null)
                throw new InvalidOperationException($"Уведомление с Id {notificationId} не найдена.");

            notifiedModelFinded.IsNotified = true;

            await dbContext.UpdateAsync(notifiedModelFinded, token: ct);
        }
    }

    public async Task<bool> ScheduleNotification(Guid userId, string type, string text, DateTime scheduledAt, CancellationToken ct)
    {
        using (var dbContext = _factory.CreateDataContext())
        {
            // Загружаем существующую запись
            var notifiedModelFinded = await dbContext.Notifications
                .AnyAsync(x => x.UserId == userId && x.Type == type && x.ScheduledAt == scheduledAt, ct);

            // Создать уведомление в БД.
            if (notifiedModelFinded == null)
            {
                var notificationModel = new NotificationModel
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    Type = type,
                    Text = text,
                    ScheduledAt = scheduledAt,
                    IsNotified = false
                };

                await dbContext.InsertAsync(notificationModel, token: ct);
                return true;
            }

            return false;
        }
    }
}
