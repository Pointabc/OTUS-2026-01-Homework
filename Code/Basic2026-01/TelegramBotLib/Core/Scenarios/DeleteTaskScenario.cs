using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBotLib.Core.Services;
using TelegramBotLib.DTO;
using TelegramBotLib.TelegramBot;

namespace TelegramBotLib.Core.Scenarios
{
    internal class DeleteTaskScenario : IScenario
    {
        IToDoService _toDoService;
        Guid _taskId = Guid.Empty; // Для хранения Guid задачи при подтверждении удаления.

        public DeleteTaskScenario(IToDoService toDoService)
        {
            _toDoService = toDoService;
        }

        public bool CanHandle(ScenarioType scenarioType)
        {
            return scenarioType == ScenarioType.DeleteTask;
        }

        public async Task<ScenarioResult> HandleMessageAsync(ITelegramBotClient bot, ScenarioContext context, Update update, CancellationToken ct)
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

            if (callbackQuery.Data == null)
                return scenarioResult;

            var toDoItemCallbackDto = ToDoItemCallbackDto.FromString(callbackQuery.Data);
            var chat = UpdateHandler.GetChatFromUpdate(update);
            var replyKeyboardDefault = await UpdateHandler.CreateKeyboardMarkupDefault();
            var telegramUser = UpdateHandler.GetUserFromUpdate(update);
            var toDoUser = telegramUser; // await _userService.GetUser(telegramUser.Id, ct);
            if (toDoUser == null)
                return scenarioResult;

            switch (toDoItemCallbackDto.Action)
            {
                case "deletetask":
                    if (toDoItemCallbackDto.ToDoItemId == null)
                        break;

                    var task = await _toDoService.Get((Guid)toDoItemCallbackDto.ToDoItemId, ct);
                    var taskName = task?.Name;
                    // Для хранения Guid задачи при подтверждении удаления.
                    _taskId = (Guid)toDoItemCallbackDto.ToDoItemId;

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
                        text: $"Подтверждаете удаление задачи {taskName}?",
                        replyMarkup: inlineKeyboardDeleteApprove,
                        cancellationToken: ct
                    );

                    context.CurrentStep = "Delete";
                    scenarioResult = ScenarioResult.Transition;

                    return scenarioResult;
                case "yes":
                    scenarioResult = ScenarioResult.Completed;
                    // Удалить задачу.
                    var taskForDelete = await _toDoService.Get(_taskId, ct);
                    var taskForDeleteName = taskForDelete?.Name;
                    await _toDoService.Delete(_taskId, ct);

                    await botClient.SendMessage(
                        chat,
                        $"Задача {taskForDeleteName} успешно удалена.",
                        replyMarkup: replyKeyboardDefault,
                        cancellationToken: ct);
                    break;
                case "no":
                    scenarioResult = ScenarioResult.Completed;
                    context.CurrentStep = "Сценарий завершен.";

                    await botClient.SendMessage(
                        chat,
                        "Удаление задачи отменено.",
                        replyMarkup: replyKeyboardDefault,
                        cancellationToken: ct);
                    break;
                default:
                    break;
            }


            return scenarioResult;
        }

        private async Task<ScenarioResult> OnMessage(ITelegramBotClient botClient, Update update, Message message, ScenarioContext context, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        private async Task<ScenarioResult> OnUnknown(Update update)
        {
            throw new NotImplementedException();
        }
    }
}
