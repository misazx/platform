namespace RoguelikeGame.Shared.Bots
{
    public enum BotDifficulty
    {
        Easy,
        Normal,
        Hard
    }

    public class BotProfile
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = "";
        public BotDifficulty Difficulty { get; set; } = BotDifficulty.Normal;
        public string BehaviorTreeConfig { get; set; } = "";
        public string GameMode { get; set; } = "";

        private static readonly string[] BOT_NAMES = {
            "AI-Alpha", "AI-Bravo", "AI-Charlie", "AI-Delta",
            "AI-Echo", "AI-Foxtrot", "AI-Golf", "AI-Hotel"
        };

        private static int _nameIndex;

        public static BotProfile CreateDefault(string gameMode = "", BotDifficulty difficulty = BotDifficulty.Normal)
        {
            var name = BOT_NAMES[_nameIndex % BOT_NAMES.Length];
            _nameIndex++;

            return new BotProfile
            {
                Name = name,
                Difficulty = difficulty,
                GameMode = gameMode,
                BehaviorTreeConfig = $"default_{difficulty.ToString().ToLower()}"
            };
        }
    }
}
