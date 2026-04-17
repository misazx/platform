using BTTree = RoguelikeGame.Shared.BehaviorTree.BehaviorTree;

namespace RoguelikeGame.Shared.Bots
{
    public class BotInstance
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public BotProfile Profile { get; }
        public BTTree Tree { get; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; } = DateTime.UtcNow;

        public BotInstance(BotProfile profile, BTTree tree)
        {
            Profile = profile;
            Tree = tree;
            IsActive = true;
        }

        public BehaviorTree.BTNodeStatus Tick(double deltaTime)
        {
            if (!IsActive) return BehaviorTree.BTNodeStatus.Failure;
            return Tree.Tick(deltaTime);
        }

        public void Deactivate()
        {
            IsActive = false;
            Tree.Abort();
        }
    }

    public class BotManager
    {
        private readonly Dictionary<string, BotInstance> _bots = new();
        private readonly Dictionary<string, Func<BotProfile, BTTree>> _treeFactories = new();

        public IReadOnlyDictionary<string, BotInstance> Bots => _bots;

        public void RegisterBehaviorTreeFactory(string gameMode, Func<BotProfile, BTTree> factory)
        {
            _treeFactories[gameMode] = factory;
        }

        public BotInstance? CreateBot(BotProfile profile)
        {
            if (!_treeFactories.TryGetValue(profile.GameMode, out var factory))
            {
                if (_treeFactories.TryGetValue("", out var defaultFactory))
                {
                    factory = defaultFactory;
                }
                else
                {
                    return null;
                }
            }

            var tree = factory(profile);
            var bot = new BotInstance(profile, tree);
            _bots[bot.Id] = bot;

            return bot;
        }

        public bool RemoveBot(string botId)
        {
            if (_bots.TryGetValue(botId, out var bot))
            {
                bot.Deactivate();
                _bots.Remove(botId);
                return true;
            }
            return false;
        }

        public void RemoveBotsByRoom(string roomId)
        {
            var botsToRemove = _bots.Where(kvp =>
                kvp.Value.Profile.BehaviorTreeConfig.Contains(roomId)).ToList();

            foreach (var kvp in botsToRemove)
            {
                kvp.Value.Deactivate();
                _bots.Remove(kvp.Key);
            }
        }

        public void TickAll(double deltaTime)
        {
            foreach (var bot in _bots.Values.Where(b => b.IsActive))
            {
                bot.Tick(deltaTime);
            }
        }

        public List<BotInstance> GetBotsByRoom(string roomId)
        {
            return _bots.Values
                .Where(b => b.Profile.BehaviorTreeConfig.Contains(roomId))
                .ToList();
        }

        public void ClearAll()
        {
            foreach (var bot in _bots.Values)
            {
                bot.Deactivate();
            }
            _bots.Clear();
        }
    }
}
