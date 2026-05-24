using Telegram.Bot;
using Telegram.Bot.Types;
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

        public bool CanHandle(ScenarioType scenario)
        {
            return scenario == ScenarioType.AddTask;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
        {
            var scenarioResult = ScenarioResult.Transition;
            var toDoUser = await _userService.GetUser(update.Message.From.Id, ct);
            
            /*var toDoUser1 = await _userService.GetUser(context.UserId, ct);
            object? user;
            context.Data.TryGetValue(update.Message.From.Id.ToString(), out user);
            ToDoUser toDoUser = (ToDoUser)user;
            if (toDoUser == null)
                return scenarioResult;*/

            var chat = update.Message.Chat;
            var currentStep = context.CurrentStep;

            switch(currentStep)
            {
                case null:
                    context.Data.Add(toDoUser.TelegramUserId.ToString(), toDoUser); // TODO VS Какой должен быть ключ? Возможно toDoUser.UserId. Что тут хранить, не понятно зачем.
                    await bot.SendMessage(chat, "Введите название задачи:", cancellationToken: ct);
                    context.CurrentStep = "Name";
                    break;
                case "Name":
                    // TODO VS Передать ToDoUser из ScenarioContext.Data и name из сообщения ???
                    
                    var task = await _toDoService.Add(toDoUser, update.Message.Text, ct);
                    if (task == null)
                    {
                        await bot.SendMessage(
                            chat,
                            $"Нужно добавить описание задачи: {BotConstants.CommandAddTask} [Описание задачи] или создано слишком много задач.",
                            cancellationToken: ct);
                        context.CurrentStep = "Сценарий завершен.";
                        break;
                    }
                    
                    scenarioResult = ScenarioResult.Completed;
                    context.CurrentStep = "Сценарий завершен.";
                    await bot.SendMessage(chat, "Задача добавлена.", cancellationToken: ct);
                    break;
                default:
                    break;
            }

            return scenarioResult;
        }
    }
}
