using Microsoft.AspNetCore.SignalR;
using RoguelikeGame.Server.Hubs;
using RoguelikeGame.Shared.BehaviorTree;
using RoguelikeGame.Shared.Bots;

namespace RoguelikeGame.Server.Services
{
    public interface IBotGameService
    {
        void RegisterRoomBot(string roomId, string botUserId, string botName, BotDifficulty difficulty, int playerIndex);
        void UnregisterRoomBot(string roomId, string botUserId);
        void UpdateBotGameState(string roomId, string botUserId, Dictionary<string, object> gameState);
        void UnregisterRoomBots(string roomId);
    }

    public class BotGameService : BackgroundService, IBotGameService
    {
        private readonly IHubContext<GameHub> _hubContext;
        private readonly ILogger<BotGameService> _logger;
        private readonly BotManager _botManager;
        private readonly Dictionary<string, RoomBotContext> _roomBots = new();

        public BotGameService(IHubContext<GameHub> hubContext, ILogger<BotGameService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
            _botManager = new BotManager();
            _botManager.RegisterBehaviorTreeFactory("PvP", profile => CardBotAIFactory.CreateBehaviorTree(profile.Difficulty));
            _botManager.RegisterBehaviorTreeFactory("Coop", profile => CardBotAIFactory.CreateBehaviorTree(profile.Difficulty));
            _botManager.RegisterBehaviorTreeFactory("", profile => CardBotAIFactory.CreateBehaviorTree(profile.Difficulty));
        }

        public void RegisterRoomBot(string roomId, string botUserId, string botName, BotDifficulty difficulty, int playerIndex)
        {
            var profile = new BotProfile
            {
                Name = botName,
                Difficulty = difficulty,
                BehaviorTreeConfig = roomId,
                GameMode = ""
            };

            var bot = _botManager.CreateBot(profile);
            if (bot != null)
            {
                _roomBots[$"{roomId}:{botUserId}"] = new RoomBotContext
                {
                    RoomId = roomId,
                    BotUserId = botUserId,
                    BotName = botName,
                    PlayerIndex = playerIndex,
                    BotInstanceId = bot.Id,
                    LastActionTime = DateTime.UtcNow,
                    ActionCooldown = TimeSpan.FromSeconds(1.5)
                };

                _logger.LogInformation("Bot AI registered: {BotName} in room {RoomId}", botName, roomId);
            }
        }

        public void UnregisterRoomBot(string roomId, string botUserId)
        {
            var key = $"{roomId}:{botUserId}";
            if (_roomBots.TryGetValue(key, out var ctx))
            {
                _botManager.RemoveBot(ctx.BotInstanceId);
                _roomBots.Remove(key);
            }
        }

        public void UnregisterRoomBots(string roomId)
        {
            var keys = _roomBots.Where(kvp => kvp.Value.RoomId == roomId).Select(kvp => kvp.Key).ToList();
            foreach (var key in keys)
            {
                if (_roomBots.TryGetValue(key, out var ctx))
                {
                    _botManager.RemoveBot(ctx.BotInstanceId);
                }
                _roomBots.Remove(key);
            }
        }

        public void UpdateBotGameState(string roomId, string botUserId, Dictionary<string, object> gameState)
        {
            var key = $"{roomId}:{botUserId}";
            if (!_roomBots.TryGetValue(key, out var ctx)) return;

            var bot = _botManager.Bots.GetValueOrDefault(ctx.BotInstanceId);
            if (bot == null || !bot.IsActive) return;

            var bb = bot.Tree.Blackboard;
            if (gameState.TryGetValue("hand", out var hand)) bb.Set(BotBBKeys.Hand, hand);
            if (gameState.TryGetValue("player_hp", out var hp)) bb.Set(BotBBKeys.PlayerHp, hp);
            if (gameState.TryGetValue("player_max_hp", out var maxHp)) bb.Set(BotBBKeys.PlayerMaxHp, maxHp);
            if (gameState.TryGetValue("player_energy", out var energy)) bb.Set(BotBBKeys.PlayerEnergy, energy);
            if (gameState.TryGetValue("player_block", out var block)) bb.Set(BotBBKeys.PlayerBlock, block);
            if (gameState.TryGetValue("enemies", out var enemies)) bb.Set(BotBBKeys.Enemies, enemies);
            if (gameState.TryGetValue("potions", out var potions)) bb.Set(BotBBKeys.Potions, potions);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BotGameService started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);

                    foreach (var kvp in _roomBots.ToList())
                    {
                        var ctx = kvp.Value;
                        if (DateTime.UtcNow - ctx.LastActionTime < ctx.ActionCooldown) continue;

                        var bot = _botManager.Bots.GetValueOrDefault(ctx.BotInstanceId);
                        if (bot == null || !bot.IsActive) continue;

                        var bb = bot.Tree.Blackboard;
                        var actionType = bb.Get<string>(BotBBKeys.ActionType);

                        if (string.IsNullOrEmpty(actionType)) continue;

                        try
                        {
                            switch (actionType)
                            {
                                case BotActions.PlayCard:
                                    var card = bb.Get<CardData>(BotBBKeys.SelectedCard);
                                    var target = bb.Get<string>(BotBBKeys.SelectedTarget);
                                    if (card != null)
                                    {
                                        await _hubContext.Clients.Group(ctx.RoomId).SendAsync("CoopCardPlayed",
                                            System.Text.Json.JsonSerializer.Serialize(new
                                            {
                                                playerIndex = ctx.PlayerIndex,
                                                cardData = new { card.Id, card.Name, card.Cost, Type = card.Type.ToString(), card.Damage, card.Block, TargetType = card.TargetType.ToString() },
                                                targetIndex = target ?? "",
                                                isBot = true
                                            }), cancellationToken: stoppingToken);

                                        ctx.LastActionTime = DateTime.UtcNow;
                                        ctx.ActionCooldown = TimeSpan.FromSeconds(1.0);
                                    }
                                    break;

                                case BotActions.EndTurn:
                                    await _hubContext.Clients.Group(ctx.RoomId).SendAsync("CoopTurnEnded",
                                        System.Text.Json.JsonSerializer.Serialize(new
                                        {
                                            playerIndex = ctx.PlayerIndex,
                                            isBot = true
                                        }), cancellationToken: stoppingToken);

                                    ctx.LastActionTime = DateTime.UtcNow;
                                    ctx.ActionCooldown = TimeSpan.FromSeconds(2.0);
                                    break;

                                case BotActions.UsePotion:
                                    var potion = bb.Get<PotionData>(BotBBKeys.SelectedPotion);
                                    if (potion != null)
                                    {
                                        await _hubContext.Clients.Group(ctx.RoomId).SendAsync("CoopCardPlayed",
                                            System.Text.Json.JsonSerializer.Serialize(new
                                            {
                                                playerIndex = ctx.PlayerIndex,
                                                cardData = new { Id = potion.Id, Name = potion.Name, Cost = 0, Type = "POTION", Damage = 0, Block = 0, TargetType = "SELF" },
                                                targetIndex = "",
                                                isBot = true
                                            }), cancellationToken: stoppingToken);

                                        ctx.LastActionTime = DateTime.UtcNow;
                                        ctx.ActionCooldown = TimeSpan.FromSeconds(1.0);
                                    }
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("Bot action broadcast failed: {Message}", ex.Message);
                        }
                        finally
                        {
                            bb.Set<string>(BotBBKeys.ActionType, "");
                            bb.Set<CardData?>(BotBBKeys.SelectedCard, null);
                            bb.Set<string>(BotBBKeys.SelectedTarget, "");
                            bb.Set<PotionData?>(BotBBKeys.SelectedPotion, null);
                        }
                    }
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "BotGameService error");
                }
            }
        }
    }

    public class RoomBotContext
    {
        public string RoomId { get; set; } = "";
        public string BotUserId { get; set; } = "";
        public string BotName { get; set; } = "";
        public int PlayerIndex { get; set; }
        public string BotInstanceId { get; set; } = "";
        public DateTime LastActionTime { get; set; }
        public TimeSpan ActionCooldown { get; set; } = TimeSpan.FromSeconds(1.5);
    }
}
