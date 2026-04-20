using System;
using System.Text.Json;
using System.Threading.Tasks;
using Godot;
using Microsoft.AspNetCore.SignalR.Client;

namespace RoguelikeGame.Network.Realtime
{
    public partial class GameHubClient : Node
    {
        private static GameHubClient _instance;
        public static GameHubClient Instance => _instance;

        private HubConnection _hubConnection;
        private string _serverUrl = "";
        private string _currentRoomId = "";
        private bool _isConnecting;

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;
        public string CurrentRoomId => _currentRoomId;

        public string GetCurrentRoomId() => _currentRoomId;

        public event Action<string, string> OnPlayerJoinedRoom;
        public event Action<string, string> OnPlayerLeftRoom;
        public event Action<string, string, string> OnRoomChatMessage;
        public event Action<string, bool> OnPlayerReadyChanged;
        public event Action<string, string> OnGameStarting;
        public event Action<string, string, string> OnGameStartingExtended;
        public event Action<string> OnRoomStateUpdate;
        public event Action<string> OnBotAdded;
        public event Action<string> OnBotRemoved;
        public event Action<int, string, int> OnCoopCardPlayed;
        public event Action<int> OnCoopTurnEnded;
        public event Action<string, float, float, string> OnRacePositionUpdate;
        public event Action<string, string> OnRaceCheckpointReached;
        public event Action<string, double> OnRaceFinished;
        public event Action<string, float, float, string> OnCoopPositionUpdate;
        public event Action<string, string, bool> OnCoopSwitchUpdate;
        public event Action<string> OnCoopPuzzleSolved;
        public event Action<string> OnCoopPlayerDied;
        public event Action OnCoopPlayerRevived;
        public event Action<string> OnLightShadowBotAction;
        public event Action<string, string, string> OnLevelCompleted;
        public event Action<string> OnGameEnded;

        public override void _Ready()
        {
            if (_instance != null && _instance != this) { QueueFree(); return; }
            _instance = this;
            ProcessMode = ProcessModeEnum.Always;

            if (string.IsNullOrEmpty(_serverUrl))
            {
                _serverUrl = GetServerUrl();
            }
        }

        private string GetServerUrl()
        {
            var configNode = GetNodeOrNull("/root/ServerConfig");
            if (configNode != null && configNode.HasMethod("get_server_url"))
            {
                return configNode.Call("get_server_url").AsString();
            }
            return "http://127.0.0.1:5002";
        }

        public void SetServerUrl(string url)
        {
            _serverUrl = url;
        }

        public async Task ConnectAsync(string authToken)
        {
            if (_hubConnection != null && _hubConnection.State != HubConnectionState.Disconnected)
            {
                return;
            }

            if (_isConnecting) return;
            _isConnecting = true;

            try
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl($"{_serverUrl}/hubs/game", options =>
                    {
                        options.AccessTokenProvider = () => Task.FromResult(authToken);
                    })
                    .WithAutomaticReconnect(new[] { TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(15) })
                    .Build();

                RegisterHandlers();

                _hubConnection.Reconnecting += error =>
                {
                    GD.Print("[GameHubClient] 重连中...");
                    return Task.CompletedTask;
                };

                _hubConnection.Reconnected += connectionId =>
                {
                    GD.Print($"[GameHubClient] 重连成功: {connectionId}");
                    if (!string.IsNullOrEmpty(_currentRoomId))
                    {
                        _ = _hubConnection.InvokeAsync("JoinRoom", _currentRoomId);
                    }
                    return Task.CompletedTask;
                };

                _hubConnection.Closed += error =>
                {
                    GD.Print($"[GameHubClient] 连接关闭: {error?.Message}");
                    return Task.CompletedTask;
                };

                await _hubConnection.StartAsync();
                GD.Print("[GameHubClient] SignalR 连接成功");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[GameHubClient] 连接失败: {ex.Message}");
            }
            finally
            {
                _isConnecting = false;
            }
        }

        public async Task DisconnectAsync()
        {
            if (_hubConnection != null)
            {
                try
                {
                    if (!string.IsNullOrEmpty(_currentRoomId))
                    {
                        await _hubConnection.InvokeAsync("LeaveRoom", _currentRoomId);
                    }
                    await _hubConnection.StopAsync();
                    await _hubConnection.DisposeAsync();
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] 断开连接异常: {ex.Message}");
                }
                _hubConnection = null;
                _currentRoomId = "";
            }
        }

        private string ExtractString(JsonElement doc, string key, string defaultValue = "")
        {
            return doc.TryGetProperty(key, out var el) ? el.GetString() ?? defaultValue : defaultValue;
        }

        private bool ExtractBool(JsonElement doc, string key, bool defaultValue = false)
        {
            return doc.TryGetProperty(key, out var el) ? el.GetBoolean() : defaultValue;
        }

        private int ExtractInt(JsonElement doc, string key, int defaultValue = 0)
        {
            return doc.TryGetProperty(key, out var el) ? el.GetInt32() : defaultValue;
        }

        private float ExtractFloat(JsonElement doc, string key, float defaultValue = 0f)
        {
            return doc.TryGetProperty(key, out var el) ? el.GetSingle() : defaultValue;
        }

        private double ExtractDouble(JsonElement doc, string key, double defaultValue = 0.0)
        {
            return doc.TryGetProperty(key, out var el) ? el.GetDouble() : defaultValue;
        }

        private void RegisterHandlers()
        {
            _hubConnection.On<JsonElement>("PlayerJoinedRoom", (data) =>
            {
                try
                {
                    string playerId = ExtractString(data, "playerId");
                    string playerName = ExtractString(data, "playerName");
                    GD.Print($"[GameHubClient] 玩家加入: {playerName}");
                    OnPlayerJoinedRoom?.Invoke(playerId, playerName);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] PlayerJoinedRoom 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<JsonElement>("PlayerLeftRoom", (data) =>
            {
                try
                {
                    string playerId = ExtractString(data, "playerId");
                    string playerName = ExtractString(data, "playerName");
                    GD.Print($"[GameHubClient] 玩家离开: {playerName}");
                    OnPlayerLeftRoom?.Invoke(playerId, playerName);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] PlayerLeftRoom 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<JsonElement>("RoomChatMessage", (data) =>
            {
                try
                {
                    string senderId = ExtractString(data, "senderId");
                    string senderName = ExtractString(data, "senderName");
                    string message = ExtractString(data, "message");
                    OnRoomChatMessage?.Invoke(senderId, senderName, message);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] RoomChatMessage 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<JsonElement>("PlayerReadyChanged", (data) =>
            {
                try
                {
                    string playerId = ExtractString(data, "playerId");
                    bool isReady = ExtractBool(data, "isReady");
                    OnPlayerReadyChanged?.Invoke(playerId, isReady);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] PlayerReadyChanged 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<JsonElement>("RoomStateUpdate", (data) =>
            {
                OnRoomStateUpdate?.Invoke(data.GetRawText());
            });

            _hubConnection.On<JsonElement>("GameStarting", (data) =>
            {
                try
                {
                    string seed = ExtractString(data, "seed");
                    string roomId = ExtractString(data, "roomId");
                    string mode = ExtractString(data, "mode");
                    string gameModeId = ExtractString(data, "gameModeId");
                    GD.Print($"[GameHubClient] 游戏开始! Seed: {seed}, Mode: {mode}, GameModeId: {gameModeId}");
                    OnGameStarting?.Invoke(seed, roomId);
                    OnGameStartingExtended?.Invoke(seed, mode, gameModeId);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] GameStarting 解析失败: {ex.Message}");
                    OnGameStarting?.Invoke("", "");
                    OnGameStartingExtended?.Invoke("", "", "");
                }
            });

            _hubConnection.On<JsonElement>("BotAdded", (data) =>
            {
                try
                {
                    string botName = ExtractString(data, "botName");
                    OnBotAdded?.Invoke(botName);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] BotAdded 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<JsonElement>("BotRemoved", (data) =>
            {
                try
                {
                    string botName = ExtractString(data, "botName");
                    OnBotRemoved?.Invoke(botName);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] BotRemoved 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<JsonElement>("CoopCardPlayed", (data) =>
            {
                try
                {
                    int playerIndex = ExtractInt(data, "playerIndex");
                    string cardData = data.TryGetProperty("cardData", out var cardEl) ? cardEl.GetRawText() : "{}";
                    int targetIndex = ExtractInt(data, "targetIndex");
                    OnCoopCardPlayed?.Invoke(playerIndex, cardData, targetIndex);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] CoopCardPlayed 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<JsonElement>("CoopTurnEnded", (data) =>
            {
                try
                {
                    int playerIndex = ExtractInt(data, "playerIndex");
                    OnCoopTurnEnded?.Invoke(playerIndex);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] CoopTurnEnded 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<JsonElement>("RacePositionUpdate", (data) =>
            {
                try
                {
                    string racerId = ExtractString(data, "racerId");
                    float x = ExtractFloat(data, "x");
                    float y = ExtractFloat(data, "y");
                    string form = ExtractString(data, "form");
                    OnRacePositionUpdate?.Invoke(racerId, x, y, form);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] RacePositionUpdate 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<JsonElement>("RaceCheckpointReached", (data) =>
            {
                try
                {
                    string racerId = ExtractString(data, "racerId");
                    string checkpointId = ExtractString(data, "checkpointId");
                    OnRaceCheckpointReached?.Invoke(racerId, checkpointId);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] RaceCheckpointReached 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<JsonElement>("RaceFinished", (data) =>
            {
                try
                {
                    string racerId = ExtractString(data, "racerId");
                    double finishTime = ExtractDouble(data, "finishTime");
                    OnRaceFinished?.Invoke(racerId, finishTime);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] RaceFinished 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<JsonElement>("CoopPositionUpdate", (data) =>
            {
                try
                {
                    string userId = ExtractString(data, "userId");
                    float x = ExtractFloat(data, "x");
                    float y = ExtractFloat(data, "y");
                    string form = ExtractString(data, "form");
                    OnCoopPositionUpdate?.Invoke(userId, x, y, form);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] CoopPositionUpdate 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<JsonElement>("CoopSwitchUpdate", (data) =>
            {
                try
                {
                    string userId = ExtractString(data, "userId");
                    string switchId = ExtractString(data, "switchId");
                    bool activated = ExtractBool(data, "activated");
                    OnCoopSwitchUpdate?.Invoke(userId, switchId, activated);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] CoopSwitchUpdate 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<JsonElement>("CoopPuzzleSolved", (data) =>
            {
                try
                {
                    string puzzleId = ExtractString(data, "puzzleId");
                    OnCoopPuzzleSolved?.Invoke(puzzleId);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] CoopPuzzleSolved 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<JsonElement>("CoopPlayerDied", (data) =>
            {
                try
                {
                    string userId = ExtractString(data, "userId");
                    OnCoopPlayerDied?.Invoke(userId);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] CoopPlayerDied 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On("CoopPlayerRevived", () =>
            {
                OnCoopPlayerRevived?.Invoke();
            });

            _hubConnection.On<JsonElement>("LightShadowBotAction", (data) =>
            {
                try
                {
                    OnLightShadowBotAction?.Invoke(data.GetRawText());
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] LightShadowBotAction 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<JsonElement>("LevelCompleted", (data) =>
            {
                try
                {
                    string levelId = ExtractString(data, "levelId");
                    string userId = ExtractString(data, "userId");
                    string resultJson = data.TryGetProperty("resultData", out var resultEl) ? resultEl.GetRawText() : "{}";
                    OnLevelCompleted?.Invoke(levelId, userId, resultJson);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] LevelCompleted 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<JsonElement>("GameEnded", (data) =>
            {
                try
                {
                    string resultJson = data.TryGetProperty("gameResult", out var resultEl) ? resultEl.GetRawText() : "{}";
                    OnGameEnded?.Invoke(resultJson);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] GameEnded 解析失败: {ex.Message}");
                }
            });
        }

        public async Task JoinRoomAsync(string roomId)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            _currentRoomId = roomId;
            await _hubConnection.InvokeAsync("JoinRoom", roomId);
        }

        public async Task LeaveRoomAsync(string roomId)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            _currentRoomId = "";
            await _hubConnection.InvokeAsync("LeaveRoom", roomId);
        }

        public async Task SendRoomChatAsync(string roomId, string message)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("SendRoomChat", roomId, message);
        }

        public async Task NotifyReadyChangedAsync(string roomId, bool isReady)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("NotifyReadyChanged", roomId, isReady);
        }

        public void NotifyReadyChangedSync(string roomId, bool isReady)
        {
            _ = NotifyReadyChangedAsync(roomId, isReady);
        }

        public async Task NotifyGameStartingAsync(string roomId)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("NotifyGameStarting", roomId);
        }

        public async Task SendCoopCardPlayAsync(string roomId, int playerIndex, object cardData, int targetIndex)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("SendCoopCardPlay", roomId, playerIndex, cardData, targetIndex);
        }

        public void SendCoopCardPlaySync(string roomId, int playerIndex, string cardJson, int targetIndex)
        {
            _ = SendCoopCardPlayAsync(roomId, playerIndex, cardJson, targetIndex);
        }

        public async Task SendCoopTurnEndAsync(string roomId, int playerIndex)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("SendCoopTurnEnd", roomId, playerIndex);
        }

        public void SendCoopTurnEndSync(string roomId, int playerIndex)
        {
            _ = SendCoopTurnEndAsync(roomId, playerIndex);
        }

        public async Task SendRacePositionAsync(string roomId, string racerId, double x, double y, string form)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("SendRacePosition", roomId, racerId, x, y, form);
        }

        public async Task SendRaceCheckpointAsync(string roomId, string racerId, string checkpointId)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("SendRaceCheckpoint", roomId, racerId, checkpointId);
        }

        public async Task SendRaceFinishAsync(string roomId, string racerId, double finishTime)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("SendRaceFinish", roomId, racerId, finishTime);
        }

        public async Task SendCoopPositionAsync(string roomId, double x, double y, string form)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("SendCoopPosition", roomId, x, y, form);
        }

        public async Task SendCoopSwitchAsync(string roomId, string switchId, bool activated)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("SendCoopSwitch", roomId, switchId, activated);
        }

        public async Task SendCoopPuzzleSolvedAsync(string roomId, string puzzleId)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("SendCoopPuzzleSolved", roomId, puzzleId);
        }

        public async Task SendCoopPlayerDiedAsync(string roomId)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("SendCoopPlayerDied", roomId);
        }

        public async Task SendCoopPlayerRevivedAsync(string roomId)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("SendCoopPlayerRevived", roomId);
        }

        public async Task UpdateBotGameStateAsync(string roomId, string botUserId, string gameStateJson)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("UpdateBotGameState", roomId, botUserId, gameStateJson);
        }

        public async Task SendLevelCompletedAsync(string roomId, string levelId, string userId, object resultData)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("SendLevelCompleted", roomId, levelId, userId, resultData);
        }

        public async Task SendGameEndedAsync(string roomId, object gameResult)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("SendGameEnded", roomId, gameResult);
        }

        public override void _ExitTree()
        {
            _ = DisconnectAsync();
            base._ExitTree();
        }
    }
}
