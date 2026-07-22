using Telegram.Bot;
using TelegramBotLib.Core.Scenarios;
using TelegramBotLib.TelegramBot;

namespace TelegramBotLib.Core.BackgroundTasks;

internal class ResetScenarioBackgroundTask : BackgroundTask
{
    ITelegramBotClient _bot { get; set; }
    IScenarioContextRepository _scenarioRepository { get; set; }
    TimeSpan _resetScenarioTimeout { get; set; }

    public ResetScenarioBackgroundTask(
        TimeSpan resetScenarioTimeout,
        IScenarioContextRepository scenarioRepository,
        ITelegramBotClient bot)
        : base(resetScenarioTimeout, nameof(ResetScenarioBackgroundTask))
    {
        _resetScenarioTimeout = resetScenarioTimeout != TimeSpan.Zero
            ? resetScenarioTimeout
            : throw new ArgumentNullException();
        _scenarioRepository = scenarioRepository ?? throw new ArgumentNullException();
        _bot = bot ?? throw new ArgumentNullException();
    }

    protected override async Task Execute(CancellationToken ct)
    {
        var contexts = await _scenarioRepository.GetContexts(CancellationToken.None);

        foreach (var context in contexts)
        {
            var timeDelta = DateTime.UtcNow - context.CreatedAt;
            if (timeDelta > this. _resetScenarioTimeout)
            {
                await _scenarioRepository.ResetContext(context.UserId, CancellationToken.None);
                // Получить chatId из context.Data.
                var chatId = context.Data.ContainsKey(BotConstants.KeyUserIdName)
                    ? context.Data[BotConstants.KeyUserIdName]
                    : null;
                if (chatId == null)
                    throw new ArgumentNullException(nameof(chatId));

                var keyboardMarkup = await UpdateHandler.CreateKeyboardMarkupDefault();
                
                await _bot.SendMessage(
                    (long)chatId, 
                    $"Сценарий отменен, так как не поступил ответ в течение {_resetScenarioTimeout}",
                    replyMarkup: keyboardMarkup,
                    cancellationToken: CancellationToken.None);
            }
        }
    }
}
