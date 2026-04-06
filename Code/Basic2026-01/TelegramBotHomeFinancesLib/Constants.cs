
namespace TelegramBotHomeFinancesLib
{
    internal class Constants
    {
        #region Команды бота.

        public const string CommandStart = "/start";
        public const string CommandHelp = "/help";
        public const string CommandInfo = "/info";
        public const string CommandExit = "/exit";

        // Функции роли вносящего данные.
        public const string CommandAddInCome = "/addincome"; // Добавить доход.
        public const string CommandAddCost = "/addcost"; // Добавить расход.
        public const string CommandViewBalance = "/viewbalance"; // Посмотреть баланс.
        public const string CommandSendBalance = "/sendbalance"; // Отправить баланс.
        public const string CommandGetTypeInCome = "/gettypeincome"; // Получить виды доходов.
        public const string CommandGetTypeCost = "/gettypeincome"; // Получить виды расходов.
        // Функции Получателя данных.

        // Функции роли администратора структуры видов доходов/расходов (возможно добавить изменение других таблиц БД).

        // Функции Отправителя данных.
        /*
   Функции Получателя данных
	- Посмотреть баланс
	- Посмотреть доход
	- Посмотреть расход
   Функции роли администратора структуры видов доходов/расходов (возможно добавить изменение других таблиц БД)
	- Посмотреть виды расходов
	- Посмотреть виды доходов
	- Добавить вид доходов
	- Добавить вид расходов
	- Удалить вид доходов (все удаленные виды доходов переносятся в указанный вид доходов)
	- Удалить вид расходов(все удаленные виды расходов переносятся в указанный вид расходов)
	- Сделать бэкап БД.
   Функции Отправителя данных
	- Отправить баланс в виде pdf, word, xml на почту
	- Открыть баланс в браузере.*/ 
        */
        #endregion

        /// <summary>
        /// Дата создания бота (начало разработки бота).
        /// </summary>
        public static readonly DateTime CreatedDate = new DateTime(2026, 3, 15);

        public const string UnknownCommand = "Неизвестная команда.";
    }
}
