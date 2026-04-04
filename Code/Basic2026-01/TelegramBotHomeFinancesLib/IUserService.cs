
namespace TelegramBotHomeFinancesLib
{
    internal interface IUserService
    {
        User RegisterUser(long telegramUserId, string telegramUserName);
        User? GetUser(long telegramUserId);
    }
}
