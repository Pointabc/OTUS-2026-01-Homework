
namespace TelegramBotLib
{
    internal class BotConstants
    {
        #region Команды бота.

        public const string CommandStart = "/start";
        public const string CommandHelp = "/help";
        public const string CommandInfo = "/info";
        public const string CommandExit = "/exit";
        public const string CommandEcho = "/echo";
        public const string CommandAddTask = "/addtask";
        public const string CommandShowTasks = "/showtasks";
        public const string CommandRemoveTask = "/removetask";
        public const string CommandCompleteTask = "/completetask";
        public const string CommandShowAllTasks = "/showalltasks";
        public const string CommandReport = "/report";
        public const string CommandFind = "/find";

        #endregion

        /// <summary>
        /// Дата создания бота.
        /// </summary>
        public static readonly DateTime CreatedDate = new DateTime(2026, 2, 9);

        #region Строковые константы.

        public const string MessageNoTaskFoundByNumber = "Задача с номером '{0}' не найдена. Используйте команду {1} и введите существующий номер задачи.";

        #endregion
    }
}
