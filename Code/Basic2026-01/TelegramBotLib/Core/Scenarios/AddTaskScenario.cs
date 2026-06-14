using System.Globalization;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotLib.Core.Entities;
using TelegramBotLib.Core.Services;
using TelegramBotLib.TelegramBot;

namespace TelegramBotLib.Core.Scenarios
{
    internal class AddTaskScenario : IScenario
    {
        IUserService _userService;
        IToDoService _toDoService;
        IToDoListService _toDoListService;
        ToDoItem _toDoItem;                         // Для работы на следующем этапе сценария.

        public AddTaskScenario(IUserService userService, IToDoService toDoService, IToDoListService toDoListService)
        {
            _userService = userService;
            _toDoService = toDoService;
            _toDoListService = toDoListService;
        }

        public bool CanHandle(ScenarioType scenarioType)
        {
            return scenarioType == ScenarioType.AddTask;
        }

        public async Task<ScenarioResult> HandleMessageAsync(
            ITelegramBotClient bot,
            ScenarioContext context,
            Update update,
            CancellationToken ct)
        {
            var scenarioResultTask = await (update switch
            {
                { Message: { } message } => OnMessage(bot, update, message, context, ct),
                { CallbackQuery: { } callbackQuery } => OnCallbackQuery(bot, update, callbackQuery, context, ct),
                _ => OnUnknown(update)
            });

            return scenarioResultTask;
        }

        private async Task<ScenarioResult> OnCallbackQuery(ITelegramBotClient botClient, Update update, CallbackQuery callbackQuery, ScenarioContext context, CancellationToken ct)
        {
            var scenarioResult = ScenarioResult.Transition;
            await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct); // Чтобы кнопка не мерцала и другие кнопки реагировали.

            if (update.Type == UpdateType.CallbackQuery)
            {
                // Получаем ID чата и уникальный ID запроса (для ответа в Telegram)
                string callbackData = callbackQuery.Data;
                string callbackId = callbackQuery.Id;
                long chatId = callbackQuery.Message.Chat.Id;

                var listName = callbackData.Split("|");
                if (listName.Length > 1)
                {
                    switch (listName[0])
                    {
                        case "SelectList":
                            var chat = UpdateHandler.GetChatFromUpdate(update);
                            // Тут получить список (категорию) для задач по Id и добавить в создаваемую задачу.
                            Guid guid = Guid.Empty;
                            Guid.TryParse(listName[1], out guid);
                            if (guid != Guid.Empty)
                            {
                                var list = await _toDoListService.Get(guid, ct);
                                _toDoItem.List = list;
                            }
                            
                            var task = await _toDoService.Add(_toDoItem, ct);
                            if (task == null)
                            {
                                await botClient.SendMessage(
                                    chat,
                                    $"Нужно добавить описание задачи: {BotConstants.CommandAddTask} [Описание задачи] или создано слишком много задач.",
                                    cancellationToken: ct);
                                break;
                            }

                            context.CurrentStep = "Сценарий завершен.";
                            scenarioResult = ScenarioResult.Completed;
                            ReplyKeyboardMarkup _replyKeyboardDefault = await UpdateHandler.CreateKeyboardMarkupDefault();
                            await botClient.SendMessage(chat, "Задача добавлена.", replyMarkup: _replyKeyboardDefault, cancellationToken: ct);

                            return ScenarioResult.Completed;
                        default:
                            break;
                    }
                }
            }

            return scenarioResult;
        }

        private async Task<ScenarioResult> OnUnknown(Update update)
        {
            throw new NotImplementedException();
        }

        private async Task<ScenarioResult> OnMessage(ITelegramBotClient botClient, Update update, Message message, ScenarioContext context, CancellationToken ct)
        {
            var scenarioResult = ScenarioResult.Transition;
            var telegramUser = UpdateHandler.GetUserFromUpdate(update);
            var toDoUser = await _userService.GetUser(telegramUser.Id, ct);
            var chat = UpdateHandler.GetChatFromUpdate(update);
            var currentStep = context.CurrentStep;
            ReplyKeyboardMarkup _replyKeyboard = await UpdateHandler.CreateKeyboardMarkupCancel();
            ReplyKeyboardMarkup _replyKeyboardDefault = await UpdateHandler.CreateKeyboardMarkupDefault();
            var userInput = UpdateHandler.GetMessageFromUpdate(update);

            switch (currentStep)
            {
                case null:
                    // TODO VS Какой должен быть ключ? Возможно ключ toDoUser.UserId. Хранить toDoUser.
                    context.Data.Add(toDoUser.TelegramUserId.ToString(), toDoUser);
                    await botClient.SendMessage(chat, "Введите название задачи:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                    context.CurrentStep = "Name";
                    break;
                case "Name":
                    try
                    {
                        _toDoItem = new ToDoItem(toDoUser, userInput, DateTime.MinValue);
                        _toDoItem.Name = userInput;
                        context.CurrentStep = "Deadline";
                        await botClient.SendMessage(chat, "Введите срок выполнения (dd.MM.yyyy):", replyMarkup: _replyKeyboard, cancellationToken: ct);
                    }
                    catch (Exception ex)
                    {
                        await botClient.SendMessage(chat, ex.Message, replyMarkup: _replyKeyboard, cancellationToken: ct);
                        switch (currentStep)
                        {
                            case "Name":
                                await botClient.SendMessage(chat, "Введите название задачи:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                                break;
                            default:
                                break;
                        }
                    }
                    break;
                case "Deadline":
                    // Проверить формат введенной даты.
                    string format = "dd.MM.yyyy";
                    DateTime deadline;
                    DateTime.TryParseExact(userInput, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out deadline);
                    if (deadline == DateTime.MinValue)
                    {
                        await botClient.SendMessage(chat, "Введите срок выполнения (dd.MM.yyyy):", replyMarkup: _replyKeyboard, cancellationToken: ct);
                        break;
                    }

                    _toDoItem.Deadline = deadline;
                    context.CurrentStep = "List";

                    // Создать inline-кнопки для выбора списка (категории) для задачи.
                    InlineKeyboardButton withOutList = InlineKeyboardButton.WithCallbackData(
                            text: "📌 Без списка",
                            callbackData: $"SelectList|WithoutList");
                    InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(withOutList);
                    // Добавить списки (категории) для задач, если есть в хранилище списков (категорий) для задач.
                    var userLists = await _toDoListService.GetUserLists(toDoUser.UserId, ct);
                    foreach (var list in userLists)
                    {
                        inlineKeyboard.AddNewRow(
                            new[]
                            {
                                    InlineKeyboardButton.WithCallbackData(text: list.Name, callbackData: $"SelectList|{list.Id}"),
                            });
                    }

                    // Отправляем сообщение с прикрепленной клавиатурой.
                    Message message1 = await botClient.SendMessage(
                        chat,
                        text: "Выберите список",
                        replyMarkup: inlineKeyboard,
                        cancellationToken: ct
                    );
                    break;
                case "List":
                    break;
                case "Cancel":
                    scenarioResult = ScenarioResult.Completed;
                    context.CurrentStep = "Сценарий завершен.";
                    await botClient.SendMessage(chat, "Операция отменена.", replyMarkup: _replyKeyboardDefault, cancellationToken: ct);
                    break;
                default:
                    break;
            }

            return scenarioResult;
        }
    }
}
