using System.Globalization;
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
        ToDoItem _toDoItem;
        string _lastTaskDescription = string.Empty;

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
            ReplyKeyboardMarkup _replyKeyboard = await CreateKeyboardMarkupInScenario();
            var userInput = update.Message.Text;

            switch (currentStep)
            {
                case null:
                    context.Data.Add(toDoUser.TelegramUserId.ToString(), toDoUser); // TODO VS Какой должен быть ключ? Возможно ключ toDoUser.UserId. Хранить toDoUser.
                    await bot.SendMessage(chat, "Введите название задачи:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                    context.CurrentStep = "Name";
                    break;
                case "Name":
                    try
                    {
                        object? user;
                        context.Data.TryGetValue(update.Message.From.Id.ToString(), out user);
                        var toDoUserForAddTask = user as ToDoUser;
                        var task = await _toDoService.Add(toDoUserForAddTask, userInput, DateTime.Now, ct);
                        if (task == null)
                        {
                            await bot.SendMessage(
                                chat,
                                $"Нужно добавить описание задачи: {BotConstants.CommandAddTask} [Описание задачи] или создано слишком много задач.",
                                cancellationToken: ct);
                            break;
                        }

                        _toDoItem = task;
                        _lastTaskDescription = userInput;
                        context.CurrentStep = "Deadline";
                        await bot.SendMessage(chat, "Введите срок выполнения (dd.MM.yyyy):", replyMarkup: _replyKeyboard, cancellationToken: ct);
                    }
                    catch (Exception ex)
                    {
                        await bot.SendMessage(chat, ex.Message, replyMarkup: _replyKeyboard, cancellationToken: ct);
                        switch (currentStep)
                        {
                            case "Name":
                                await bot.SendMessage(chat, "Введите название задачи:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                                break;
                            case "Deadline":
                                await bot.SendMessage(chat, "Введите срок выполнения (dd.MM.yyyy):", replyMarkup: _replyKeyboard, cancellationToken: ct);
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
                        await bot.SendMessage(chat, "Введите срок выполнения (dd.MM.yyyy):", replyMarkup: _replyKeyboard, cancellationToken: ct);
                        break;
                    }

                    _toDoItem.Deadline = deadline;
                    scenarioResult = ScenarioResult.Completed;
                    await bot.SendMessage(chat, "Задача добавлена.", cancellationToken: ct);
                    break;
                case "Cancel":
                    // Если задачи создана на первом этапе (), удалить ее.
                    var tempTask = await _toDoService.Find(toDoUser, _lastTaskDescription, ct);
                    if (tempTask != null)
                        await _toDoService.Delete(tempTask.FirstOrDefault().Id, ct);

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
