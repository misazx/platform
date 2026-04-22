using Microsoft.AspNetCore.SignalR;
using RoguelikeGame.Server.Hubs;
using RoguelikeGame.Server.Models;
using RoguelikeGame.Shared.BehaviorTree;
using RoguelikeGame.Shared.Bots;

namespace RoguelikeGame.Server.Services
{
    public interface IBotGameService
    {
        void RegisterRoomBot(string roomId, string botUserId, string botName, BotDifficulty difficulty, int playerIndex, string gameModeId = "", GameMode roomMode = GameMode.PvP);
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

        public const string PackageIdBaseGame = "base_game";
        public const string PackageIdLightShadow = "light_shadow_traveler";
        public const string BotModeLightShadowSolo = "light_shadow_traveler_solo";
        public const string BotModeLightShadowRace = "light_shadow_traveler_race";
        public const string BotModeLightShadowCoop = "light_shadow_traveler_coop";

        public BotGameService(IHubContext<GameHub> hubContext, ILogger<BotGameService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
            _botManager = new BotManager();
            _botManager.RegisterBehaviorTreeFactory(PackageIdBaseGame, profile => CardBotAIFactory.CreateBehaviorTree(profile.Difficulty));
            _botManager.RegisterBehaviorTreeFactory("", profile => CardBotAIFactory.CreateBehaviorTree(profile.Difficulty));
            _botManager.RegisterBehaviorTreeFactory(BotModeLightShadowSolo, profile => LightShadowBotAIFactory.CreateBehaviorTree(profile.Difficulty, LightShadowPlayMode.Solo));
            _botManager.RegisterBehaviorTreeFactory(BotModeLightShadowRace, profile => LightShadowBotAIFactory.CreateBehaviorTree(profile.Difficulty, LightShadowPlayMode.Race));
            _botManager.RegisterBehaviorTreeFactory(BotModeLightShadowCoop, profile => LightShadowBotAIFactory.CreateBehaviorTree(profile.Difficulty, LightShadowPlayMode.Coop));
        }

        private static string ResolveBotGameMode(string gameModeId, GameMode roomMode)
        {
            if (string.IsNullOrEmpty(gameModeId) || gameModeId == PackageIdBaseGame)
            {
                return PackageIdBaseGame;
            }

            if (gameModeId == PackageIdLightShadow)
            {
                return roomMode switch
                {
                    GameMode.Coop => BotModeLightShadowCoop,
                    GameMode.PvE => BotModeLightShadowRace,
                    GameMode.Race => BotModeLightShadowRace,
                    GameMode.PvP => BotModeLightShadowRace,
                    _ => BotModeLightShadowSolo
                };
            }

            return gameModeId;
        }

        public void RegisterRoomBot(string roomId, string botUserId, string botName, BotDifficulty difficulty, int playerIndex, string gameModeId = "", GameMode roomMode = GameMode.PvP)
        {
            var resolvedMode = ResolveBotGameMode(gameModeId, roomMode);
            var profile = new BotProfile
            {
                Name = botName,
                Difficulty = difficulty,
                BehaviorTreeConfig = roomId,
                GameMode = resolvedMode
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
            var gameMode = bot.Profile.GameMode;

            if (gameMode == BotModeLightShadowSolo || gameMode == BotModeLightShadowRace || gameMode == BotModeLightShadowCoop)
            {
                // 光影旅者游戏状态
                var lsState = ParseLightShadowGameState(gameState);
                bb.Set(LightShadowBBKeys.GameState, lsState);
            }
            else
            {
                // 卡牌游戏状态
                if (gameState.TryGetValue("hand", out var hand)) bb.Set(BotBBKeys.Hand, hand);
                if (gameState.TryGetValue("player_hp", out var hp)) bb.Set(BotBBKeys.PlayerHp, hp);
                if (gameState.TryGetValue("player_max_hp", out var maxHp)) bb.Set(BotBBKeys.PlayerMaxHp, maxHp);
                if (gameState.TryGetValue("player_energy", out var energy)) bb.Set(BotBBKeys.PlayerEnergy, energy);
                if (gameState.TryGetValue("player_block", out var block)) bb.Set(BotBBKeys.PlayerBlock, block);
                if (gameState.TryGetValue("enemies", out var enemies)) bb.Set(BotBBKeys.Enemies, enemies);
                if (gameState.TryGetValue("potions", out var potions)) bb.Set(BotBBKeys.Potions, potions);
            }
        }

        private LightShadowGameState ParseLightShadowGameState(Dictionary<string, object> gameState)
        {
            var state = new LightShadowGameState();

            if (gameState.TryGetValue("player_x", out var px)) state.PlayerX = Convert.ToDouble(px);
            if (gameState.TryGetValue("player_y", out var py)) state.PlayerY = Convert.ToDouble(py);
            if (gameState.TryGetValue("player_form", out var pf)) state.PlayerForm = pf?.ToString() ?? "light";
            if (gameState.TryGetValue("player_health", out var ph)) state.PlayerHealth = Convert.ToInt32(ph);
            if (gameState.TryGetValue("player_max_health", out var pmh)) state.PlayerMaxHealth = Convert.ToInt32(pmh);
            if (gameState.TryGetValue("player_energy", out var pe)) state.PlayerEnergy = Convert.ToDouble(pe);
            if (gameState.TryGetValue("is_grounded", out var ig)) state.IsGrounded = Convert.ToBoolean(ig);
            if (gameState.TryGetValue("facing_right", out var fr)) state.FacingRight = Convert.ToBoolean(fr);

            // 解析平台数据
            if (gameState.TryGetValue("platforms", out var platformsObj) && platformsObj is System.Text.Json.JsonElement platformsJson)
            {
                foreach (var p in platformsJson.EnumerateArray())
                {
                    state.Platforms.Add(new PlatformData
                    {
                        X = p.GetProperty("x").GetDouble(),
                        Y = p.GetProperty("y").GetDouble(),
                        Width = p.GetProperty("w").GetDouble(),
                        Height = p.GetProperty("h").GetDouble(),
                        Type = p.GetProperty("type").GetString() ?? "normal"
                    });
                }
            }

            // 解析碎片数据
            if (gameState.TryGetValue("fragments", out var fragmentsObj) && fragmentsObj is System.Text.Json.JsonElement fragmentsJson)
            {
                foreach (var f in fragmentsJson.EnumerateArray())
                {
                    state.Fragments.Add(new FragmentData
                    {
                        X = f.GetProperty("x").GetDouble(),
                        Y = f.GetProperty("y").GetDouble(),
                        Collected = f.TryGetProperty("collected", out var c) && c.GetBoolean()
                    });
                }
            }

            // 解析检查点数据
            if (gameState.TryGetValue("checkpoints", out var checkpointsObj) && checkpointsObj is System.Text.Json.JsonElement checkpointsJson)
            {
                foreach (var cp in checkpointsJson.EnumerateArray())
                {
                    state.Checkpoints.Add(new CheckpointData
                    {
                        X = cp.GetProperty("x").GetDouble(),
                        Y = cp.GetProperty("y").GetDouble(),
                        Id = cp.GetProperty("id").GetString() ?? "",
                        Activated = cp.TryGetProperty("activated", out var a) && a.GetBoolean()
                    });
                }
            }

            // 解析终点
            if (gameState.TryGetValue("goal", out var goalObj) && goalObj is System.Text.Json.JsonElement goalJson)
            {
                state.Goal = new GoalData
                {
                    X = goalJson.GetProperty("x").GetDouble(),
                    Y = goalJson.GetProperty("y").GetDouble()
                };
            }

            // 解析光影区域
            if (gameState.TryGetValue("light_zones", out var lightZonesObj) && lightZonesObj is System.Text.Json.JsonElement lightZonesJson)
            {
                foreach (var lz in lightZonesJson.EnumerateArray())
                {
                    state.LightZones.Add(new LightZoneData
                    {
                        X = lz.GetProperty("x").GetDouble(),
                        Y = lz.GetProperty("y").GetDouble(),
                        Radius = lz.GetProperty("radius").GetDouble()
                    });
                }
            }

            if (gameState.TryGetValue("shadow_zones", out var shadowZonesObj) && shadowZonesObj is System.Text.Json.JsonElement shadowZonesJson)
            {
                foreach (var sz in shadowZonesJson.EnumerateArray())
                {
                    state.ShadowZones.Add(new ShadowZoneData
                    {
                        X = sz.GetProperty("x").GetDouble(),
                        Y = sz.GetProperty("y").GetDouble(),
                        Radius = sz.GetProperty("radius").GetDouble()
                    });
                }
            }

            return state;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("BotGameService started");

            var lastTick = DateTime.UtcNow;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.UtcNow;
                    var delta = (now - lastTick).TotalSeconds;
                    lastTick = now;

                    await Task.Delay(TimeSpan.FromMilliseconds(100), stoppingToken);

                    // Tick 所有机器人的行为树
                    _botManager.TickAll(delta);

                    foreach (var kvp in _roomBots.ToList())
                    {
                        var ctx = kvp.Value;

                        var bot = _botManager.Bots.GetValueOrDefault(ctx.BotInstanceId);
                        if (bot == null || !bot.IsActive) continue;

                        var gameMode = bot.Profile.GameMode;

                        if (gameMode == BotModeLightShadowSolo || gameMode == BotModeLightShadowRace || gameMode == BotModeLightShadowCoop)
                        {
                            await ProcessLightShadowBotActions(ctx, bot, stoppingToken);
                        }
                        else
                        {
                            await ProcessCardBotActions(ctx, bot, stoppingToken);
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

        private async Task ProcessCardBotActions(RoomBotContext ctx, BotInstance bot, CancellationToken stoppingToken)
        {
            if (DateTime.UtcNow - ctx.LastActionTime < ctx.ActionCooldown) return;

            var bb = bot.Tree.Blackboard;
            var actionType = bb.Get<string>(BotBBKeys.ActionType);

            if (string.IsNullOrEmpty(actionType)) return;

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
                _logger.LogWarning("Card bot action broadcast failed: {Message}", ex.Message);
            }
            finally
            {
                bb.Set<string>(BotBBKeys.ActionType, "");
                bb.Set<CardData?>(BotBBKeys.SelectedCard, null);
                bb.Set<string>(BotBBKeys.SelectedTarget, "");
                bb.Set<PotionData?>(BotBBKeys.SelectedPotion, null);
            }
        }

        private async Task ProcessLightShadowBotActions(RoomBotContext ctx, BotInstance bot, CancellationToken stoppingToken)
        {
            if (DateTime.UtcNow - ctx.LastActionTime < ctx.ActionCooldown) return;

            var bb = bot.Tree.Blackboard;

            var gameState = bb.Get<LightShadowGameState>(LightShadowBBKeys.GameState);
            if (gameState == null)
            {
                if (DateTime.UtcNow - ctx.LastActionTime > TimeSpan.FromSeconds(5))
                {
                    _logger.LogDebug("LightShadow bot {BotUserId} has no game state yet, waiting for client to send state", ctx.BotUserId);
                    ctx.LastActionTime = DateTime.UtcNow;
                }
                return;
            }

            var currentAction = bb.Get<string>(LightShadowBBKeys.CurrentAction);
            var moveDir = bb.Get<int>(LightShadowBBKeys.MoveDirection);
            var shouldJump = bb.Get<bool>(LightShadowBBKeys.ShouldJump);
            var shouldSwitchForm = bb.Get<bool>(LightShadowBBKeys.ShouldSwitchForm);
            var shouldDash = bb.Get<bool>(LightShadowBBKeys.ShouldDash);
            var targetForm = bb.Get<string>(LightShadowBBKeys.TargetForm);

            // 如果有任何动作需要执行，发送给客户端
            if (!string.IsNullOrEmpty(currentAction) || moveDir != 0 || shouldJump || shouldSwitchForm || shouldDash)
            {
                try
                {
                    await _hubContext.Clients.Group(ctx.RoomId).SendAsync("LightShadowBotAction",
                        System.Text.Json.JsonSerializer.Serialize(new
                        {
                            playerIndex = ctx.PlayerIndex,
                            botUserId = ctx.BotUserId,
                            moveDirection = moveDir,
                            shouldJump = shouldJump,
                            shouldSwitchForm = shouldSwitchForm,
                            shouldDash = shouldDash,
                            targetForm = targetForm,
                            action = currentAction,
                            isBot = true,
                            timestamp = DateTime.UtcNow
                        }), cancellationToken: stoppingToken);

                    ctx.LastActionTime = DateTime.UtcNow;
                    ctx.ActionCooldown = TimeSpan.FromMilliseconds(100);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("LightShadow bot action broadcast failed: {Message}", ex.Message);
                }
                finally
                {
                    // 清除单次触发的标志
                    bb.Set(LightShadowBBKeys.ShouldJump, false);
                    bb.Set(LightShadowBBKeys.ShouldSwitchForm, false);
                    bb.Set(LightShadowBBKeys.ShouldDash, false);
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
