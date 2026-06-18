using System.Text;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotLib.Core.DataAccess;
using TelegramBotLib.Core.Entities;
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
        IToDoRepository _toDoRepository;
        IToDoRepositoryIndex _toDoRepositoryIndex;
        IToDoListRepository _toDoListRepository;
        string _userCommand = string.Empty;
        string _commandArgument = string.Empty;
        ReplyKeyboardMarkup _replyKeyboard;
        IEnumerable<IScenario> _scenarios;
        IScenarioContextRepository _contextRepository;
        ITelegramBotClient? _botClient = null;

        public UpdateHandler(
            string pathToDoItemsRepository,
            string pathUsersRepositoty,
            string pathToDoListRepository,
            IToDoRepositoryIndex toDoRepositoryIndex,
            IEnumerable<IScenario> scenarios,
            IScenarioContextRepository contextRepository,
            ITelegramBotClient botClient)
        {
            _toDoRepositoryIndex = toDoRepositoryIndex;
            _toDoRepository = new FileToDoRepository(pathToDoItemsRepository, _toDoRepositoryIndex);
            _toDoListRepository = new FileToDoListRepository(pathToDoListRepository);
            _toDoListService = new ToDoListService(_toDoListRepository);
            _toDoService = new ToDoService(_toDoRepository, _toDoListService);
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

        private async Task OnCallbackQuery(ITelegramBotClient botClient, Update update, CallbackQuery callbackQuery, CancellationToken ct)
        {
            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct); // Чтобы кнопка не мерцала и другие кнопки реагировали.
            // Добавить проверку на то, что пользователь зарегистрирован.
            var user = callbackQuery.From;
            var chat = callbackQuery.Message?.Chat;
            var toDoUser = await _userService.GetUser(user.Id, ct);
            if (toDoUser == null)
                return;

            // Также нужно проверять запущен ли для пользователя сценарий и вызывать ProcessScenario.
            var contextRepository = await _contextRepository.GetContext(user.Id, ct);
            if (contextRepository != null)
            {
                await ProcessScenario(contextRepository, update, ct);
                // TODO VS возможно нужно return убрать.
                return;
            }

            #region Обработка inline-кнопок

            // Проверяем, что событие — это нажатие на инлайн-кнопку
            if (update.Type == UpdateType.CallbackQuery)
            {
                // Получаем ID чата и уникальный ID запроса (для ответа в Telegram)
                var callbackData = callbackQuery.Data;
                var toDoListCallbackDto = ToDoListCallbackDto.FromString(callbackData);

                // Обрабатываем нажатие в зависимости от callbackData
                switch (toDoListCallbackDto.Action)
                {
                    case "show":
                        // Получить задачи без списка (категории) для задач.
                        var tasks = toDoListCallbackDto.ToDoListId != null
                            ? await _toDoService.GetByUserIdAndList(toDoUser.UserId, toDoListCallbackDto.ToDoListId, ct)
                            : await _toDoRepository.Find(toDoUser.UserId, x => x.List == null, ct);

                        if (!tasks.Any())
                        {
                            await botClient.SendMessage(chat, "Список задач пуст.", replyMarkup: _replyKeyboard, cancellationToken: ct);
                            break;
                        }

                        await botClient.SendMessage(chat, "Cписок задач:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                        var taskNumber = 1;
                        foreach (var taskForShow in tasks)
                        {
                            await botClient.SendMessage(
                                chat,
                                $"Задача: {taskNumber++}. {taskForShow.Name} - {taskForShow.CreatedAt} - '{taskForShow.Id}'",
                                replyMarkup: _replyKeyboard,
                                cancellationToken: ct);
                        }
                        break;
                    case "addlist":
                        var newScenarioContext = new ScenarioContext(ScenarioType.AddList);
                        newScenarioContext.UserId = toDoUser.TelegramUserId;
                        var addListScenario = new AddListScenario(_userService, _toDoListService);
                        _scenarios = _scenarios.Append(addListScenario).ToList();
                        await ProcessScenario(newScenarioContext, update, ct);
                        break;
                    case "deletelist":
                        var deleteListScenarioContext = new ScenarioContext(ScenarioType.DeleteList);
                        deleteListScenarioContext.UserId = toDoUser.TelegramUserId;
                        var deleteListScenario = new DeleteListScenario(_userService, _toDoListService, _toDoService);
                        _scenarios = _scenarios.Append(deleteListScenario).ToList();
                        await ProcessScenario(deleteListScenarioContext, update, ct);
                        break;
                    default:
                        break;
                }
            }

            #endregion
        }

        private async Task OnMessage(ITelegramBotClient botClient, Update update, Message message, CancellationToken ct)
        {
            try
            {
                // Only process text messages
                if (message.Text is not { } messageText)
                    return;

                // Получить пользователя и чат.
                var user = update.Message.From;
                var chat = update.Message.Chat;

                var toDoUser = await _userService.GetUser(user.Id, ct);
                // Получить команду и агрументы команды.
                await GetUserCommandAndArgumentAsync(messageText, ct);

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
                        await botClient.SendMessage(update.Message.Chat, messageInfo.ToString(), replyMarkup: _replyKeyboard, cancellationToken: ct);
                        break;
                    case BotConstants.CommandAddTask:
                        var isValidUser = await ValidateUserAsync(toDoUser, botClient, update, _replyKeyboard, ct);
                        if (!isValidUser)
                            break;

                        #region Запустить сессиею/сценарий пользователя добавления задачи.

                        var newScenarioContext = new ScenarioContext(ScenarioType.AddTask);
                        newScenarioContext.UserId = toDoUser.TelegramUserId;
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
                        InlineKeyboardButton withOutList = InlineKeyboardButton.WithCallbackData(
                            text: "📌 Без списка",
                            callbackData: "show");
                        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(withOutList);
                        // Добавить списки (категории) для задач, если есть в хранилище списков (категорий) для задач.
                        var userLists = await _toDoListRepository.GetByUserId(toDoUser.UserId, ct);
                        foreach (var list in userLists)
                        {
                            inlineKeyboard.AddNewRow(
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData(text: list.Name, callbackData: $"show|{list.Id}"),
                                });
                        }
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
                    case BotConstants.CommandRemoveTask:
                        isValidUser = await ValidateUserAsync(toDoUser, botClient, update, _replyKeyboard, ct);
                        if (!isValidUser)
                            break;

                        var tasksForRemove = await _toDoService.GetAllByUserId(toDoUser.UserId, ct);
                        if (!tasksForRemove.Any())
                        {
                            await botClient.SendMessage(chat, "Список задач пуст.", replyMarkup: _replyKeyboard, cancellationToken: ct);
                            break;
                        }

                        if (!Guid.TryParse(_commandArgument, out var taskGuidForRemove))
                        {
                            await botClient.SendMessage(
                                chat,
                                string.Format(BotConstants.MessageNoTaskFoundByNumber, _commandArgument, BotConstants.CommandRemoveTask),
                                replyMarkup: _replyKeyboard,
                                cancellationToken: ct);
                            break;
                        }

                        // Найти задачу для удаления.
                        ToDoItem? taskToRemove = tasksForRemove.Where(x => x.Id == taskGuidForRemove).FirstOrDefault();

                        if (taskToRemove != null)
                        {
                            await _toDoService.Delete(taskToRemove.Id, ct);
                            await botClient.SendMessage(chat, $"Задача с номером {taskToRemove.Id} удалена", replyMarkup: _replyKeyboard, cancellationToken: ct);
                        }
                        else
                            await botClient.SendMessage(
                                chat,
                                string.Format(BotConstants.MessageNoTaskFoundByNumber, _commandArgument, BotConstants.CommandRemoveTask),
                                replyMarkup: _replyKeyboard,
                                cancellationToken: ct);

                        break;
                    case BotConstants.CommandCompleteTask:
                        isValidUser = await ValidateUserAsync(toDoUser, botClient, update, _replyKeyboard, ct);
                        if (!isValidUser)
                            break;

                        var isCommandArgumentEmpty = string.IsNullOrWhiteSpace(_commandArgument);
                        if (!Guid.TryParse(_commandArgument, out var taskGuid) || isCommandArgumentEmpty)
                        {
                            if (isCommandArgumentEmpty)
                                await botClient.SendMessage(chat, "Id задачи не указан.", replyMarkup: _replyKeyboard, cancellationToken: ct);
                            else
                                await botClient.SendMessage(chat, $"Id {_commandArgument} задачи некорректный.", replyMarkup: _replyKeyboard, cancellationToken: ct);

                            break;
                        }

                        await _toDoService.MarkCompleted(taskGuid, ct);
                        await botClient.SendMessage(chat, $"Задача с Id {taskGuid} завершена.", replyMarkup: _replyKeyboard, cancellationToken: ct);
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
            //messageHelp.AppendLine($"{BotConstants.CommandInfo} - Вывести информацию о Telegram боте.");

            if (toDoUser != null)
            {
                messageHelp.AppendLine($"{BotConstants.CommandAddTask} - Добавить задчу.");
                messageHelp.AppendLine($"{BotConstants.CommandShowTasks} - Вывести задачи в работе.");
                messageHelp.AppendLine($"{BotConstants.CommandRemoveTask} - Удалить задачу.");
                //messageHelp.AppendLine($"{BotConstants.CommandCompleteTask} - Установить статус задачи на Завершена.");
                messageHelp.AppendLine($"{BotConstants.CommandReport} - Вывести отчет по задачам.");
                //messageHelp.AppendLine($"{BotConstants.CommandFind} - Вывести задачи, которые начинаются на префикс.");
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