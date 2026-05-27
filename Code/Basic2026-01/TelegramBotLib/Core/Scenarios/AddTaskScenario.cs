using Telegram.Bot;
using Telegram.Bot.Types;
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

        public AddTaskScenario(IUserService userService, IToDoService toDoService)
        {
            _userService = userService;
            _toDoService = toDoService;
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
            var scenarioResult = ScenarioResult.Transition;
            var toDoUser = await _userService.GetUser(update.Message.From.Id, ct);
            var chat = update.Message.Chat;
            var currentStep = context.CurrentStep;

            switch (currentStep)
            {
                case null:
                    ReplyKeyboardMarkup _replyKeyboard = await CreateKeyboardMarkupInScenario();
                    context.Data.Add(toDoUser.TelegramUserId.ToString(), toDoUser); // TODO VS Какой должен быть ключ? Возможно ключ toDoUser.UserId. Хранить toDoUser.
                    await bot.SendMessage(chat, "Введите название задачи:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                    context.CurrentStep = "Name";
                    break;
                case "Name":
                    object? user;
                    context.Data.TryGetValue(update.Message.From.Id.ToString(), out user);
                    var toDoUserForAddTask = user as ToDoUser;
                    try
                    {
                        var task = await _toDoService.Add(toDoUserForAddTask, update.Message.Text, ct);
                        if (task == null)
                        {
                            await bot.SendMessage(
                                chat,
                                $"Нужно добавить описание задачи: {BotConstants.CommandAddTask} [Описание задачи] или создано слишком много задач.",
                                cancellationToken: ct);
                            break;
                        }

                        scenarioResult = ScenarioResult.Completed;
                        context.CurrentStep = "Сценарий завершен.";
                        await bot.SendMessage(chat, "Задача добавлена.", cancellationToken: ct);
                    }
                    catch (Exception ex)
                    {
                        await bot.SendMessage(chat, ex.Message, cancellationToken: ct);
                        await bot.SendMessage(chat, "Введите название задачи:", cancellationToken: ct);
                    }
                    break;
                case "Cancel":
                    ReplyKeyboardMarkup _replyKeyboardDefault = await CreateKeyboardMarkup();
                    scenarioResult = ScenarioResult.Completed;
                    context.CurrentStep = "Сценарий завершен.";
                    await bot.SendMessage(chat, "Операция отменена.", replyMarkup: _replyKeyboardDefault, cancellationToken: ct);
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

        private async Task<ReplyKeyboardMarkup> CreateKeyboardMarkup()
        {
            var buttons = new List<KeyboardButton[]>();
            buttons.Add(new KeyboardButton[] { new KeyboardButton("/start") });
            buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandAddTask) });
            buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandShowAllTasks) });
            buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandShowTasks) });
            buttons.Add(new KeyboardButton[] { new KeyboardButton(BotConstants.CommandReport) });

            return new ReplyKeyboardMarkup(buttons) { ResizeKeyboard = true };
        }
    }
}
