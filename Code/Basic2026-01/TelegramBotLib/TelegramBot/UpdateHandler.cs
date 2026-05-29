using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;
using TelegramBotLib.Core.Scenarios;
using TelegramBotLib.Core.Services;
using TelegramBotLib.Infrastructure.DataAccess;
using static System.Console;

namespace TelegramBotLib.TelegramBot
{
    internal class UpdateHandler : IUpdateHandler
    {
        IToDoService _toDoService;
        IUserService _userService;
        IUserRepository _userRepository;
        IToDoReportService _toDoReportService;
        IToDoRepository _toDoRepository;
        IToDoRepositoryIndex _toDoRepositoryIndex;
        string _userCommand = string.Empty;
        string _commandArgument = string.Empty;
        ReplyKeyboardMarkup _replyKeyboard;
        IEnumerable<IScenario> _scenarios;
        IScenarioContextRepository _contextRepository;
        ITelegramBotClient? _botClient = null;

        public UpdateHandler(
            string pathToDoItemsRepository,
            string pathUsersRepositoty,
            IToDoRepositoryIndex toDoRepositoryIndex,
            IEnumerable<IScenario> scenarios,
            IScenarioContextRepository contextRepository,
            ITelegramBotClient botClient)
        {
            _toDoRepositoryIndex = toDoRepositoryIndex;
            _toDoRepository = new FileToDoRepository(pathToDoItemsRepository, _toDoRepositoryIndex);
            _toDoService = new ToDoService(_toDoRepository);
            _userRepository = new FileUserRepository(pathUsersRepositoty);
            _userService = new UserService(_userRepository);
            _toDoReportService = new ToDoReportService(_toDoRepository);
            _replyKeyboard = new ReplyKeyboardMarkup();
            _scenarios = scenarios;
            _contextRepository = contextRepository;
            _botClient = botClient;
        }

        /// <summary>
        /// Возвращает сессию/сценарий. Если сессия/сценарий не найден, то выбрасывать исключение.
        /// </summary>
        /// <param name="scenario">Тип сессии/сценария.</param>
        /// <returns>Сессия/сценарий.</returns>
        IScenario GetScenario(ScenarioType scenarioType)
        {
            // TODO VS c чем сравнивать scenarioType?
            // TODO VS тут возможно нужно получать сценарий из _contextRepository.
            /*var userId = 1; // Как получить пользователя.
            var scenarioContext = await _contextRepository.GetContext(userId, CancellationToken.None);
            if (scenarioContext?.CurrentScenario != null)
                return scenarioContext;
            else
                throw new NullReferenceException($"Тип сессии/сценария {scenarioType} не найден.");*/

            var scenarios = _scenarios.Where(x => x.CanHandle(scenarioType));
            if (scenarios.Any())
                return scenarios.First();
            else
                throw new NullReferenceException($"Тип сессии/сценария {scenarioType} не найден.");
        }

        async Task ProcessScenario(ScenarioContext context, Update update, CancellationToken ct)
        {
            if (_botClient == null)
                return;

            var user = update.Message.From;
            var scenario = GetScenario(context.CurrentScenario);

            var scenarioResult = await scenario.HandleMessageAsync(_botClient, context, update, ct);

            if (scenarioResult == ScenarioResult.Completed)
            {
                _scenarios = Enumerable.Empty<IScenario>();
                await _contextRepository.ResetContext(user.Id, ct);
            }
            else
                await _contextRepository.SetContext(user.Id, context, ct);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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
                var toDoUser = await _userService.GetUser(user.Id, cancellationToken);
                // Получить команду и агрументы команды.
                await GetUserCommandAndArgumentAsync(messageText, cancellationToken);

                var scenarioContext = await _contextRepository.GetContext(user.Id, cancellationToken);
                if (scenarioContext != null && _userCommand != BotConstants.CommandCancel)
                {
                    await ProcessScenario(scenarioContext, update, cancellationToken);
                    return;
                }

                // Обработать команду пользователя.
                switch (_userCommand)
                {
                    case BotConstants.CommandStart:
                        if (toDoUser == null)
                            toDoUser = await _userService.RegisterUser(user.Id, user.Username, cancellationToken);

                        // Создаем кнопки.
                        _replyKeyboard = await CreateKeyboardMarkup(toDoUser, botClient, update, cancellationToken);
                        await botClient.SendMessage(chat, $"Привет, {toDoUser.TelegramUserName}", replyMarkup: _replyKeyboard, cancellationToken: cancellationToken);
                        break;
                    case BotConstants.CommandHelp:
                        await CommandHelpAsync(toDoUser, botClient, update, cancellationToken);
                        break;
                    case BotConstants.CommandInfo:
                        var messageInfo = new StringBuilder();
                        messageInfo.AppendLine("Информация о программе.");
                        messageInfo.Append($"Версия бота 0.0.1. Дата создания {BotConstants.CreatedDate}");
                        await botClient.SendMessage(update.Message.Chat, messageInfo.ToString(), replyMarkup: _replyKeyboard, cancellationToken: cancellationToken);
                        break;
                    case BotConstants.CommandExit:
                        await botClient.SendMessage(chat, "Работа с ботом завершена.", replyMarkup: _replyKeyboard, cancellationToken: cancellationToken);
                        return;
                    case BotConstants.CommandAddTask:
                        var isValidUser = await ValidateUserAsync(toDoUser, botClient, update, _replyKeyboard, cancellationToken);
                        if (!isValidUser)
                            break;

                        #region Тут как-то надо начать работать со сессией/сценарием пользователя.
                        var newScenarioContext = new ScenarioContext(ScenarioType.AddTask);
                        newScenarioContext.UserId = toDoUser.TelegramUserId;
                        var taskScenario = new AddTaskScenario(_userService, _toDoService);
                        _scenarios = _scenarios.Append(taskScenario).ToList();
                        await ProcessScenario(newScenarioContext, update, cancellationToken);

                        #endregion

                        break;
                    case BotConstants.CommandShowTasks:
                        isValidUser = await ValidateUserAsync(toDoUser, botClient, update, _replyKeyboard, cancellationToken);
                        if (!isValidUser)
                            break;

                        var tasks = await _toDoService.GetActiveByUserId(toDoUser.UserId, cancellationToken);
                        if (!tasks.Any())
                        {
                            await botClient.SendMessage(chat, "Список задач пуст.", replyMarkup: _replyKeyboard, cancellationToken: cancellationToken);
                            break;
                        }

                        await botClient.SendMessage(chat, "Cписок задач:", replyMarkup: _replyKeyboard, cancellationToken: cancellationToken);
                        var taskNumber = 1;
                        foreach (var taskForShow in tasks)
                        {
                            await botClient.SendMessage(
                                chat,
                                $"Задача: {taskNumber++}. {taskForShow.Name} - {taskForShow.CreatedAt} - '{taskForShow.Id}'",
                                replyMarkup: _replyKeyboard,
                                cancellationToken: cancellationToken);
                        }
                        break;
                    case BotConstants.CommandRemoveTask:
                        isValidUser = await ValidateUserAsync(toDoUser, botClient, update, _replyKeyboard, cancellationToken);
                        if (!isValidUser)
                            break;

                        var tasksForRemove = await _toDoService.GetAllByUserId(toDoUser.UserId, cancellationToken);
                        if (!tasksForRemove.Any())
                        {
                            await botClient.SendMessage(chat, "Список задач пуст.", replyMarkup: _replyKeyboard, cancellationToken: cancellationToken);
                            break;
                        }

                        if (!Guid.TryParse(_commandArgument, out var taskGuidForRemove))
                        {
                            await botClient.SendMessage(
                                chat,
                                string.Format(BotConstants.MessageNoTaskFoundByNumber, _commandArgument, BotConstants.CommandRemoveTask),
                                replyMarkup: _replyKeyboard,
                                cancellationToken: cancellationToken);
                            break;
                        }

                        // Найти задачу для удаления.
                        ToDoItem? taskToRemove = tasksForRemove.Where(x => x.Id == taskGuidForRemove).FirstOrDefault();

                        if (taskToRemove != null)
                        {
                            await _toDoService.Delete(taskToRemove.Id, cancellationToken);
                            await botClient.SendMessage(chat, $"Задача с номером {taskToRemove.Id} удалена", replyMarkup: _replyKeyboard, cancellationToken: cancellationToken);
                        }
                        else
                            await botClient.SendMessage(
                                chat,
                                string.Format(BotConstants.MessageNoTaskFoundByNumber, _commandArgument, BotConstants.CommandRemoveTask),
                                replyMarkup: _replyKeyboard,
                                cancellationToken: cancellationToken);

                        break;
                    case BotConstants.CommandCompleteTask:
                        isValidUser = await ValidateUserAsync(toDoUser, botClient, update, _replyKeyboard, cancellationToken);
                        if (!isValidUser)
                            break;

                        var isCommandArgumentEmpty = string.IsNullOrWhiteSpace(_commandArgument);
                        if (!Guid.TryParse(_commandArgument, out var taskGuid) || isCommandArgumentEmpty)
                        {
                            if (isCommandArgumentEmpty)
                                await botClient.SendMessage(chat, "Id задачи не указан.", replyMarkup: _replyKeyboard, cancellationToken: cancellationToken);
                            else
                                await botClient.SendMessage(chat, $"Id {_commandArgument} задачи некорректный.", replyMarkup: _replyKeyboard, cancellationToken: cancellationToken);

                            break;
                        }

                        await _toDoService.MarkCompleted(taskGuid, cancellationToken);
                        await botClient.SendMessage(chat, $"Задача с Id {taskGuid} завершена.", replyMarkup: _replyKeyboard, cancellationToken: cancellationToken);
                        break;
                    case BotConstants.CommandShowAllTasks:
                        isValidUser = await ValidateUserAsync(toDoUser, botClient, update, _replyKeyboard, cancellationToken);
                        if (!isValidUser)
                            break;

                        var allUserTasks = await _toDoService.GetAllByUserId(toDoUser.UserId, cancellationToken);
                        if (!allUserTasks.Any())
                        {
                            await botClient.SendMessage(chat, "Список задач пуст.", replyMarkup: _replyKeyboard, cancellationToken: cancellationToken);
                            break;
                        }

                        await botClient.SendMessage(chat, "Cписок задач:", replyMarkup: _replyKeyboard, cancellationToken: cancellationToken);
                        var allTaskNumber = 1;
                        foreach (var taskForShow in allUserTasks)
                        {
                            await botClient.SendMessage(
                                chat,
                                $"Задача: {allTaskNumber++}. {taskForShow.Name} - {taskForShow.CreatedAt} - '{taskForShow.Id}'",
                                replyMarkup: _replyKeyboard,
                                cancellationToken: cancellationToken);
                        }
                        break;
                    case BotConstants.CommandReport:
                        isValidUser = await ValidateUserAsync(toDoUser, botClient, update, _replyKeyboard, cancellationToken);
                        if (!isValidUser)
                            break;

                        (int total, int completed, int active, DateTime generatedAt) = await _toDoReportService.GetUserStats(toDoUser.UserId, cancellationToken);
                        await botClient.SendMessage(
                            chat,
                            $"Статистика по задачам на {generatedAt}. Всего: {total}; Завершенных: {completed}; Активных: {active};",
                            replyMarkup: _replyKeyboard,
                            cancellationToken: cancellationToken);
                        break;
                    case BotConstants.CommandFind:
                        isValidUser = await ValidateUserAsync(toDoUser, botClient, update, _replyKeyboard, cancellationToken);
                        if (!isValidUser)
                            break;

                        if (string.IsNullOrWhiteSpace(_commandArgument))
                        {
                            await botClient.SendMessage(
                                chat,
                                $"Формат команды {BotConstants.CommandFind} [Текст].",
                                replyMarkup: _replyKeyboard,
                                cancellationToken: cancellationToken);
                            break;
                        }

                        var tasksFinded = await _toDoService.Find(toDoUser, _commandArgument, cancellationToken);
                        var taskFindedNumber = 1;
                        if (tasksFinded.Any())
                        {
                            foreach (var taskFinded in tasksFinded)
                            {
                                await botClient.SendMessage(
                                    chat,
                                    $"Задача: {taskFindedNumber++}. {taskFinded.Name} - {taskFinded.CreatedAt} - {taskFinded.Id}",
                                    replyMarkup: _replyKeyboard,
                                    cancellationToken: cancellationToken);
                            }
                            break;
                        }
                        await botClient.SendMessage(chat, "Задачи не найдены.", replyMarkup: _replyKeyboard, cancellationToken: cancellationToken);

                        break;
                    case BotConstants.CommandCancel:
                        var context = await _contextRepository.GetContext(user.Id, cancellationToken);
                        if (context == null)
                            break;

                        context.CurrentStep = "Cancel";
                        await ProcessScenario(context, update, cancellationToken);
                        _scenarios = Enumerable.Empty<IScenario>();
                        break;
                    default:
                        await botClient.SendMessage(update.Message.Chat, "Неизвестная команда.", replyMarkup: _replyKeyboard, cancellationToken: cancellationToken);
                        await CommandHelpAsync(toDoUser, botClient, update, cancellationToken);
                        break;
                }
            }
            catch (Exception e)
            {
                await botClient.SendMessage(update.Message.Chat, e.Message, replyMarkup: _replyKeyboard, cancellationToken: cancellationToken);
            }
        }

        /// <summary>
        /// Вывести список команд.
        /// </summary>
        async Task CommandHelpAsync(ToDoUser toDoUser, ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
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
                messageHelp.AppendLine($"{BotConstants.CommandCancel} - Отменить сценарий.");
                messageHelp.AppendLine($"{BotConstants.CommandExit} - Выход.");
            }

            await botClient.SendMessage(update.Message.Chat, messageHelp.ToString().Trim(), replyMarkup: _replyKeyboard, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Получить команду и аргументы команды от пользователя.
        /// </summary>
        async Task GetUserCommandAndArgumentAsync(string messageText, CancellationToken cancellationToken)
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

        /// <summary>
        /// Проверить пользователя.
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <param name="botClient">TelegramBot клиент.</param>
        /// <param name="update">Обновленные данные от пользователя.</param>
        /// <param name="replyKeyboard">Клавиатура Telegram бота.</param>
        /// <param name="cancellationToken">Токен отмены.</param>
        /// <returns>False - пользователь равен null (отправить сообщение анонимному пользователю о дальнейших действиях), иначе True.</returns>
        static async Task<bool> ValidateUserAsync(
            ToDoUser user,
            ITelegramBotClient botClient,
            Update update,
            ReplyKeyboardMarkup replyKeyboard,
            CancellationToken cancellationToken)
        {
            if (user == null)
            {
                await botClient.SendMessage(
                    update.Message.Chat,
                    $"Для начала работы используйте команду {BotConstants.CommandStart}.", replyMarkup: replyKeyboard,
                    cancellationToken: cancellationToken);
                return false;
            }

            return true;
        }

        public Task HandleErrorAsync(
            ITelegramBotClient botClient,
            Exception exception,
            HandleErrorSource source,
            CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegran API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            WriteLine(ErrorMessage);

            return Task.CompletedTask;
        }

        private async Task<ReplyKeyboardMarkup> CreateKeyboardMarkup(
            ToDoUser user,
            ITelegramBotClient botClient,
            Update update,
            CancellationToken cancellationToken)
        {
            var isValidUser = await ValidateUserAsync(user, botClient, update, _replyKeyboard, cancellationToken);
            var buttons = new List<KeyboardButton[]>();

            buttons.Add(new KeyboardButton[] { new KeyboardButton("/start") });
            if (isValidUser)
            {
                buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandAddTask) });
                buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandShowAllTasks) });
                buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandShowTasks) });
                buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandReport) });
            }

            return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
        }
    }
}