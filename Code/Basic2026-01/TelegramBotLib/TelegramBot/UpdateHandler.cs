using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;
using TelegramBotLib.Core.Helpers;
using TelegramBotLib.Core.Scenarios;
using TelegramBotLib.Core.Services;
using TelegramBotLib.DTO;
using TelegramBotLib.Infrastructure.DataAccess;
using static System.Console;

namespace TelegramBotLib.TelegramBot
{
    internal class UpdateHandler : IUpdateHandler
    {
        IToDoService _toDoService;
        IUserService _userService;
        IToDoListService _toDoListService;
        IUserRepository _userRepository;
        IToDoReportService _toDoReportService;
        IToDoRepository _toDoRepository;                            //
        //IToDoRepositoryIndex _toDoRepositoryIndex;
        IToDoListRepository _toDoListRepository;
        string _userCommand = string.Empty;
        string _commandArgument = string.Empty;
        ReplyKeyboardMarkup _replyKeyboard;
        IEnumerable<IScenario> _scenarios;
        IScenarioContextRepository _contextRepository;
        ITelegramBotClient? _botClient = null;
        static int _pageSize = 3;
        int _currentPage = 0;

        public UpdateHandler(
            IEnumerable<IScenario> scenarios,
            IScenarioContextRepository contextRepository,
            IDataContextFactory<ToDoDataContext> dataContextFactory,
            IToDoRepository toDoRepository,
            IUserRepository userRepository,
            IToDoListRepository toDoListRepository,
            IToDoListService toDoListService,
            IToDoService toDoService,
            IUserService userService,
            IToDoReportService toDoReportService,
            ITelegramBotClient botClient)
        {

            //var dataContextFactory = new DataContextFactory();
            //dataContextFactory.CreateDataContext();
            //_toDoRepository = new SqlToDoRepository(dataContextFactory);
            _toDoRepository = toDoRepository;
            //_toDoListRepository = new SqlToDoListRepository(dataContextFactory);
            _toDoListRepository = toDoListRepository;
            //_toDoListService = new ToDoListService(_toDoListRepository);
            _toDoListService = toDoListService;
            //_toDoService = new ToDoService(_toDoRepository, _toDoListService);
            _toDoService = toDoService;
            //_userRepository = new SqlUserRepository(dataContextFactory);
            _userRepository = userRepository;
            //_userService = new UserService(_userRepository);
            _userService = userService;
            //_toDoReportService = new ToDoReportService(_toDoRepository);
            _toDoReportService = toDoReportService;
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

            var user = GetUserFromUpdate(update);
            var scenario = GetScenario(context.CurrentScenario);

            var scenarioResult = await scenario.HandleMessageAsync(_botClient, context, update, ct);

            if (scenarioResult == ScenarioResult.Completed)
            {
                _scenarios = Enumerable.Empty<IScenario>(); // TODO VS по идее нужно удалять IScenario только для определенного пользователя.
                await _contextRepository.ResetContext(user.Id, ct);
            }
            else
                await _contextRepository.SetContext(user.Id, context, ct);
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            await (update switch
            {
                { Message: { } message } => OnMessage(botClient, update, message, ct),
                { CallbackQuery: { } callbackQuery } => OnCallbackQuery(botClient, update, callbackQuery, ct),
                _ => OnUnknown(update)
            });
        }

        private async Task OnUnknown(Update update)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Создать разметку клавиатуры для страницы.
        /// </summary>
        /// <param name="callbackData">Общий набор кнопок. Ключ - имя кнопки, Значение - callbackData.</param>
        /// <param name="listDto">????</param>
        /// <returns>Разметка клавиатуры для страницы.</returns>
        private async Task<InlineKeyboardMarkup> BuildPagedButtons(
            IReadOnlyList<KeyValuePair<string, string>> callbackData,
            PagedListCallbackDto pageListDto)
        {
            //Расчитать общее количество страниц.
            var totalPages = (callbackData.Count + _pageSize - 1) / _pageSize; // Деление целых чисел с округлением вверх.
            //Создать InlineKeyboardMarkup и добавить кнопки относящие только к конкретной странице с помощью 
            var inlineKeyboardMarkup = new InlineKeyboardMarkup();

            // Получить задачи из callbackData.
            var tasks = new List<ToDoItem>();
            for (var i = 0; i < callbackData.Count; ++i)
            {
                var toDoListId = ToDoListCallbackDto.FromString(callbackData[i].Value).ToDoListId;
                var toDoItem = await _toDoRepository.Get((Guid)toDoListId, CancellationToken.None);
                tasks.Add(toDoItem);
            }

            var tempCurrentPage = _currentPage;
            var tasksInPage = tasks.GetBatchByNumber(_pageSize, pageListDto.Page)?.Cast<ToDoItem>();
            // Добавить задачи.
            foreach (var task in tasksInPage)
            {
                //var activeTasksCallbackDto = ToDoListCallbackDto.FromString($"showtask|{task.Id}");
                var activeTasksCallbackDto = pageListDto.Action == "show"
                    ? ToDoListCallbackDto.FromString($"showtask|{task.Id}")
                    : ToDoListCallbackDto.FromString($"showcompletedtaskinfo|{task.Id}");

                inlineKeyboardMarkup.AddNewRow(
                new[]
                {
                    InlineKeyboardButton.WithCallbackData(text: $"{task.Name}", callbackData: activeTasksCallbackDto.ToString()),
                });
            }

            //Если listDto.Page > 0 то добавить кнопку ⬅️ с PagedListCallbackDto(listDto.Action, listDto.ToDoListId, page -1)
            bool toLeftAdded = false;
            if (pageListDto.Page > 0)
            {
                toLeftAdded = true;
                //var pagedListCallbackDto = PagedListCallbackDto.FromString($"{pageListDto.Action}|{pageListDto.ToDoListId}|{pageListDto.Page - 1}");
                var pagedListCallbackDto = PagedListCallbackDto.FromString($"{pageListDto.Action}|{pageListDto.ToDoListId}|{_currentPage - 1}");
                inlineKeyboardMarkup.AddNewRow(
                    new[]
                    {
                        InlineKeyboardButton.WithCallbackData(text: "⬅️", callbackData: pagedListCallbackDto.ToString()),
                    });
            }

            //Если listDto.Page < totalPages - 1 то добавить кнопку ➡️ с PagedListCallbackDto(listDto.Action, listDto.ToDoListId, page +1)
            if (pageListDto.Page < totalPages - 1)
            {
                var pagedListCallbackDtoNext = PagedListCallbackDto.FromString($"{pageListDto.Action}|{pageListDto.ToDoListId}|{_currentPage + 1}");
                if (toLeftAdded)
                {
                    inlineKeyboardMarkup.AddButton(InlineKeyboardButton.WithCallbackData(text: "➡️", callbackData: pagedListCallbackDtoNext.ToString()));
                }
                else
                {
                    inlineKeyboardMarkup.AddNewRow(
                        new[]
                        {
                            InlineKeyboardButton.WithCallbackData(text: "➡️", callbackData: pagedListCallbackDtoNext.ToString()),
                        });
                }
            }

            if (pageListDto.Action == "show")
            {
                // Добавить кнопку Посмотреть выполненные.
                var pagedListActiveCallbackDtoNext = PagedListCallbackDto.FromString($"show_completed|{pageListDto.ToDoListId}|0");
                inlineKeyboardMarkup.AddNewRow(
                    new[]
                    {
                    InlineKeyboardButton.WithCallbackData(text: "☑️Посмотреть выполненные", callbackData: pagedListActiveCallbackDtoNext.ToString()),
                    });
            }

            return inlineKeyboardMarkup;
        }

        /// <summary>
        /// Отобразить задачи в зависимости от статуса задачи (в работе/выполнена).
        /// </summary>
        /// <param name="botClient">Бот клиент.</param>
        /// <param name="callbackQuery">Информация о действии пользователя.</param>
        /// <param name="needActiveTasks">True - отобразить активные, иначе завершенные задачи.</param>
        /// <param name="ct">Токен отмены.</param>
        private async Task ShowTasks(ITelegramBotClient botClient, CallbackQuery callbackQuery, bool needActiveTasks, CancellationToken ct)
        {
            var user = callbackQuery.From;
            var chat = callbackQuery.Message?.Chat;
            var toDoUser = await _userService.GetUser(user.Id, ct);
            if (toDoUser == null || chat == null || callbackQuery == null)
                return;

            var toDoListCallbackDto = ToDoListCallbackDto.FromString(callbackQuery.Data);
            // Получить задачи без списка (категории) для задач.
            var tasks = toDoListCallbackDto.ToDoListId != null
                ? await _toDoService.GetByUserIdAndList(toDoUser.UserId, toDoListCallbackDto.ToDoListId, ct)
                : await _toDoRepository.Find(toDoUser.UserId, x => x.List == null, ct);

            var activeTasks = needActiveTasks 
                ? tasks.Where(x => x.State == ToDoItemState.Active)
                : tasks.Where(x => x.State == ToDoItemState.Completed);

            if (!activeTasks.Any() && !needActiveTasks)
            {
                await botClient.SendMessage(chat, "Список задач пуст.", replyMarkup: _replyKeyboard, cancellationToken: ct);
                return;
            }

            var allTasksKeyValuePair = new List<KeyValuePair<string, string>>();
            foreach (var task in activeTasks)
            {
                var activeTasksCallbackDto = needActiveTasks
                    ? ToDoListCallbackDto.FromString($"showtask|{task.Id}")
                    : ToDoListCallbackDto.FromString($"show_completed|{task.Id}");
                allTasksKeyValuePair.Add(new KeyValuePair<string, string>(task.Name, activeTasksCallbackDto.ToString()));
            }

            // Добавить inline-кнопки для отображения активных задач.
            // Пробуем получить номер страницы. Если удается обновляем _currentPage.
            try
            {
                string[] data = callbackQuery.Data.Split("|");
                if (data.Length > 2)
                    _currentPage = int.Parse(data[2]);
            }
            catch (Exception ex) { }

            PagedListCallbackDto pagedListCallbackDto = new PagedListCallbackDto
            {
                Page = _currentPage,
                Action = toDoListCallbackDto.Action,
                ToDoListId = toDoListCallbackDto.ToDoListId
            };

            var inlineKeyboardActiveTasks = await BuildPagedButtons(allTasksKeyValuePair, pagedListCallbackDto);
            var title = needActiveTasks ? "Задачи в работе" : "Выполненные задачи";
            await botClient.SendMessage(chat, title, replyMarkup: inlineKeyboardActiveTasks, cancellationToken: ct);
        }

        private async Task OnCallbackQuery(ITelegramBotClient botClient, Update update, CallbackQuery callbackQuery, CancellationToken ct)
        {
            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct); // Чтобы кнопка не мерцала и другие кнопки реагировали.
            // Добавить проверку на то, что пользователь зарегистрирован.
            var user = callbackQuery.From;
            var chat = callbackQuery.Message?.Chat;
            var toDoUser = await _userService.GetUser(user.Id, ct);
            if (toDoUser == null || chat == null)
                return;

            // Также нужно проверять запущен ли для пользователя сценарий и вызывать ProcessScenario.
            var contextRepository = await _contextRepository.GetContext(user.Id, ct);
            if (contextRepository != null)
            {
                await ProcessScenario(contextRepository, update, ct);
                return;
            }

            #region Обработка inline-кнопок

            if (callbackQuery.Data == null)
                return;

            var toDoListCallbackDto = ToDoListCallbackDto.FromString(callbackQuery.Data);
            // Обрабатываем нажатие в зависимости от callbackData
            switch (toDoListCallbackDto.Action)
            {
                case "show":
                    await ShowTasks(botClient, callbackQuery, true, ct);
                    break;
                case "show_completed":
                    await ShowTasks(botClient, callbackQuery, false, ct);
                    break;
                case "showtask":
                    InlineKeyboardMarkup inlineKeyboardCompliteDeleteTasks = new InlineKeyboardMarkup();
                    var completeCallbackDto = ToDoListCallbackDto.FromString($"completetask|{toDoListCallbackDto.ToDoListId}");
                    var deleteCallbackDto = ToDoListCallbackDto.FromString($"deletetask|{toDoListCallbackDto.ToDoListId}");
                    inlineKeyboardCompliteDeleteTasks.AddNewRow(
                            new[]
                            {
                                    InlineKeyboardButton.WithCallbackData(text: "✅Выполнить", callbackData: completeCallbackDto.ToString()),
                                    InlineKeyboardButton.WithCallbackData(text: "❌Удалить", callbackData: deleteCallbackDto.ToString()),
                            });

                    var taskSelected = await _toDoRepository.Get((Guid)toDoListCallbackDto.ToDoListId, ct);
                    await botClient.SendMessage(chat, $"Задача {taskSelected?.Name}:", replyMarkup: inlineKeyboardCompliteDeleteTasks, cancellationToken: ct);
                    break;
                case "showcompletedtaskinfo":
                    var completedTaskSelected = await _toDoRepository.Get((Guid)toDoListCallbackDto.ToDoListId, ct);
                    await botClient.SendMessage(
                        chat,
                        $"Задача {completedTaskSelected?.Name}:\nСрок выполнения: {completedTaskSelected?.Deadline}\nВремя создания: {completedTaskSelected?.CreatedAt}\nВремя выполнения: {completedTaskSelected?.StateChangedAt}",
                        replyMarkup: _replyKeyboard,
                        cancellationToken: ct);
                    break;
                case "completetask":
                    var taskForComplete = await _toDoRepository.Get((Guid)toDoListCallbackDto.ToDoListId, ct);
                    await _toDoRepository.Update(taskForComplete, ct);
                    await botClient.SendMessage(
                        chat,
                        $"Задача {taskForComplete?.Name}:\nСрок выполнения: {taskForComplete?.Deadline}\nВремя создания: {taskForComplete?.CreatedAt}\nЗадача выполнена.",
                        replyMarkup: _replyKeyboard,
                        cancellationToken: ct);
                    break;
                case "deletetask":
                    var deleteTaskScenarioContext = new ScenarioContext(ScenarioType.DeleteTask);
                    deleteTaskScenarioContext.Data.Add(BotConstants.KeyUserIdName, chat.Id);
                    var deleteTaskScenario = new DeleteTaskScenario(_toDoService);
                    _scenarios = _scenarios.Append(deleteTaskScenario).ToList();
                    await ProcessScenario(deleteTaskScenarioContext, update, ct);
                    break;
                case "addlist":
                    var newScenarioContext = new ScenarioContext(ScenarioType.AddList)
                    {
                        UserId = toDoUser.TelegramUserId
                    };
                    newScenarioContext.Data.Add(BotConstants.KeyUserIdName, chat.Id);
                    var addListScenario = new AddListScenario(_userService, _toDoListService);
                    _scenarios = _scenarios.Append(addListScenario).ToList();
                    await ProcessScenario(newScenarioContext, update, ct);
                    break;
                case "deletelist":
                    var deleteListScenarioContext = new ScenarioContext(ScenarioType.DeleteList)
                    {
                        UserId = toDoUser.TelegramUserId
                    };
                    deleteListScenarioContext.Data.Add(BotConstants.KeyUserIdName, chat.Id);
                    var deleteListScenario = new DeleteListScenario(_userService, _toDoListService, _toDoService);
                    _scenarios = _scenarios.Append(deleteListScenario).ToList();
                    await ProcessScenario(deleteListScenarioContext, update, ct);
                    break;
                default:
                    break;
            }

            #endregion
        }

        private async Task OnMessage(ITelegramBotClient botClient, Update update, Message message, CancellationToken ct)
        {
            try
            {
                if (update == null || message == null)
                    return;

                // Получить пользователя и чат.
                var user = update.Message?.From;
                var chat = update.Message?.Chat;
                if (user == null || chat == null)
                    return;

                var toDoUser = await _userService.GetUser(user.Id, ct);
                // Получить команду и агрументы команды.
                await GetUserCommandAndArgumentAsync(message.Text, ct);

                var scenarioContext = await _contextRepository.GetContext(user.Id, ct);
                if (scenarioContext != null && _userCommand != BotConstants.CommandCancel)
                {
                    await ProcessScenario(scenarioContext, update, ct);
                    return;
                }

                // Обработать команду пользователя.
                switch (_userCommand)
                {
                    case BotConstants.CommandStart:
                        if (toDoUser == null)
                            toDoUser = await _userService.RegisterUser(user.Id, user.Username, ct);

                        // Создаем кнопки.
                        _replyKeyboard = await CreateKeyboardMarkup(toDoUser, botClient, update, ct);
                        await botClient.SendMessage(chat, $"Привет, {toDoUser.TelegramUserName}", replyMarkup: _replyKeyboard, cancellationToken: ct);
                        break;
                    case BotConstants.CommandHelp:
                        await CommandHelpAsync(toDoUser, botClient, update, ct);
                        break;
                    case BotConstants.CommandInfo:
                        var messageInfo = new StringBuilder();
                        messageInfo.AppendLine("Информация о программе.");
                        messageInfo.Append($"Версия бота 0.0.1. Дата создания {BotConstants.CreatedDate}");
                        await botClient.SendMessage(chat, messageInfo.ToString(), replyMarkup: _replyKeyboard, cancellationToken: ct);
                        break;
                    case BotConstants.CommandAddTask:
                        var isValidUser = await ValidateUserAsync(toDoUser, botClient, update, _replyKeyboard, ct);
                        if (!isValidUser)
                            break;

                        #region Запустить сессиею/сценарий пользователя добавления задачи.

                        var newScenarioContext = new ScenarioContext(ScenarioType.AddTask)
                        {
                            UserId = toDoUser.TelegramUserId
                        };
                        newScenarioContext.Data.Add(BotConstants.KeyUserIdName, chat.Id);
                        var taskScenario = new AddTaskScenario(_userService, _toDoService, _toDoListService);
                        _scenarios = _scenarios.Append(taskScenario).ToList();
                        await ProcessScenario(newScenarioContext, update, ct);

                        #endregion

                        break;
                    case BotConstants.CommandShowTasks:
                        isValidUser = await ValidateUserAsync(toDoUser, botClient, update, _replyKeyboard, ct);
                        if (!isValidUser)
                            break;

                        #region Inline-клавиатура.

                        // Создаем клавиатуру
                        InlineKeyboardButton withOutList = InlineKeyboardButton.WithCallbackData(text: "📌 Без списка", callbackData: "show");
                        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(withOutList);
                        // Добавить списки (категории) для задач, если есть в хранилище списков (категорий) для задач.
                        var userLists = await _toDoListRepository.GetByUserId(toDoUser.UserId, ct);
                        foreach (var list in userLists)
                        {
                            var toDoListCallbackDto = ToDoListCallbackDto.FromString($"show|{list.Id}");
                            inlineKeyboard.AddNewRow(
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData(text: list.Name, callbackData: toDoListCallbackDto.ToString()),
                                });
                        }
                        // Кнопки Добавить и Удалить.
                        InlineKeyboardButton[] addDelete =
                            new[]
                            {
                                InlineKeyboardButton.WithCallbackData(text: "🆕 Добавить", callbackData: "addlist"),
                                InlineKeyboardButton.WithCallbackData(text: "❌ Удалить", callbackData: "deletelist"),
                            };
                        inlineKeyboard.AddNewRow(addDelete);

                        // Отправляем сообщение с прикрепленной клавиатурой.
                        Message message1 = await botClient.SendMessage(
                            chat,
                            text: "Выберите список",
                            replyMarkup: inlineKeyboard,
                            cancellationToken: ct
                        );

                        #endregion

                        break;
                    case BotConstants.CommandReport:
                        isValidUser = await ValidateUserAsync(toDoUser, botClient, update, _replyKeyboard, ct);
                        if (!isValidUser)
                            break;

                        (int total, int completed, int active, DateTime generatedAt) = await _toDoReportService.GetUserStats(toDoUser.UserId, ct);
                        await botClient.SendMessage(
                            chat,
                            $"Статистика по задачам на {generatedAt}. Всего: {total}; Завершенных: {completed}; Активных: {active};",
                            replyMarkup: _replyKeyboard,
                            cancellationToken: ct);
                        break;
                    case BotConstants.CommandFind:
                        isValidUser = await ValidateUserAsync(toDoUser, botClient, update, _replyKeyboard, ct);
                        if (!isValidUser)
                            break;

                        if (string.IsNullOrWhiteSpace(_commandArgument))
                        {
                            await botClient.SendMessage(
                                chat,
                                $"Формат команды {BotConstants.CommandFind} [Текст].",
                                replyMarkup: _replyKeyboard,
                                cancellationToken: ct);
                            break;
                        }

                        var tasksFinded = await _toDoService.Find(toDoUser, _commandArgument, ct);
                        var taskFindedNumber = 1;
                        if (tasksFinded.Any())
                        {
                            foreach (var taskFinded in tasksFinded)
                            {
                                await botClient.SendMessage(
                                    chat,
                                    $"Задача: {taskFindedNumber++}. {taskFinded.Name} - {taskFinded.CreatedAt} - {taskFinded.Id}",
                                    replyMarkup: _replyKeyboard,
                                    cancellationToken: ct);
                            }
                            break;
                        }
                        await botClient.SendMessage(chat, "Задачи не найдены.", replyMarkup: _replyKeyboard, cancellationToken: ct);

                        break;
                    case BotConstants.CommandCancel:
                        var context = await _contextRepository.GetContext(user.Id, ct);
                        if (context == null)
                            break;

                        context.CurrentStep = "Cancel";
                        await ProcessScenario(context, update, ct);
                        _scenarios = Enumerable.Empty<IScenario>();
                        break;
                    default:
                        await botClient.SendMessage(update.Message.Chat, "Неизвестная команда.", replyMarkup: _replyKeyboard, cancellationToken: ct);
                        await CommandHelpAsync(toDoUser, botClient, update, ct);
                        break;
                }
            }
            catch (Exception e)
            {
                await botClient.SendMessage(update.Message.Chat, e.Message, replyMarkup: _replyKeyboard, cancellationToken: ct);
            }
        }

        /// <summary>
        /// Вывести список команд.
        /// </summary>
        async Task CommandHelpAsync(ToDoUser toDoUser, ITelegramBotClient botClient, Update update, CancellationToken ct)
        {
            var messageHelp = new StringBuilder();
            messageHelp.AppendLine("Список команд:");
            messageHelp.AppendLine($"{BotConstants.CommandStart} - Начать работать с ботом.");
            messageHelp.AppendLine($"{BotConstants.CommandHelp} - Вывести команды.");

            if (toDoUser != null)
            {
                messageHelp.AppendLine($"{BotConstants.CommandAddTask} - Добавить задчу.");
                messageHelp.AppendLine($"{BotConstants.CommandShowTasks} - Вывести задачи в работе.");
                messageHelp.AppendLine($"{BotConstants.CommandReport} - Вывести отчет по задачам.");
                messageHelp.AppendLine($"{BotConstants.CommandCancel} - Отменить сценарий.");
            }

            await botClient.SendMessage(update.Message.Chat, messageHelp.ToString().Trim(), replyMarkup: _replyKeyboard, cancellationToken: ct);
        }

        /// <summary>
        /// Получить команду и аргументы команды от пользователя.
        /// </summary>
        async Task GetUserCommandAndArgumentAsync(string messageText, CancellationToken ct)
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
            CancellationToken ct)
        {
            if (user == null)
            {
                await botClient.SendMessage(
                    update.Message.Chat,
                    $"Для начала работы используйте команду {BotConstants.CommandStart}.", replyMarkup: replyKeyboard,
                    cancellationToken: ct);
                return false;
            }

            return true;
        }

        public Task HandleErrorAsync(
            ITelegramBotClient botClient,
            Exception exception,
            HandleErrorSource source,
            CancellationToken ct)
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

        /// <summary>
        /// Создать разметку клавиатуры по умолчанию (доступны основные действия).
        /// </summary>
        /// <param name="user">Пользователь.</param>
        /// <param name="botClient">Бот клиент.</param>
        /// <param name="update">Обновления от Telegram.</param>
        /// <param name="ct">Токен отмены.</param>
        /// <returns></returns>
        private async Task<ReplyKeyboardMarkup> CreateKeyboardMarkup(
            ToDoUser user,
            ITelegramBotClient botClient,
            Update update,
            CancellationToken ct)
        {
            var isValidUser = await ValidateUserAsync(user, botClient, update, _replyKeyboard, ct);
            var buttons = new List<KeyboardButton[]>();

            buttons.Add(new KeyboardButton[] { new KeyboardButton("/start") });
            if (isValidUser)
            {
                buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandAddTask) });
                buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandShowTasks) });
                buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandReport) });
            }

            return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
        }

        #region Клавиатуры для сценариев и не только.

        /// <summary>
        /// Создать клавиатуру по умолчанию.
        /// </summary>
        /// <returns>Клавиатура по умолчанию.</returns>
        public static async Task<ReplyKeyboardMarkup> CreateKeyboardMarkupDefault()
        {
            var buttons = new List<KeyboardButton[]>();
            buttons.Add(new KeyboardButton[] { new KeyboardButton("/start") });
            buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandAddTask) });
            buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandShowTasks) });
            buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandReport) });

            return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
        }

        /// <summary>
        /// Создать клавиатуру во время обработки сценариев.
        /// </summary>
        /// <returns>Клавиатура.</returns>
        public static async Task<ReplyKeyboardMarkup> CreateKeyboardMarkupCancel()
        {
            var buttons = new List<KeyboardButton[]>();
            buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandCancel) });
            return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
        }

        #endregion

        /// <summary>
        /// Получить пользователя из объекта обновления.
        /// </summary>
        /// <param name="update">Объект обновления.</param>
        /// <returns>Пользователь.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static User GetUserFromUpdate(Update update)
        {
            if (update.Message != null)
                return update.Message.From;

            if (update.CallbackQuery != null)
                return update.CallbackQuery.From;

            if (update.InlineQuery != null)
                return update.InlineQuery.From;

            if (update.EditedMessage != null)
                return update.EditedMessage.From;

            if (update.ChannelPost != null)
                return update.ChannelPost.From;

            if (update.EditedChannelPost != null)
                return update.EditedChannelPost.From;

            if (update.ChosenInlineResult != null)
                return update.ChosenInlineResult.From;

            throw new InvalidOperationException("Не удалось определить пользователя из update");
        }

        /// <summary>
        /// Получить чат из объекта обновления.
        /// </summary>
        /// <param name="update">Объект обновления.</param>
        /// <returns>Чат.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static Chat GetChatFromUpdate(Update update)
        {
            if (update.Message != null)
                return update.Message.Chat;

            if (update.CallbackQuery != null)
                return update.CallbackQuery.Message.Chat;

            if (update.EditedMessage != null)
                return update.EditedMessage.Chat;

            if (update.ChannelPost != null)
                return update.ChannelPost.Chat;

            if (update.EditedChannelPost != null)
                return update.EditedChannelPost.Chat;


            throw new InvalidOperationException("Не удалось определить чат из update");
        }

        /// <summary>
        /// Получить сообщение из объекта обновления.
        /// </summary>
        /// <param name="update">Объект обновления.</param>
        /// <returns>Чат.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        public static string GetMessageFromUpdate(Update update)
        {
            if (update.Message != null)
                return update.Message.Text;

            if (update.CallbackQuery != null)
                return update.CallbackQuery.Message.Text;

            if (update.EditedMessage != null)
                return update.EditedMessage.Text;

            if (update.ChannelPost != null)
                return update.ChannelPost.Text;

            if (update.EditedChannelPost != null)
                return update.EditedChannelPost.Text;


            throw new InvalidOperationException("Не удалось получить сообщение из update");
        }
    }
}