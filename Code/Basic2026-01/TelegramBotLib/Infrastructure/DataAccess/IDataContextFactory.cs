using LinqToDB.Data;

namespace TelegramBotLib.Infrastructure.DataAccess
{
    public interface IDataContextFactory<TDataContext> 
        where TDataContext : DataConnection
    {
        TDataContext CreateDataContext();
    }
}
