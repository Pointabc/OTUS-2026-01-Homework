using LinqToDB;
using LinqToDB.Async;
using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Infrastructure.DataAccess
{
    internal class SqlToDoRepository : IToDoRepository
    {
        IDataContextFactory<ToDoDataContext> _factory;

        public SqlToDoRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task Add(ToDoItem item, CancellationToken ct)
        {
            if (item == null)
                throw new ArgumentNullException($"В методе {nameof(Add)} item = null.");

            using (var dbContext = _factory.CreateDataContext())
            {
                var toDoItemModel = ModelMapper.MapToModel(item);
                await dbContext.InsertAsync(toDoItemModel, token: ct);
            }
        }

        public async Task<int> CountActive(Guid userId, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                return await dbContext.ToDoItems
                    .LoadWith(i => i.User)
                    .Where(i => i.User.UserId == userId && i.State == ToDoItemState.Active)
                    .CountAsync();
            }
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                var toDoItem = await Get(id, ct);
                if (toDoItem == null)
                    return;

                var toDoItemModel = ModelMapper.MapToModel(toDoItem);
                await dbContext.DeleteAsync(toDoItemModel);
            }
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                return await dbContext.ToDoItems
                    .Where(i => i.User.UserId == userId && i.Name == name)
                    .AnyAsync();
            }
        }

        public async Task<IReadOnlyList<ToDoItem>> Find(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                // Сначала загружаем все данные из БД
                var allItems = await dbContext.ToDoItems
                    .LoadWith(i => i.User)
                    .LoadWith(i => i.List)
                    .Where(i => i.UserId == userId)
                    .ToListAsync(ct);

                // Применяем предикат в памяти
                var filteredItems = allItems.Select(i => ModelMapper.MapFromModel(i)).Where(predicate).ToList();

                return filteredItems.AsReadOnly();
            }
        }

        public async Task<ToDoItem?> Get(Guid id, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                var toDoItem = await dbContext.ToDoItems
                    .LoadWith(i => i.User)
                    .LoadWith(i => i.List)
                    .Where(i => i.Id == id)
                    .FirstOrDefaultAsync();

                return toDoItem != null ? ModelMapper.MapFromModel(toDoItem) : null;
            }
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                var toDoItems = await dbContext.ToDoItems
                    .LoadWith(i => i.User)
                    .LoadWith(i => i.List)
                    .Where(i => i.UserId == userId && i.State == ToDoItemState.Active)
                    .ToListAsync();

                return toDoItems.Select(ModelMapper.MapFromModel).ToList();
            }
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveWithDeadline(Guid userId, DateTime from, DateTime to, CancellationToken ct)
        {
            return await Find(userId, (x) => { return x.State == ToDoItemState.Active && x.Deadline <= from && x.Deadline < to; }, ct);
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                var toDoItems = await dbContext.ToDoItems
                    .LoadWith(i => i.User)
                    .LoadWith(i => i.List)
                    .Where(i => i.UserId == userId)
                    .ToListAsync();

                return toDoItems.Select(i => ModelMapper.MapFromModel(i)).ToList().AsReadOnly();
            }
        }

        public async Task Update(ToDoItem item, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                // Загружаем существующую запись
                var toDoItemModelFinded = await dbContext.ToDoItems
                    .LoadWith(i => i.User)
                    .LoadWith(i => i.List)
                    .FirstOrDefaultAsync(i => i.Id == item.Id, ct);

                if (toDoItemModelFinded == null)
                    throw new InvalidOperationException($"ToDoItem с Id {item.Id} не найдена.");

                toDoItemModelFinded.State = ToDoItemState.Completed;
                toDoItemModelFinded.StateChangedAt = DateTime.UtcNow;

                await dbContext.UpdateAsync(toDoItemModelFinded, token: ct);
            }
        }
    }
}