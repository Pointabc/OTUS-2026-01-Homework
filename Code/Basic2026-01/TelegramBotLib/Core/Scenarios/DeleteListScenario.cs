using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotLib.Core.Services;
using TelegramBotLib.DTO;
using TelegramBotLib.TelegramBot;

namespace TelegramBotLib.Core.Scenarios
{
    internal class DeleteListScenario : IScenario
    {
        IUserService _userService;
        IToDoListService _toDoListService;
        IToDoService _toDoService;
        Guid _listId = Guid.Empty; // Для хранения Guid списка (категории) для задач при подтверждении удаления.

        public DeleteListScenario(IUserService userService, IToDoListService toDoListService, IToDoService toDoService)
        {
            _userService = userService;
            _toDoListService = toDoListService;
            _toDoService = toDoService;
        }

        public bool CanHandle(ScenarioType scenarioType)
        {
            return scenarioType == ScenarioType.DeleteList;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Update update, CancellationToken ct)
        {
            var scenarioResultTask = await (update switch
            {
                { Message: { } message } => OnMessage(botClient, update, message, context, ct),
                { CallbackQuery: { } callbackQuery } => OnCallbackQuery(botClient, update, callbackQuery, context, ct),
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
                if (callbackQuery.Data == null)
                    return scenarioResult;

                await botClient.AnswerCallbackQuery(callbackQuery.Id, cancellationToken: ct);
                var toDoListCallbackDto = ToDoListCallbackDto.FromString(callbackQuery.Data);
                var chat = UpdateHandler.GetChatFromUpdate(update);
                var replyKeyboardDefault = await UpdateHandler.CreateKeyboardMarkupDefault();
                var telegramUser = UpdateHandler.GetUserFromUpdate(update);
                var toDoUser = await _userService.GetUser(telegramUser.Id, ct);
                if (toDoUser == null)
                    return scenarioResult;

                switch (toDoListCallbackDto.Action)
                {
                    case "SelectList":
                        if (toDoListCallbackDto.ToDoListId == null)
                            break;

                        // Для хранения Guid списка (категории) для задач при подтверждении удаления.
                        _listId = (Guid)toDoListCallbackDto.ToDoListId;
                        // Получить список (категорию) для задач по Id.
                        var list = await _toDoListService.Get((Guid)toDoListCallbackDto.ToDoListId, ct);

                        #region Inline-клавиатура.

                        // Создаем клавиатуру
                        InlineKeyboardMarkup inlineKeyboardDeleteApprove = new(
                            new[]
                            {
                                        // Первый ряд кнопок.
                                        new[]
                                        {
                                            InlineKeyboardButton.WithCallbackData(text: "✅Да", callbackData: "yes"),
                                            InlineKeyboardButton.WithCallbackData(text: "❌Нет", callbackData: "no"),
                                        },
                            });

                        #endregion

                        // Отправляем сообщение с прикрепленной клавиатурой.
                        Message message1 = await botClient.SendMessage(
                            chat,
                            text: $"Подтверждаете удаление списка {list?.Name} и всех его задач",
                            replyMarkup: inlineKeyboardDeleteApprove,
                            cancellationToken: ct
                        );

                        context.CurrentStep = "Delete";
                        scenarioResult = ScenarioResult.Transition;

                        return scenarioResult;
                    case "deletelist":
                        // Создать inline-кнопки для выбора списка (категории) для задачи.
                        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup();
                        // Получить списки (категории) для задач, если есть в хранилище списков (категорий) для задач.
                        var userLists = await _toDoListService.GetUserLists(toDoUser.UserId, ct);
                        foreach (var userList in userLists)
                        {
                            inlineKeyboard.AddNewRow(
                                new[]
                                {
                                        InlineKeyboardButton.WithCallbackData(text: userList.Name, callbackData: $"SelectList|{userList.Id}"),
                                });
                        }

                        // Отправляем сообщение с прикрепленной клавиатурой.
                        Message message = await botClient.SendMessage(
                            chat,
                            text: "Выберите список для удачения",
                            replyMarkup: inlineKeyboard,
                            cancellationToken: ct
                        );

                        context.CurrentStep = "Delete";
                        scenarioResult = ScenarioResult.Transition;
                        break;
                    case "yes":
                        scenarioResult = ScenarioResult.Completed;
                        // Удалить задачи в списке (категории) для задач.
                        var tasks = await _toDoService.GetByUserIdAndList(toDoUser.UserId, toDoListCallbackDto.ToDoListId, ct);

                        foreach (var task in tasks)
                            await _toDoService.Delete(task.Id, ct);

                        await _toDoListService.Delete(_listId, ct);
                        await botClient.SendMessage(
                            chat,
                            "Список (категория) для задач успешно удален.",
                            replyMarkup: replyKeyboardDefault,
                            cancellationToken: ct);
                        break;
                    case "no":
                        scenarioResult = ScenarioResult.Completed;
                        context.CurrentStep = "Сценарий завершен.";

                        await botClient.SendMessage(
                            chat,
                            "Удаление списка (категории) для задач отменено.",
                            replyMarkup: replyKeyboardDefault,
                            cancellationToken: ct);
                        break;
                    default:
                        break;
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
            if (update == null)
                return scenarioResult;

            var chat = UpdateHandler.GetChatFromUpdate(update);
            var currentStep = context.CurrentStep;
            ReplyKeyboardMarkup _replyKeyboard = await UpdateHandler.CreateKeyboardMarkupCancel();
            ReplyKeyboardMarkup _replyKeyboardDefault = await UpdateHandler.CreateKeyboardMarkupDefault();
            var userInput = UpdateHandler.GetMessageFromUpdate(update);
            var userFromUpdate = UpdateHandler.GetUserFromUpdate(update);
            var toDoUser = await _userService.GetUser(userFromUpdate.Id, ct);
            if (toDoUser == null)
                return scenarioResult;

            switch (currentStep)
            {
                case null:
                    // Создать inline-кнопки для выбора списка (категории) для задачи.
                    InlineKeyboardMarkup inlineKeyboardForDelete = new InlineKeyboardMarkup();
                    // Добавить списки (категории) для задач, если есть в хранилище списков (категорий) для задач.
                    var userLists = await _toDoListService.GetUserLists(toDoUser.UserId, ct);
                    foreach (var list in userLists)
                    {
                        inlineKeyboardForDelete.AddNewRow(
                            new[]
                            {
                                    InlineKeyboardButton.WithCallbackData(text: list.Name, callbackData: $"SelectList|{list.Id}"),
                            });
                    }
                    await botClient.SendMessage(chat, "Выберете список для удаления:", replyMarkup: inlineKeyboardForDelete, cancellationToken: ct);
                    context.CurrentStep = "Approve";
                    break;
                case "Approve":
                    try
                    {
                        var userListForDelete = await _toDoListService.Get(_listId, ct);

                        #region Inline-клавиатура.

                        // Создаем клавиатуру
                        InlineKeyboardMarkup inlineKeyboardDeleteApprove = new(
                            new[]
                            {
                                // Первый ряд кнопок.
                                new[]
                                {
                                    InlineKeyboardButton.WithCallbackData(text: "✅Да", callbackData: "yes"),
                                    InlineKeyboardButton.WithCallbackData(text: "❌Нет", callbackData: "no"),
                                },
                            });

                        // Отправляем сообщение с прикрепленной клавиатурой.
                        Message message1 = await botClient.SendMessage(
                            chat,
                            text: $"Подтверждаете удаление списка {userListForDelete?.Name} и всех его задач?",
                            replyMarkup: inlineKeyboardDeleteApprove,
                            cancellationToken: ct
                        );

                        #endregion

                        context.CurrentStep = "Delete";
                    }
                    catch (Exception ex)
                    {
                        await botClient.SendMessage(chat, ex.Message, replyMarkup: _replyKeyboard, cancellationToken: ct);
                        switch (currentStep)
                        {
                            case "Name":
                                await botClient.SendMessage(chat, "Введите название списка:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                                break;
                        }
                    }
                    break;
                case "Delete":
                    switch (update.CallbackQuery.Data)
                    {
                        case "yes":
                            // Получить задачи со списком (категории) для задач пользователя.
                            var toDoLists = await _toDoListService.GetUserLists(toDoUser.UserId, ct);
                            var toDoList = toDoLists.Where(x => x.Name == update.Message.Text).FirstOrDefault(); // TODO VS Проверить, по идее нужен текст на inline-кнопке.
                            var tasks = await _toDoService.GetByUserIdAndList(toDoUser.UserId, new Guid(), ct); // TODO VS Где взять ToDoList Guid?
                            // Удалить эти задачи.
                            foreach (var task in tasks)
                                await _toDoService.Delete(task.Id, ct);
                            // Удалить список (категорию) для задач пользователя.
                            await _toDoListService.Delete(toDoList.Id, ct);
                            break;
                        case "no":
                            await botClient.SendMessage(
                                chat,
                                "Удаление списка (категории) для задач отменено.",
                                replyMarkup: _replyKeyboardDefault,
                                cancellationToken: ct);
                            scenarioResult = ScenarioResult.Completed;
                            break;
                        default:
                            break;
                    }

                    scenarioResult = ScenarioResult.Completed;
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
