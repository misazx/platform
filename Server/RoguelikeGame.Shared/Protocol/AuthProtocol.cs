namespace RoguelikeGame.Shared.Protocol
{
    public class AuthRequest
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string? Email { get; set; }
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string? Error { get; set; }
        public UserInfo? User { get; set; }
    }

    public class UserInfo
    {
        public string Id { get; set; } = "";
        public string Username { get; set; } = "";
        public int Level { get; set; }
        public int TotalGamesPlayed { get; set; }
        public int GamesWon { get; set; }
    }
}
