using TelegramBotLib.TelegramBot;

namespace TelegramBotLib.Infrastructure.DataAccess
{
    internal class DataContextFactory : IDataContextFactory<ToDoDataContext>
    {
        public ToDoDataContext CreateDataContext()
        {
            return new ToDoDataContext(BotConstants.ConnectionString);
        }
    }
}
