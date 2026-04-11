namespace RoguelikeGame.Shared.Protocol
{
    public class GameAction
    {
        public string Type { get; set; } = "";
        public string PlayerId { get; set; } = "";
        public string? TargetId { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
    }

    public class GameStateUpdate
    {
        public int TurnNumber { get; set; }
        public string CurrentPlayerId { get; set; } = "";
        public Dictionary<string, object> State { get; set; } = new();
    }

    public class GameResult
    {
        public bool Victory { get; set; }
        public string WinnerId { get; set; } = "";
        public Dictionary<string, int> Scores { get; set; } = new();
    }
}
