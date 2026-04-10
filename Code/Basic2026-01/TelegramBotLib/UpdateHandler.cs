using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;
using System.Text;
using TelegramBotLib.Services;
using TelegramBotLib.DataAccess;
using TelegramBotLib.Entities;

namespace TelegramBotLib
{
    public class UpdateHandler : IUpdateHandler
    {
        IToDoService _toDoService;
        IUserService _userService;
        IToDoReportService _toDoReportService;
        IToDoRepository _toDoRepository;
        string _userCommand = string.Empty;
        string _commandArgument = string.Empty;

        public UpdateHandler()
        {
            _toDoRepository = new InMemoryToDoRepository();
            _toDoService = new ToDoService(_toDoRepository);
            _userService = new UserService();
            _toDoReportService = new ToDoReportService(_toDoRepository);
        }

        public void HandleUpdateAsync(ITelegramBotClient botClient, Update update)
        {
            try
            {
                // Only process Message updates: https://core.tetegram.org/bots/api#message
                if (update.Message is not { } message)
                    return;

                // Only process text messages
                if (message.Text is not { } messageText)
                    return;

                // Получить пользователя и чат.
                var user = update.Message.From;
                var chat = update.Message.Chat;
                var toDoUser = _userService.GetUser(user.Id);
                // Получить команду и агрументы команды.
                GetUserCommandAndArgument(messageText);

                // Обработать команду пользователя.
                switch (_userCommand)
                {
                    case BotConstants.CommandStart:
                        if (toDoUser == null)
                            toDoUser = _userService.RegisterUser(user.Id, user.Username);

                        botClient.SendMessage(chat, $"Привет, {toDoUser.TelegramUserName}");
                        break;
                    case BotConstants.CommandHelp:
                        CommandHelp(toDoUser, botClient, update);
                        break;
                    case BotConstants.CommandInfo:
                        var messageInfo = new StringBuilder();
                        messageInfo.AppendLine("Информация о программе.");
                        messageInfo.Append($"Версия бота 0.0.1. Дата создания {BotConstants.CreatedDate}");
                        botClient.SendMessage(update.Message.Chat, messageInfo.ToString());
                        break;
                    case BotConstants.CommandExit:
                        botClient.SendMessage(chat, "Работа с ботом завершена.");
                        Environment.Exit(0);
                        return;
                    case BotConstants.CommandAddTask:
                        if (!ValidateUser(toDoUser, botClient, update))
                            break;

                        var task = _toDoService.Add(toDoUser, _commandArgument);
                        if (task == null)
                        {
                            botClient.SendMessage(chat, $"Нужно добавить описание задачи: {BotConstants.CommandAddTask} [Описание задачи] или создано слишком много задач.");
                            break;
                        }
                        botClient.SendMessage(chat, "Задача добавлена.");
                        break;
                    case BotConstants.CommandShowTasks:
                        if (!ValidateUser(toDoUser, botClient, update))
                            break;

                        var tasks = _toDoService.GetActiveByUserId(toDoUser.UserId);
                        if (!tasks.Any())
                        {
                            botClient.SendMessage(chat, "Список задач пуст.");
                            break;
                        }

                        botClient.SendMessage(chat, "Cписок задач:");
                        var taskNumber = 1;
                        foreach (var taskForShow in tasks)
                        {
                            botClient.SendMessage(chat, $"Задача: {taskNumber++}. {taskForShow.Name} - {taskForShow.CreatedAt} - {taskForShow.Id}");
                        }
                        break;
                    case BotConstants.CommandRemoveTask:
                        if (!ValidateUser(toDoUser, botClient, update))
                            break;

                        var tasksForRemove = _toDoService.GetActiveByUserId(toDoUser.UserId);
                        if (!tasksForRemove.Any())
                        {
                            botClient.SendMessage(chat, "Список задач пуст.");
                            break;
                        }

                        if (!long.TryParse(_commandArgument, out var taskNumberForRemove) || taskNumberForRemove > tasksForRemove.Count)
                        {
                            botClient.SendMessage(chat, string.Format(BotConstants.MessageNoTaskFoundByNumber, _commandArgument, BotConstants.CommandRemoveTask));
                            break;
                        }

                        // Найти задачу для удаления.
                        ToDoItem taskToRemove = null;
                        long number = 1;
                        foreach (var taskForRemove in tasksForRemove)
                        {
                            if (number == taskNumberForRemove)
                            {
                                taskToRemove = taskForRemove;
                                break;
                            }
                            number++;
                        }

                        if (taskToRemove != null)
                        {
                            _toDoService.Delete(taskToRemove.Id);
                            botClient.SendMessage(chat, $"Задача с номером {number} удалена");
                        }
                        else
                            botClient.SendMessage(chat, string.Format(BotConstants.MessageNoTaskFoundByNumber, _commandArgument, BotConstants.CommandRemoveTask));

                        break;
                    case BotConstants.CommandCompleteTask:
                        if (!ValidateUser(toDoUser, botClient, update))
                            break;

                        var isCommandArgumentEmpty = string.IsNullOrWhiteSpace(_commandArgument);
                        if (!Guid.TryParse(_commandArgument, out var taskGuid) || isCommandArgumentEmpty)
                        {
                            if (isCommandArgumentEmpty)
                                botClient.SendMessage(chat, "Id задачи не указан.");
                            else
                                botClient.SendMessage(chat, $"Id {_commandArgument} задачи некорректный.");

                            break;
                        }

                        _toDoService.MarkCompleted(taskGuid);
                        botClient.SendMessage(chat, $"Задача с Id {taskGuid} завершена.");
                        break;
                    case BotConstants.CommandShowAllTasks:
                        if (!ValidateUser(toDoUser, botClient, update))
                            break;

                        var allUserTasks = _toDoService.GetAllByUserId(toDoUser.UserId);
                        if (!allUserTasks.Any())
                        {
                            botClient.SendMessage(chat, "Список задач пуст.");
                            break;
                        }

                        botClient.SendMessage(chat, "Cписок задач:");
                        var allTaskNumber = 1;
                        foreach (var taskForShow in allUserTasks)
                            botClient.SendMessage(chat, $"Задача: {allTaskNumber++}. {taskForShow.Name} - {taskForShow.CreatedAt} - {taskForShow.Id}");
                        
                        break;
                    case BotConstants.CommandReport:
                        if (!ValidateUser(toDoUser, botClient, update))
                            break;

                        (int total, int completed, int active, DateTime generatedAt) = _toDoReportService.GetUserStats(toDoUser.UserId);
                        botClient.SendMessage(chat, $"Статистика по задачам на {generatedAt}. Всего: {total}; Завершенных: {completed}; Активных: {active};");
                        break;
                    case BotConstants.CommandFind:
                        if (!ValidateUser(toDoUser, botClient, update))
                            break;

                        if (string.IsNullOrWhiteSpace(_commandArgument))
                        {
                            botClient.SendMessage(chat, $"Формат команды {BotConstants.CommandFind} [Текст].");
                            break;
                        }

                        var tasksFinded = _toDoService.Find(toDoUser, _commandArgument);
                        var taskFindedNumber = 1;
                        if (tasksFinded.Any())
                        {
                            foreach (var taskFinded in tasksFinded)
                            {
                                botClient.SendMessage(chat, $"Задача: {taskFindedNumber++}. {taskFinded.Name} - {taskFinded.CreatedAt} - {taskFinded.Id}");
                            }
                            break;
                        }
                        botClient.SendMessage(chat, "Задачи не найдены.");
                        break;
                    default:
                        botClient.SendMessage(update.Message.Chat, "Неизвестная команда.");
                        CommandHelp(toDoUser, botClient, update);
                        break;
                }
            }
            catch (Exception e)
            {
                botClient.SendMessage(update.Message.Chat, e.Message);
            }
        }

        /// <summary>
        /// Вывести список команд.
        /// </summary>
        void CommandHelp(ToDoUser toDoUser, ITelegramBotClient botClient, Update update)
        {
            var messageHelp = new StringBuilder();
            messageHelp.AppendLine("Список команд:");
            messageHelp.AppendLine($"{BotConstants.CommandStart} - Начать работать с ботом.");
            messageHelp.AppendLine($"{BotConstants.CommandHelp} - Вывести команды.");
            messageHelp.AppendLine($"{BotConstants.CommandInfo} - Вывести информацию о Telegram боте.");

            if (toDoUser != null)
            {
                messageHelp.AppendLine($"{BotConstants.CommandAddTask} - Добавить задчу.");
                messageHelp.AppendLine($"{BotConstants.CommandShowTasks} - Вывести задачи в работе.");
                messageHelp.AppendLine($"{BotConstants.CommandRemoveTask} - Удалить задачу.");
                messageHelp.AppendLine($"{BotConstants.CommandCompleteTask} - Установить статус задачи на Завершена.");
                messageHelp.AppendLine($"{BotConstants.CommandShowAllTasks} - Вывести все задачи.");
                messageHelp.AppendLine($"{BotConstants.CommandReport} - Вывести отчет по задачам.");
                messageHelp.AppendLine($"{BotConstants.CommandFind} - Вывести задачи, которые начинаются на префикс.");
                messageHelp.AppendLine($"{BotConstants.CommandExit} - Выход.");
            }

            botClient.SendMessage(update.Message.Chat, messageHelp.ToString().Trim());
        }

        /// <summary>
        /// Получить команду и аргументы команды от пользователя.
        /// </summary>
        void GetUserCommandAndArgument(string messageText)
        {
            _commandArgument = string.Empty;
            string[] arr = messageText.Split(' ');
            if (arr.Length > 0)
            {
                _userCommand = arr[0].Trim();

                if (arr.Length > 1)
                {
                    for (int i = 1; i < arr.Length; i++)
                        _commandArgument = string.Join(" ", _commandArgument, arr[i].Trim()).Trim();
                }
            }
        }

        static bool ValidateUser(ToDoUser user, ITelegramBotClient botClient, Update update)
        {
            if (user == null)
            {
                botClient.SendMessage(update.Message.Chat, $"Для начала работы используйте команду {BotConstants.CommandStart}.");
                return false;
            }

            return true;
        }
    }
}