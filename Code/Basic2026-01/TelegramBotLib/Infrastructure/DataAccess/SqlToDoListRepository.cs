using LinqToDB;
using LinqToDB.Async;
using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Infrastructure.DataAccess
{
    internal class SqlToDoListRepository : IToDoListRepository
    {
        IDataContextFactory<ToDoDataContext> _factory;

        public SqlToDoListRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task Add(ToDoList list, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                var toDoListModel = ModelMapper.MapToModel(list);
                await dbContext.InsertAsync(toDoListModel, token: ct);
            }
        }

        public async Task Delete(Guid id, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                var toDoList = await Get(id, ct);
                if (toDoList == null)
                    return;

                var toDoListModel = ModelMapper.MapToModel(toDoList);
                await dbContext.DeleteAsync(toDoListModel);
            }
        }

        public async Task<bool> ExistsByName(Guid userId, string name, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                return await dbContext.ToDoLists
                    .Where(i => i.User.UserId == userId && i.Name == name)
                    .AnyAsync();
            }
        }

        public async Task<ToDoList?> Get(Guid id, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                var toDoList = await dbContext.ToDoLists
                    .LoadWith(i => i.User)
                    .Where(i => i.Id == id)
                    .FirstOrDefaultAsync();
                
                return ModelMapper.MapFromModel(toDoList);
            }
        }

        public async Task<IReadOnlyList<ToDoList>> GetByUserId(Guid userId, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                var toDoLists = await dbContext.ToDoLists
                    .LoadWith(i => i.User)
                    .Where(i => i.User!.UserId == userId)
                    .ToListAsync();
                
                return toDoLists.Select(i => ModelMapper.MapFromModel(i)).ToList().AsReadOnly();
            }
        }
    }
}
