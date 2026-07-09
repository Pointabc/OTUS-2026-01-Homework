using LinqToDB;
using LinqToDB.Async;
using Telegram.Bot.Types;
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
                    .Select(i => ModelMapper.MapFromModel(i))
                    .FirstOrDefaultAsync();

                return toDoUser;
            }
        }

        public async Task<ToDoUser?> GetUserByTelegramUserId(long telegramUserId, CancellationToken cancellationToken)
        {
            using (var dbContext = _factory.CreateDataContext())
            {
                //var toDoUser1 = dbContext.ToDoUsers.Where(i => i.TelegramUserId == telegramUserId);

                var toDoUser = await dbContext.ToDoUsers
                    .Where(i => i.TelegramUserId == telegramUserId)
                    .FirstOrDefaultAsync();

                return ModelMapper.MapFromModel(toDoUser);
            }
        }
    }
}
