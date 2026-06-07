using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotLib.Core.Entities;
using TelegramBotLib.Core.Services;
using TelegramBotLib.TelegramBot;

namespace TelegramBotLib.Core.Scenarios
{
    internal class AddListScenario : IScenario
    {
        IUserService _userService;
        IToDoListService _toDoListService;

        public AddListScenario(IUserService userService, IToDoListService toDoListService)
        {
            _userService = userService;
            _toDoListService = toDoListService;
        }

        public bool CanHandle(ScenarioType scenarioType)
        {
            return scenarioType == ScenarioType.AddList;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            var scenarioResult = ScenarioResult.Transition;
            var chat = UpdateHandler.GetChatFromUpdate(update);
            var userFromUpdate = UpdateHandler.GetUserFromUpdate(update);
            var currentStep = context.CurrentStep;
            ReplyKeyboardMarkup _replyKeyboard = await CreateKeyboardMarkupInScenario();
            ReplyKeyboardMarkup _replyKeyboardDefault = await CreateKeyboardMarkup();
            var userInput = UpdateHandler.GetMessageFromUpdate(update);

            switch (currentStep)
            {
                case null:
                    var toDoUser = await _userService.GetUser(userFromUpdate.Id, ct);
                    context.Data.Add(toDoUser.TelegramUserId.ToString(), toDoUser); // TODO VS Какой должен быть ключ? Возможно ключ toDoUser.UserId. Хранить toDoUser.
                    await bot.SendMessage(chat, "Введите название списка:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                    context.CurrentStep = "Name";
                    break;
                case "Name":
                    try
                    {
                        // Получить пользователя из контекста.
                        object? user;
                        context.Data.TryGetValue(userFromUpdate.Id.ToString(), out user);
                        var toDoUserForAddList = user as ToDoUser;
                        var toDoList = await _toDoListService.Add(toDoUserForAddList, userInput, ct);
                        if (toDoList == null)
                        {
                            await bot.SendMessage(
                                chat,
                                $"Список (категория) для задач не создан. Сообщение: {userInput}.)",
                                cancellationToken: ct);
                            break;
                        }

                        context.CurrentStep = "Список (категория) для задач создан.";
                        scenarioResult = ScenarioResult.Completed;
                        await bot.SendMessage(chat, "Список (категория) для задач добавлен.", replyMarkup: _replyKeyboardDefault, cancellationToken: ct);
                    }
                    catch (Exception ex)
                    {
                        await bot.SendMessage(chat, ex.Message, replyMarkup: _replyKeyboard, cancellationToken: ct);
                        switch (currentStep)
                        {
                            case "Name":
                                await bot.SendMessage(chat, "Введите название списка:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                                break;
                        }
                    }
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
