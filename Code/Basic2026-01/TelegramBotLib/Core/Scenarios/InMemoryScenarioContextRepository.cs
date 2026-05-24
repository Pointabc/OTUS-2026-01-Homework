namespace TelegramBotLib.Core.Scenarios
{
    internal class InMemoryScenarioContextRepository : IScenarioContextRepository
    {
        Dictionary<long, ScenarioContext> _contextRepository = new Dictionary<long, ScenarioContext>();
        public async Task<ScenarioContext?> GetContext(long userId, CancellationToken ct)
        {
            ScenarioContext? scenario = null;
            _contextRepository.TryGetValue(userId, out scenario);

            return scenario;
        }

        public async Task ResetContext(long userId, CancellationToken ct)
        {
            _contextRepository.Remove(userId);
        }

        public async Task SetContext(long userId, ScenarioContext context, CancellationToken ct)
        {
            _contextRepository[userId] = context;
        }
    }
}
