using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
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

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient botClient, ScenarioContext context, Update update, CancellationToken ct)
        {
            var scenarioResult = ScenarioResult.Transition;
            var chat = UpdateHandler.GetChatFromUpdate(update);
            var telegramUser = UpdateHandler.GetUserFromUpdate(update);
            var toDoUser = await _userService.GetUser(telegramUser.Id, ct);
            var userFromUpdate = UpdateHandler.GetUserFromUpdate(update);
            var currentStep = context.CurrentStep;
            ReplyKeyboardMarkup _replyKeyboard = await UpdateHandler.CreateKeyboardMarkupCancel();
            ReplyKeyboardMarkup _replyKeyboardDefault = await UpdateHandler.CreateKeyboardMarkupDefault();
            var userInput = UpdateHandler.GetMessageFromUpdate(update);

            switch (currentStep)
            {
                case null:
                    context.Data.Add(toDoUser.TelegramUserId.ToString(), toDoUser); // TODO VS Какой должен быть ключ? Возможно ключ toDoUser.UserId. Хранить toDoUser.
                    await botClient.SendMessage(chat, "Введите название списка:", replyMarkup: _replyKeyboard, cancellationToken: ct);
                    context.CurrentStep = "Name";
                    break;
                case "Name":
                    try
                    {
                        // Получить пользователя из контекста.
                        var toDoList = await _toDoListService.Add(toDoUser, userInput, ct);
                        if (toDoList == null)
                        {
                            await botClient.SendMessage(
                                chat,
                                $"Список (категория) для задач не создан. Сообщение: {userInput}.)",
                                cancellationToken: ct);
                            break;
                        }

                        context.CurrentStep = "Список (категория) для задач создан.";
                        scenarioResult = ScenarioResult.Completed;
                        await botClient.SendMessage(chat, "Список (категория) для задач добавлен.", replyMarkup: _replyKeyboardDefault, cancellationToken: ct);
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
