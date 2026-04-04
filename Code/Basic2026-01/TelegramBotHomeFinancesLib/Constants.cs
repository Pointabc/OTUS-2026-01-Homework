
namespace TelegramBotHomeFinancesLib
{
    internal class Constants
    {
        #region Команды бота.

        public const string CommandStart = "/start";
        public const string CommandHelp = "/help";
        public const string CommandInfo = "/info";
        public const string CommandExit = "/exit";

        #endregion

        /// <summary>
        /// Дата создания бота (начало разработки бота).
        /// </summary>
        public static readonly DateTime CreatedDate = new DateTime(2026, 3, 15);

        public const string UnknownCommand = "Неизвестная команда.";
    }
}
