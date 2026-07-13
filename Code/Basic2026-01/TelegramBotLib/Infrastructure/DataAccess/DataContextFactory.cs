namespace TelegramBotLib.Infrastructure.DataAccess;

internal class DataContextFactory : IDataContextFactory<ToDoDataContext>
{
    public ToDoDataContext CreateDataContext()
    {
        var connectionString = Environment.GetEnvironmentVariable("ToDoEducationBotConnectionString", EnvironmentVariableTarget.User);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentNullException("Отсутствует строка подключения к БД в переменных среды текущего пользователя");

        return new ToDoDataContext(connectionString);
    }
}
