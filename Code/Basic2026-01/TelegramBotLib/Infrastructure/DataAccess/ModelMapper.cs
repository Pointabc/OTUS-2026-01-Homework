using TelegramBotLib.Core.DataAccess.Models;
using TelegramBotLib.Core.Entities;
using TelegramBotLib.Infrastructure.DataAccess.Models;

namespace TelegramBotLib.Infrastructure.DataAccess;

internal static class ModelMapper
{
    public static ToDoUser MapFromModel(ToDoUserModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return new ToDoUser
        {
            TelegramUserId = model.TelegramUserId,
            UserId = model.UserId,
            TelegramUserName = model.TelegramUserName,
            RegisteredAt = model.RegisteredAt
        };
    }

    public static ToDoUserModel MapToModel(ToDoUser entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return new ToDoUserModel
        {
            TelegramUserId = entity.TelegramUserId,
            UserId = entity.UserId,
            TelegramUserName = entity.TelegramUserName,
            RegisteredAt = entity.RegisteredAt,
        };
    }
    public static ToDoItem? MapFromModel(ToDoItemModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return new ToDoItem
        {
            Id = model.Id,
            User = model.User,
            Name = model.Name,
            CreatedAt = model.CreatedAt,
            State = model.State,
            StateChangedAt = model.StateChangedAt,
            Deadline = model.Deadline,
            List = model.List
        };
    }
    public static ToDoItemModel MapToModel(ToDoItem entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return new ToDoItemModel
        {
            Id = entity.Id,
            UserId = (Guid)(entity.User?.UserId),
            User = entity.User,
            Name = entity.Name,
            CreatedAt = entity.CreatedAt,
            State = entity.State,
            StateChangedAt = entity.StateChangedAt,
            Deadline = entity.Deadline,
            ListId = entity.List?.Id,
            List = entity.List
        };
    }
    public static ToDoList MapFromModel(ToDoListModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return new ToDoList
        {
            Id = (Guid)model.Id,
            Name = model.Name,
            User = model.User,
            CreatedAt = model.CreatedAt,
        };
    }
    public static ToDoListModel MapToModel(ToDoList entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return new ToDoListModel
        {
            Id = (Guid)entity.Id,
            Name = entity.Name,
            UserId = (Guid)(entity.User?.UserId),
            User = entity.User,
            CreatedAt = entity.CreatedAt,
        };
    }

    public static Notification MapFromModel(NotificationModel model)
    {
        if (model == null)
            throw new ArgumentNullException(nameof(model));

        return new Notification
        {
            Id = (Guid)model.Id,
            User = model.User,
            Type = model.Type,
            Text = model.Text,
            ScheduledAt = model.ScheduledAt,
            IsNotified = model.IsNotified,
            NotifiedAt = model.NotifiedAt,
        };
    }

    public static NotificationModel MapToModel(Notification entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return new NotificationModel
        {
            Id = (Guid)entity.Id,
            UserId = entity.User.UserId,
            User = entity.User,
            Type = entity.Type,
            Text = entity.Text,
            ScheduledAt = entity.ScheduledAt,
            IsNotified = entity.IsNotified,
            NotifiedAt = entity.NotifiedAt,
        };
    }
}
