using Otus.ToDoList.ConsoleBot;
using TelegramBotLib.TelegramBot;
using static System.Console;

namespace TelegramBotLib
{
    public class CoreTBot
    {
        public void Start()
        {
            try
            {
                var handler = new UpdateHandler();
                var botClient = new ConsoleBotClient();
                var cancellationTokenSource = new CancellationTokenSource();
                botClient.StartReceiving(handler, cancellationTokenSource.Token);
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
