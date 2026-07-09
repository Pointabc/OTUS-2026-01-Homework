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
                    //.LoadWith(i => i.User)
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
                    .LoadWith(i => i.List!.User)
                    .Where(i => i.User!.UserId == userId)
                    .Select(i => ModelMapper.MapFromModel(i))
                    .ToListAsync(ct);

                // Применяем предикат в памяти
                var filteredItems = allItems.Where(predicate).ToList();

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
                    .LoadWith(i => i.List!.User)
                    .Where(i => i.Id == id)
                    .Select(i => ModelMapper.MapFromModel(i))
                    .FirstOrDefaultAsync();

                return toDoItem;
            }
        }

        public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserId(Guid userId, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                var toDoItems = await dbContext.ToDoItems
                    .LoadWith(i => i.User)
                    .LoadWith(i => i.List)
                    .LoadWith(i => i.List!.User)
                    .Where(i => i.User!.UserId == userId && i.State == ToDoItemState.Active)
                    .Select(i => ModelMapper.MapFromModel(i))
                    .ToListAsync();

                return toDoItems;
            }
        }

        public async Task<IReadOnlyList<ToDoItem>> GetAllByUserId(Guid userId, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                var toDoItems = await dbContext.ToDoItems
                    .LoadWith(i => i.User)
                    .LoadWith(i => i.List)
                    .LoadWith(i => i.List!.User)
                    .Where(i => i.User!.UserId == userId)
                    .Select(i => ModelMapper.MapFromModel(i))
                    .ToListAsync();

                return toDoItems;
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
                    .LoadWith(i => i.List!.User)
                    .FirstOrDefaultAsync(i => i.Id == item.Id, ct);

                if (toDoItemModelFinded == null)
                    throw new InvalidOperationException($"ToDoItem с Id {item.Id} не найдена.");

                toDoItemModelFinded.State = ToDoItemState.Completed;
                toDoItemModelFinded.StateChangedAt = DateTime.Now;

                var toDoItemModel = ModelMapper.MapToModel(item);
                await dbContext.UpdateAsync(toDoItemModel, token: ct);
            }
        }
    }
}