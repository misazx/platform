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

        public event Action<string, string> OnPlayerJoinedRoom;
        public event Action<string, string> OnPlayerLeftRoom;
        public event Action<string, string, string> OnRoomChatMessage;
        public event Action<string, bool> OnPlayerReadyChanged;
        public event Action<string, string> OnGameStarting;
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
            _hubConnection.On<string>("PlayerJoinedRoom", (json) =>
            {
                try
                {
                    var doc = JsonDocument.Parse(json).RootElement;
                    string playerId = ExtractString(doc, "playerId");
                    string playerName = ExtractString(doc, "playerName");
                    GD.Print($"[GameHubClient] 玩家加入: {playerName}");
                    OnPlayerJoinedRoom?.Invoke(playerId, playerName);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] PlayerJoinedRoom 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<string>("PlayerLeftRoom", (json) =>
            {
                try
                {
                    var doc = JsonDocument.Parse(json).RootElement;
                    string playerId = ExtractString(doc, "playerId");
                    string playerName = ExtractString(doc, "playerName");
                    GD.Print($"[GameHubClient] 玩家离开: {playerName}");
                    OnPlayerLeftRoom?.Invoke(playerId, playerName);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] PlayerLeftRoom 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<string>("RoomChatMessage", (json) =>
            {
                try
                {
                    var doc = JsonDocument.Parse(json).RootElement;
                    string senderId = ExtractString(doc, "senderId");
                    string senderName = ExtractString(doc, "senderName");
                    string message = ExtractString(doc, "message");
                    OnRoomChatMessage?.Invoke(senderId, senderName, message);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] RoomChatMessage 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<string>("PlayerReadyChanged", (json) =>
            {
                try
                {
                    var doc = JsonDocument.Parse(json).RootElement;
                    string playerId = ExtractString(doc, "playerId");
                    bool isReady = ExtractBool(doc, "isReady");
                    OnPlayerReadyChanged?.Invoke(playerId, isReady);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] PlayerReadyChanged 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<string>("RoomStateUpdate", (json) =>
            {
                OnRoomStateUpdate?.Invoke(json);
            });

            _hubConnection.On<string>("GameStarting", (json) =>
            {
                try
                {
                    var doc = JsonDocument.Parse(json).RootElement;
                    string seed = ExtractString(doc, "seed");
                    string roomId = ExtractString(doc, "roomId");
                    GD.Print($"[GameHubClient] 游戏开始! Seed: {seed}");
                    OnGameStarting?.Invoke(seed, roomId);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] GameStarting 解析失败: {ex.Message}");
                    OnGameStarting?.Invoke("", "");
                }
            });

            _hubConnection.On<string>("BotAdded", (json) =>
            {
                try
                {
                    var doc = JsonDocument.Parse(json).RootElement;
                    string botName = ExtractString(doc, "botName");
                    OnBotAdded?.Invoke(botName);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] BotAdded 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<string>("BotRemoved", (json) =>
            {
                try
                {
                    var doc = JsonDocument.Parse(json).RootElement;
                    string botName = ExtractString(doc, "botName");
                    OnBotRemoved?.Invoke(botName);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] BotRemoved 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<string>("CoopCardPlayed", (json) =>
            {
                try
                {
                    var doc = JsonDocument.Parse(json).RootElement;
                    int playerIndex = ExtractInt(doc, "playerIndex");
                    string cardData = ExtractString(doc, "cardData");
                    int targetIndex = ExtractInt(doc, "targetIndex");
                    OnCoopCardPlayed?.Invoke(playerIndex, cardData, targetIndex);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] CoopCardPlayed 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<string>("CoopTurnEnded", (json) =>
            {
                try
                {
                    var doc = JsonDocument.Parse(json).RootElement;
                    int playerIndex = ExtractInt(doc, "playerIndex");
                    OnCoopTurnEnded?.Invoke(playerIndex);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] CoopTurnEnded 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<string>("RacePositionUpdate", (json) =>
            {
                try
                {
                    var doc = JsonDocument.Parse(json).RootElement;
                    string racerId = ExtractString(doc, "racerId");
                    float x = ExtractFloat(doc, "x");
                    float y = ExtractFloat(doc, "y");
                    string form = ExtractString(doc, "form");
                    OnRacePositionUpdate?.Invoke(racerId, x, y, form);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] RacePositionUpdate 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<string>("RaceCheckpointReached", (json) =>
            {
                try
                {
                    var doc = JsonDocument.Parse(json).RootElement;
                    string racerId = ExtractString(doc, "racerId");
                    string checkpointId = ExtractString(doc, "checkpointId");
                    OnRaceCheckpointReached?.Invoke(racerId, checkpointId);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] RaceCheckpointReached 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<string>("RaceFinished", (json) =>
            {
                try
                {
                    var doc = JsonDocument.Parse(json).RootElement;
                    string racerId = ExtractString(doc, "racerId");
                    double finishTime = ExtractDouble(doc, "finishTime");
                    OnRaceFinished?.Invoke(racerId, finishTime);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] RaceFinished 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<string>("CoopPositionUpdate", (json) =>
            {
                try
                {
                    var doc = JsonDocument.Parse(json).RootElement;
                    string userId = ExtractString(doc, "userId");
                    float x = ExtractFloat(doc, "x");
                    float y = ExtractFloat(doc, "y");
                    string form = ExtractString(doc, "form");
                    OnCoopPositionUpdate?.Invoke(userId, x, y, form);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] CoopPositionUpdate 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<string>("CoopSwitchUpdate", (json) =>
            {
                try
                {
                    var doc = JsonDocument.Parse(json).RootElement;
                    string userId = ExtractString(doc, "userId");
                    string switchId = ExtractString(doc, "switchId");
                    bool activated = ExtractBool(doc, "activated");
                    OnCoopSwitchUpdate?.Invoke(userId, switchId, activated);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] CoopSwitchUpdate 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<string>("CoopPuzzleSolved", (json) =>
            {
                try
                {
                    var doc = JsonDocument.Parse(json).RootElement;
                    string puzzleId = ExtractString(doc, "puzzleId");
                    OnCoopPuzzleSolved?.Invoke(puzzleId);
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"[GameHubClient] CoopPuzzleSolved 解析失败: {ex.Message}");
                }
            });

            _hubConnection.On<string>("CoopPlayerDied", (json) =>
            {
                try
                {
                    var doc = JsonDocument.Parse(json).RootElement;
                    string userId = ExtractString(doc, "userId");
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

        public async Task SendCoopTurnEndAsync(string roomId, int playerIndex)
        {
            if (_hubConnection?.State != HubConnectionState.Connected) return;
            await _hubConnection.InvokeAsync("SendCoopTurnEnd", roomId, playerIndex);
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

        public override void _ExitTree()
        {
            _ = DisconnectAsync();
            base._ExitTree();
        }
    }
}
