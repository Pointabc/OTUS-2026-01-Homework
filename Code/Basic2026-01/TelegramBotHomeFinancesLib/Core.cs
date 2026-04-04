using static System.Console;

namespace TelegramBotHomeFinancesLib
{
    public class Core
    {
        public async Task StartAsync()
        {
            try
            {
                var botClient = new HFTelegramBotClient();
                await botClient.StartAsync();
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
