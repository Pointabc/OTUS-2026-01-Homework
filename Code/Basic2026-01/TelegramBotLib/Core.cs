using Otus.ToDoList.ConsoleBot;
using static System.Console;

namespace TelegramBotLib
{
    public class Core
    {
        public void Start()
        {
            try
            {
                var handler = new UpdateHandler();
                var botClient = new ConsoleBotClient();
                botClient.StartReceiving(handler);
            }
            catch (Exception e)
            {
                WriteLine("Произошла непредвиденная ошибка: ");
                WriteLine($"Type of exception: {e.GetType()}");
                WriteLine($"Message: {e.Message}");
                WriteLine($"StackTrace: {e.StackTrace}");
                WriteLine($"InnerException: {e.InnerException}");
            }
        }
    }
}
