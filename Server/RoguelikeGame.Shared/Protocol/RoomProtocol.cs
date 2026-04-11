namespace RoguelikeGame.Shared.Protocol
{
    public enum RoomStatus
    {
        Waiting,
        Full,
        Ready,
        Playing,
        Finished
    }

    public enum GameMode
    {
        PvP,
        PvE,
        Coop
    }

    public class RoomInfo
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string HostId { get; set; } = "";
        public string HostName { get; set; } = "";
        public RoomStatus Status { get; set; }
        public GameMode Mode { get; set; }
        public int MaxPlayers { get; set; }
        public int CurrentPlayers { get; set; }
        public bool HasPassword { get; set; }
    }

    public class CreateRoomRequest
    {
        public string Name { get; set; } = "";
        public GameMode Mode { get; set; }
        public int MaxPlayers { get; set; } = 4;
        public string? Password { get; set; }
    }

    public class JoinRoomRequest
    {
        public string RoomId { get; set; } = "";
        public string? Password { get; set; }
    }

    public class RoomPlayerInfo
    {
        public string UserId { get; set; } = "";
        public string Username { get; set; } = "";
        public bool IsReady { get; set; }
        public string? CharacterId { get; set; }
    }
}
