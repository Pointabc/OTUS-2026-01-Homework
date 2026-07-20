using LinqToDB;
using LinqToDB.Async;
using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;

namespace TelegramBotLib.Infrastructure.DataAccess
{
    internal class SqlUserRepository : IUserRepository
    {
        IDataContextFactory<ToDoDataContext> _factory;

        public SqlUserRepository(IDataContextFactory<ToDoDataContext> factory)
        {
            _factory = factory;
        }

        public async Task Add(ToDoUser user, CancellationToken ct)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                var toDoUserModel = ModelMapper.MapToModel(user);
                await dbContext.InsertAsync(toDoUserModel, token: ct);
            }
        }

        public async Task<ToDoUser?> GetUser(Guid userId, CancellationToken cancellationToken)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                var toDoUser = await dbContext.ToDoUsers
                    .Where(i => i.UserId == userId)
                    .FirstOrDefaultAsync();

                return toDoUser != null ? ModelMapper.MapFromModel(toDoUser) : null;
            }
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken cancellationToken)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                var toDoUser = await dbContext.ToDoUsers
                    .Where(i => i.TelegramUserId == telegramUserId)
                    .FirstOrDefaultAsync();

                return toDoUser != null ? ModelMapper.MapFromModel(toDoUser) : null;
            }
        }

        public async Task<IReadOnlyList<ToDoUser>> GetUsers(CancellationToken ct)
        {
            var allUsers = new List<ToDoUser>();
            using (var dbContext = _factory.CreateDataContext())
            {
                var toDoUser = await dbContext.ToDoUsers.FirstOrDefaultAsync();
                if (toDoUser != null)
                    allUsers.Add(ModelMapper.MapFromModel(toDoUser));
            }

            return allUsers;
        }
    }
}
