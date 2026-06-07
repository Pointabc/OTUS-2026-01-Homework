using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotLib.Core.Entities;
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

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            var scenarioResult = ScenarioResult.Transition;
            var chat = UpdateHandler.GetChatFromUpdate(update);
            var currentStep = context.CurrentStep;
            ReplyKeyboardMarkup _replyKeyboard = await CreateKeyboardMarkupInScenario();
            ReplyKeyboardMarkup _replyKeyboardDefault = await CreateKeyboardMarkup();
            //var userInput = update.Message.Text;
            var userFromUpdate = UpdateHandler.GetUserFromUpdate(update);
            var toDoUser = await _userService.GetUser(userFromUpdate.Id, ct);

            switch (currentStep)
            {
                case null:
                    context.Data.Add(toDoUser.TelegramUserId.ToString(), toDoUser); // TODO VS Какой должен быть ключ? Возможно ключ toDoUser.UserId. Хранить toDoUser.
                    await bot.SendMessage(chat, "Выберете список для удаления:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                    context.CurrentStep = "Approve";
                    break;
                case "Approve":
                    try
                    {
                        /*Получить ToDoList и сохранить его в ScenarioContext.Data.
                        Отправить пользователю сообщение "Подтверждаете удаление списка {toDoList.Name} и всех его задач" 
                        с Inline кнопками: WithCallbackData("✅Да", "yes"), WithCallbackData("❌Нет", "no")
                        Обновить ScenarioContext.CurrentStep на "Delete"*/

                        // Получить пользователя из контекста.
                        object? user;
                        context.Data.TryGetValue(update.Message.From.Id.ToString(), out user);
                        var toDoUserForDeleteList = user as ToDoUser;
                        var toDoList = await _toDoListService.Add(toDoUserForDeleteList, update.Message.Text, ct);
                        if (toDoList == null)
                        {
                            await bot.SendMessage(
                                chat,
                                $"Список (категория) для задач не добавлен в репозиторий. Сообщение: {update.Message.Text}.)",
                                cancellationToken: ct);
                            break;
                        }

                        #region Inline-клавиатура.

                        // Создаем клавиатуру
                        InlineKeyboardMarkup inlineKeyboard = new(
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
                        Message message1 = await bot.SendMessage(
                            chat,
                            text: $"Подтверждаете удаление списка {toDoList.Name} и всех его задач",
                            replyMarkup: inlineKeyboard,
                            cancellationToken: ct
                        );

                        #endregion

                        context.CurrentStep = "Delete";
                    }
                    catch (Exception ex)
                    {
                        // TODO VS 07062026 Тут переформулировать сообщение.
                        await bot.SendMessage(chat, ex.Message, replyMarkup: _replyKeyboard, cancellationToken: ct);
                        switch (currentStep)
                        {
                            case "Name":
                                await bot.SendMessage(chat, "Введите название списка:", replyMarkup: _replyKeyboard, cancellationToken: ct);
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
                            await bot.SendMessage(
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
                default:
                    break;
            }

            return scenarioResult;
        }

        /// <summary>
        /// Создать клавиатуру во время обработки сценариев.
        /// </summary>
        /// <returns>Клавиатура.</returns>
        private async Task<ReplyKeyboardMarkup> CreateKeyboardMarkupInScenario()
        {
            var buttons = new List<KeyboardButton[]>();
            buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandCancel) });
            return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
        }

        /// <summary>
        /// Создать клавиатуру по умолчанию.
        /// </summary>
        /// <returns>Клавиатура по умолчанию.</returns>
        private async Task<ReplyKeyboardMarkup> CreateKeyboardMarkup()
        {
            var buttons = new List<KeyboardButton[]>();
            buttons.Add(new KeyboardButton[] { new KeyboardButton("/start") });
            buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandAddTask) });
            buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandShowTasks) });
            buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandReport) });

            return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
        }
    }
}
