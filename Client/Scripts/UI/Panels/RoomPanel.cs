using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using RoguelikeGame.Network;
using RoguelikeGame.Network.Auth;
using RoguelikeGame.Network.Rooms;
using RoguelikeGame.Packages;
using RoguelikeGame.Client.UI;

namespace RoguelikeGame.UI.Panels
{
    public partial class RoomPanel : Control
    {
        private Label _roomTitleLabel;
        private Label _roomStatusIcon;
        private Label _roomModeLabel;
        private OptionButton _modeOption;
        private readonly List<GameMode> _availableModes = new();
        private Label _roomPlayerCountLabel;
        private VBoxContainer _playerListContainer;
        private RichTextLabel _chatOutput;
        private LineEdit _chatInput;
        private Button _readyButton;
        private Button _startButton;
        private Button _addBotButton;
        private Button _leaveButton;
        private Timer _refreshTimer;
        private bool _isReady;
        private bool _refreshPending;
        private double _refreshDebounceTime;

        public event Action OnLeaveRoom;
        public event Action OnGameStarted;

        public override void _Ready()
        {
            SetAnchorsPreset(Control.LayoutPreset.FullRect);
            OffsetLeft = 0;
            OffsetTop = 0;
            OffsetRight = 0;
            OffsetBottom = 0;

            CreateUI();
            SetupEventHandlers();
            RefreshRoomInfo();

            _refreshTimer = new Timer
            {
                WaitTime = 3.0,
                Autostart = true
            };
            _refreshTimer.Timeout += RefreshRoomInfo;
            AddChild(_refreshTimer);
        }

        private void CreateUI()
        {
            var bg = new ColorRect
            {
                Color = new Color(0.02f, 0.02f, 0.05f, 0.97f),
                MouseFilter = MouseFilterEnum.Ignore,
                AnchorsPreset = (int)Control.LayoutPreset.FullRect
            };
            AddChild(bg);

            var mainContainer = new HSplitContainer();
            mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            mainContainer.AddThemeConstantOverride("separation", 8);
            AddChild(mainContainer);

            CreateLeftPanel(mainContainer);
            CreateRightPanel(mainContainer);
        }

        private void CreateLeftPanel(HSplitContainer parent)
        {
            var leftPanel = new VBoxContainer();
            leftPanel.CustomMinimumSize = new Vector2(360, 0);
            leftPanel.AddThemeConstantOverride("separation", 10);
            parent.AddChild(leftPanel);

            var headerBox = new VBoxContainer();
            headerBox.AddThemeConstantOverride("separation", 5);
            leftPanel.AddChild(headerBox);

            _roomTitleLabel = new Label
            {
                Text = "房间名称",
                HorizontalAlignment = HorizontalAlignment.Center,
                CustomMinimumSize = new Vector2(340, 40)
            };
            _roomTitleLabel.AddThemeFontSizeOverride("font_size", 24);
            _roomTitleLabel.Modulate = new Color(1f, 0.9f, 0.6f);
            headerBox.AddChild(_roomTitleLabel);

            var infoRow = new HBoxContainer();
            infoRow.AddThemeConstantOverride("separation", 12);
            infoRow.CustomMinimumSize = new Vector2(340, 25);
            headerBox.AddChild(infoRow);

            _roomStatusIcon = new Label
            {
                Text = "[等待]",
                CustomMinimumSize = new Vector2(100, 25)
            };
            _roomStatusIcon.AddThemeFontSizeOverride("font_size", 13);
            infoRow.AddChild(_roomStatusIcon);

            _roomModeLabel = new Label
            {
                Text = "PvP",
                CustomMinimumSize = new Vector2(100, 25)
            };
            _roomModeLabel.AddThemeFontSizeOverride("font_size", 13);
            infoRow.AddChild(_roomModeLabel);

            _modeOption = new OptionButton
            {
                CustomMinimumSize = new Vector2(120, 28),
                Visible = false
            };
            _modeOption.Connect("item_selected", new Callable(this, nameof(OnModeOptionSelected)));
            infoRow.AddChild(_modeOption);

            _roomPlayerCountLabel = new Label
            {
                Text = "1/4",
                CustomMinimumSize = new Vector2(80, 25)
            };
            _roomPlayerCountLabel.AddThemeFontSizeOverride("font_size", 13);
            infoRow.AddChild(_roomPlayerCountLabel);

            leftPanel.AddChild(new HSeparator());

            var playerHeader = new Label
            {
                Text = "玩家列表",
                CustomMinimumSize = new Vector2(340, 28)
            };
            playerHeader.AddThemeFontSizeOverride("font_size", 16);
            leftPanel.AddChild(playerHeader);

            _playerListContainer = new VBoxContainer();
            _playerListContainer.AddThemeConstantOverride("separation", 6);
            _playerListContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            leftPanel.AddChild(_playerListContainer);

            leftPanel.AddChild(new HSeparator());

            var buttonBox = new VBoxContainer();
            buttonBox.AddThemeConstantOverride("separation", 8);
            leftPanel.AddChild(buttonBox);

            _readyButton = UITheme.MakeWideButton("准备", "icon_hand", new Vector2(320, 45));
            _readyButton.Pressed += OnReadyPressed;
            buttonBox.AddChild(_readyButton);

            _startButton = UITheme.MakeWideButton("开始游戏", "icon_play", new Vector2(320, 45));
            _startButton.Pressed += OnStartGamePressed;
            _startButton.Visible = false;
            buttonBox.AddChild(_startButton);

            _addBotButton = UITheme.MakeSmallButton("添加机器人", "icon_bot", new Vector2(320, 38));
            _addBotButton.Pressed += OnAddBotPressed;
            _addBotButton.Visible = false;
            buttonBox.AddChild(_addBotButton);

            _leaveButton = UITheme.MakeSmallButton("退出房间", "icon_cross", new Vector2(320, 40));
            _leaveButton.Modulate = new Color(0.9f, 0.4f, 0.4f);
            _leaveButton.Pressed += OnLeavePressed;
            buttonBox.AddChild(_leaveButton);
        }

        private void CreateRightPanel(HSplitContainer parent)
        {
            var rightPanel = new VBoxContainer();
            rightPanel.AddThemeConstantOverride("separation", 8);
            rightPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            parent.AddChild(rightPanel);

            var chatHeader = new Label
            {
                Text = "房间聊天",
                CustomMinimumSize = new Vector2(0, 28)
            };
            chatHeader.AddThemeFontSizeOverride("font_size", 16);
            rightPanel.AddChild(chatHeader);

            var chatScroll = new ScrollContainer
            {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
                VerticalScrollMode = ScrollContainer.ScrollMode.Auto
            };
            rightPanel.AddChild(chatScroll);

            _chatOutput = new RichTextLabel
            {
                CustomMinimumSize = new Vector2(0, 0),
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                BbcodeEnabled = true,
                FitContent = true,
                ScrollFollowing = true
            };
            chatScroll.AddChild(_chatOutput);

            var chatRow = new HBoxContainer();
            chatRow.AddThemeConstantOverride("separation", 8);
            chatRow.CustomMinimumSize = new Vector2(0, 36);
            rightPanel.AddChild(chatRow);

            _chatInput = new LineEdit
            {
                PlaceholderText = "输入消息 (Enter发送)...",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(0, 36)
            };
            _chatInput.Connect("text_submitted", new Callable(this, nameof(OnChatSubmitted)));
            chatRow.AddChild(_chatInput);

            var sendButton = UITheme.MakeSmallButton("发送", "", new Vector2(80, 36));
            sendButton.Pressed += SendChatMessage;
            chatRow.AddChild(sendButton);
        }

        private void SetupEventHandlers()
        {
            if (RoomManager.Instance != null)
            {
                RoomManager.Instance.RoomUpdated += OnRoomUpdated;
                RoomManager.Instance.PlayerJoinedRoom += OnPlayerJoined;
                RoomManager.Instance.PlayerLeftRoom += OnPlayerLeft;
                RoomManager.Instance.PlayerReadyChanged += OnPlayerReadyChanged;
                RoomManager.Instance.GameStarting += OnGameStarting;
            }

            var hubClient = Network.Realtime.GameHubClient.Instance;
            if (hubClient != null)
            {
                hubClient.OnPlayerJoinedRoom += OnHubPlayerJoined;
                hubClient.OnPlayerLeftRoom += OnHubPlayerLeft;
                hubClient.OnRoomChatMessage += OnHubChatMessage;
                hubClient.OnPlayerReadyChanged += OnHubReadyChanged;
                hubClient.OnGameStarting += OnHubGameStarting;
                hubClient.OnBotAdded += OnHubBotAdded;
                hubClient.OnBotRemoved += OnHubBotRemoved;
                hubClient.OnRoomModeChanged += OnHubModeChanged;
                hubClient.OnPlayerSlotSwapped += OnHubSlotSwapped;
            }
        }

        public override void _ExitTree()
        {
            if (RoomManager.Instance != null)
            {
                RoomManager.Instance.RoomUpdated -= OnRoomUpdated;
                RoomManager.Instance.PlayerJoinedRoom -= OnPlayerJoined;
                RoomManager.Instance.PlayerLeftRoom -= OnPlayerLeft;
                RoomManager.Instance.PlayerReadyChanged -= OnPlayerReadyChanged;
                RoomManager.Instance.GameStarting -= OnGameStarting;
            }

            var hubClient = Network.Realtime.GameHubClient.Instance;
            if (hubClient != null)
            {
                hubClient.OnPlayerJoinedRoom -= OnHubPlayerJoined;
                hubClient.OnPlayerLeftRoom -= OnHubPlayerLeft;
                hubClient.OnRoomChatMessage -= OnHubChatMessage;
                hubClient.OnPlayerReadyChanged -= OnHubReadyChanged;
                hubClient.OnGameStarting -= OnHubGameStarting;
                hubClient.OnBotAdded -= OnHubBotAdded;
                hubClient.OnBotRemoved -= OnHubBotRemoved;
                hubClient.OnRoomModeChanged -= OnHubModeChanged;
                hubClient.OnPlayerSlotSwapped -= OnHubSlotSwapped;
            }

            base._ExitTree();
        }

        private async void RefreshRoomInfo()
        {
            _refreshPending = true;
            _refreshDebounceTime = 0.3;
        }

        private async void DoRefreshRoomInfo()
        {
            _refreshPending = false;
            var room = RoomManager.Instance?.CurrentRoom;
            if (room == null) return;

            try
            {
                var result = await RoomManager.Instance.GetRoomDetailsAsync(room.Id);
                if (result.Success && result.Room != null)
                {
                    UpdateRoomDisplay(result.Room);
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[RoomPanel] 刷新房间信息失败: {ex.Message}");
            }
        }

        public override void _Process(double delta)
        {
            if (_refreshPending)
            {
                _refreshDebounceTime -= delta;
                if (_refreshDebounceTime <= 0)
                {
                    DoRefreshRoomInfo();
                }
            }
        }

        private void UpdateRoomDisplay(RoomInfo room)
        {
            _roomTitleLabel.Text = room.Name;

            _roomStatusIcon.Text = room.Status switch
            {
                RoomStatus.Waiting => "[等待]",
                RoomStatus.Full => "[已满]",
                RoomStatus.Ready => "[就绪]",
                RoomStatus.Playing => "[游戏中]",
                _ => "[未知]"
            };

            string modeText = room.Mode switch
            {
                GameMode.PvP => "PvP 对战",
                GameMode.PvE => "PvE 合作",
                GameMode.Coop => "Coop 团队",
                GameMode.Race => "Race 竞速",
                _ => "未知"
            };

            bool isHost = RoomManager.Instance?.IsHost ?? false;

            if (isHost)
            {
                _roomModeLabel.Visible = false;
                _modeOption.Visible = true;
                PopulateModeOptions(room);
                int currentIdx = _availableModes.IndexOf(room.Mode);
                if (currentIdx >= 0)
                    _modeOption.Selected = currentIdx;
            }
            else
            {
                _roomModeLabel.Visible = true;
                _modeOption.Visible = false;
                _roomModeLabel.Text = modeText;
            }

            _roomPlayerCountLabel.Text = $"{room.CurrentPlayers}/{room.MaxPlayers}";

            var sortedPlayers = room.Players.OrderBy(p => p.JoinedAt).ToList();
            UpdatePlayerList(room, sortedPlayers);
            UpdateButtonStates(room);
        }

        private void PopulateModeOptions(RoomInfo room)
        {
            _modeOption.Clear();
            _availableModes.Clear();

            var modeMap = new Dictionary<string, (GameMode Mode, string Label)>
            {
                { "pvp", (GameMode.PvP, "PvP 对战") },
                { "pve", (GameMode.PvE, "PvE 合作") },
                { "coop", (GameMode.Coop, "Coop 团队") },
                { "race", (GameMode.Race, "Race 竞速") }
            };

            string gameModeId = room.GameModeId ?? "";
            if (!string.IsNullOrEmpty(gameModeId))
            {
                var pkg = PackageManager.Instance?.GetPackage(gameModeId);
                if (pkg?.MultiplayerModes != null && pkg.MultiplayerModes.Count > 0)
                {
                    foreach (var modeStr in pkg.MultiplayerModes)
                    {
                        if (modeMap.TryGetValue(modeStr.ToLower(), out var entry))
                        {
                            _modeOption.AddItem(entry.Label);
                            _availableModes.Add(entry.Mode);
                        }
                    }
                }
            }

            if (_availableModes.Count == 0)
            {
                _modeOption.AddItem("Coop 团队");
                _availableModes.Add(GameMode.Coop);
            }
        }

        private async void OnModeOptionSelected(long index)
        {
            if (index < 0 || index >= _availableModes.Count) return;

            var newMode = _availableModes[(int)index];
            bool success = await RoomManager.Instance.ChangeModeAsync(newMode);

            if (success)
            {
                AddSystemMessage($"模式已更改为 {_modeOption.GetItemText((int)index)}");
            }
            else
            {
                AddSystemMessage("模式更改失败");
                var room = RoomManager.Instance?.CurrentRoom;
                if (room != null)
                {
                    int currentIdx = _availableModes.IndexOf(room.Mode);
                    if (currentIdx >= 0)
                        _modeOption.Selected = currentIdx;
                }
            }
        }

        private void UpdatePlayerList(RoomInfo room, List<PlayerInfo> sortedPlayers)
        {
            foreach (var child in _playerListContainer.GetChildren())
            {
                child.QueueFree();
            }

            bool isHost = RoomManager.Instance?.IsHost ?? false;
            string currentUserId = AuthSystem.Instance?.CurrentUser?.Id ?? "";

            for (int slotIndex = 0; slotIndex < room.MaxPlayers; slotIndex++)
            {
                bool hasPlayer = slotIndex < sortedPlayers.Count;
                var player = hasPlayer ? sortedPlayers[slotIndex] : null;

                var playerRow = new HBoxContainer();
                playerRow.AddThemeConstantOverride("separation", 6);
                playerRow.CustomMinimumSize = new Vector2(320, 36);

                var slotLabel = new Label
                {
                    Text = $"#{slotIndex + 1}",
                    CustomMinimumSize = new Vector2(28, 30)
                };
                slotLabel.AddThemeFontSizeOverride("font_size", 12);
                slotLabel.Modulate = new Color(0.5f, 0.5f, 0.55f);
                playerRow.AddChild(slotLabel);

                if (hasPlayer && player != null)
                {
                    var roleIcon = new Label
                    {
                        Text = player.IsHost ? "[房主]" : (player.IsBot ? "[Bot]" : "  "),
                        CustomMinimumSize = new Vector2(40, 30)
                    };
                    roleIcon.AddThemeFontSizeOverride("font_size", 11);
                    playerRow.AddChild(roleIcon);

                    var nameLabel = new Label
                    {
                        Text = player.Username,
                        CustomMinimumSize = new Vector2(100, 30)
                    };
                    nameLabel.AddThemeFontSizeOverride("font_size", 14);
                    if (player.IsHost)
                        nameLabel.Modulate = new Color(1f, 0.85f, 0.3f);
                    else if (player.IsBot)
                        nameLabel.Modulate = new Color(0.6f, 0.8f, 1f);
                    else
                        nameLabel.Modulate = new Color(0.85f, 0.9f, 0.95f);
                    playerRow.AddChild(nameLabel);

                    var spacer = new Control
                    {
                        SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
                    };
                    playerRow.AddChild(spacer);

                    var statusLabel = new Label
                    {
                        Text = player.IsHost ? "房主" : (player.IsReady ? "已准备" : "未准备"),
                        CustomMinimumSize = new Vector2(70, 30),
                        HorizontalAlignment = HorizontalAlignment.Right
                    };
                    statusLabel.AddThemeFontSizeOverride("font_size", 11);
                    if (player.IsHost)
                        statusLabel.Modulate = new Color(1f, 0.85f, 0.3f);
                    else
                        statusLabel.Modulate = player.IsReady ? new Color(0.4f, 0.9f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);
                    playerRow.AddChild(statusLabel);

                    bool isLocalPlayer = player.Id == currentUserId;
                    bool canSwap = (isLocalPlayer || isHost) && !player.IsHost;

                    if (canSwap && room.Players.Count > 1)
                    {
                        var swapBtn = new Button
                        {
                            Text = "⇅",
                            CustomMinimumSize = new Vector2(28, 28)
                        };
                        swapBtn.AddThemeFontSizeOverride("font_size", 14);
                        swapBtn.Modulate = new Color(0.5f, 0.7f, 0.9f);
                        int fromSlot = slotIndex;
                        swapBtn.Pressed += async () => await SwapPlayerSlot(fromSlot);
                        playerRow.AddChild(swapBtn);
                    }

                    if (isHost && player.IsBot)
                    {
                        var removeBtn = new Button
                        {
                            Text = "✕",
                            CustomMinimumSize = new Vector2(28, 28)
                        };
                        removeBtn.AddThemeFontSizeOverride("font_size", 12);
                        removeBtn.Modulate = new Color(0.9f, 0.4f, 0.4f);
                        var botId = player.Id;
                        removeBtn.Pressed += async () => await RemoveBot(botId);
                        playerRow.AddChild(removeBtn);
                    }
                }
                else
                {
                    var emptyLabel = new Label
                    {
                        Text = "  空位",
                        CustomMinimumSize = new Vector2(100, 30)
                    };
                    emptyLabel.AddThemeFontSizeOverride("font_size", 14);
                    emptyLabel.Modulate = new Color(0.35f, 0.35f, 0.4f);
                    playerRow.AddChild(emptyLabel);
                }

                var styleBox = UITheme.MakeCardPanelStyle();

                var panel = new PanelContainer();
                panel.AddThemeStyleboxOverride("panel", styleBox);
                panel.AddChild(playerRow);

                _playerListContainer.AddChild(panel);
            }
        }

        private async Task SwapPlayerSlot(int fromSlot)
        {
            var room = RoomManager.Instance?.CurrentRoom;
            if (room == null) return;

            string currentUserId = AuthSystem.Instance?.CurrentUser?.Id ?? "";
            bool isHost = RoomManager.Instance?.IsHost ?? false;

            var sortedPlayers = room.Players.OrderBy(p => p.JoinedAt).ToList();

            for (int toSlot = 0; toSlot < sortedPlayers.Count; toSlot++)
            {
                if (toSlot == fromSlot) continue;

                var targetPlayer = sortedPlayers[toSlot];
                if (isHost || targetPlayer.Id == currentUserId)
                {
                    bool success = await RoomManager.Instance.SwapSlotAsync(fromSlot, toSlot);
                    if (success)
                    {
                        AddSystemMessage($"位置已交换: #{fromSlot + 1} ↔ #{toSlot + 1}");
                    }
                    else
                    {
                        AddSystemMessage("位置交换失败");
                    }
                    return;
                }
            }
        }

        private async System.Threading.Tasks.Task RemoveBot(string botId)
        {
            var result = await Network.Rooms.RoomManager.Instance.RemoveBotAsync(botId);
            if (result.Success)
            {
                AddSystemMessage($"{result.Message}");
            }
            else
            {
                AddSystemMessage($"移除机器人失败: {result.Message}");
            }
        }

        private void UpdateButtonStates(RoomInfo room)
        {
            bool isHost = RoomManager.Instance?.IsHost ?? false;
            string currentUserId = AuthSystem.Instance?.CurrentUser?.Id ?? "";

            var currentPlayer = room.Players.Find(p => p.Id == currentUserId);
            _isReady = currentPlayer?.IsReady ?? false;

            _readyButton.Visible = !isHost;
            _readyButton.Text = _isReady ? "取消准备" : "准备";
            _readyButton.Modulate = _isReady ? new Color(0.9f, 0.6f, 0.3f) : new Color(0.3f, 0.8f, 0.5f);

            _startButton.Visible = isHost;
            _addBotButton.Visible = isHost;

            if (isHost)
            {
                bool canStart = room.Players.Count >= 2 && room.Players.TrueForAll(p => p.IsReady || p.IsHost);
                _startButton.Disabled = !canStart;
                _startButton.Modulate = canStart ? new Color(0.3f, 0.9f, 0.5f) : new Color(0.4f, 0.4f, 0.4f);
            }
        }

        private async void OnReadyPressed()
        {
            _readyButton.Disabled = true;

            try
            {
                bool success = await RoomManager.Instance.SetReadyAsync(!_isReady);

                if (success)
                {
                    AddSystemMessage(_isReady ? "取消准备" : "已准备就绪");
                }
                else
                {
                    AddSystemMessage("准备状态更新失败");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[RoomPanel] 准备操作异常: {ex.Message}");
            }
            finally
            {
                _readyButton.Disabled = false;
            }
        }

        private async void OnStartGamePressed()
        {
            _startButton.Disabled = true;

            try
            {
                bool success = await RoomManager.Instance.StartGameAsync();

                if (success)
                {
                    AddSystemMessage("游戏开始！");
                }
                else
                {
                    AddSystemMessage("无法开始游戏，请确认所有玩家已准备");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[RoomPanel] 开始游戏异常: {ex.Message}");
                AddSystemMessage("开始游戏异常");
            }
            finally
            {
                _startButton.Disabled = false;
            }
        }

        private async void OnAddBotPressed()
        {
            _addBotButton.Disabled = true;

            try
            {
                var result = await Network.Rooms.RoomManager.Instance.AddBotAsync("Normal");

                if (result.Success)
                {
                    AddSystemMessage($"{result.Message}");
                }
                else
                {
                    AddSystemMessage($"添加机器人失败: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[RoomPanel] 添加机器人异常: {ex.Message}");
                AddSystemMessage($"添加机器人异常: {ex.Message}");
            }
            finally
            {
                _addBotButton.Disabled = false;
            }
        }

        private async void OnLeavePressed()
        {
            _leaveButton.Disabled = true;

            try
            {
                var result = await RoomManager.Instance.LeaveRoomAsync();

                if (result.Success)
                {
                    AddSystemMessage("已离开房间");
                }
                else
                {
                    AddSystemMessage($"离开房间失败: {result.Message}");
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[RoomPanel] 离开房间异常: {ex.Message}");
            }
            finally
            {
                OnLeaveRoom?.Invoke();
            }
        }

        private void OnChatSubmitted(string text)
        {
            SendChatMessage();
        }

        private void SendChatMessage()
        {
            string message = _chatInput.Text.Trim();
            if (string.IsNullOrEmpty(message)) return;

            string username = AuthSystem.Instance?.CurrentUser?.Username ?? "Unknown";
            AddChatMessage(username, message, true);
            _chatInput.Text = "";

            var room = RoomManager.Instance?.CurrentRoom;
            if (room != null)
            {
                var hubClient = Network.Realtime.GameHubClient.Instance;
                if (hubClient != null && hubClient.IsConnected)
                {
                    _ = hubClient.SendRoomChatAsync(room.Id, message);
                }
            }
        }

        public void AddChatMessage(string sender, string message, bool isLocal = false)
        {
            Color color = isLocal ? new Color(0.4f, 0.8f, 1f) : new Color(1f, 0.9f, 0.6f);
            string timeStr = DateTime.Now.ToString("HH:mm");
            string formattedMsg = $"[color=gray][{timeStr}][/color] [color=#{color.ToHtml(false)}]{sender}[/color]: {message}\n";

            _chatOutput.AppendText(formattedMsg);
            _chatOutput.ScrollToLine(_chatOutput.GetLineCount() - 1);
        }

        public void AddSystemMessage(string message)
        {
            string timeStr = DateTime.Now.ToString("HH:mm");
            _chatOutput.AppendText($"[color=gray][{timeStr}] 系统: {message}[/color]\n");
            _chatOutput.ScrollToLine(_chatOutput.GetLineCount() - 1);
        }

        private void OnRoomUpdated(string roomId)
        {
            RefreshRoomInfo();
        }

        private void OnPlayerJoined(string playerId, string playerName)
        {
            AddSystemMessage($"{playerName} 加入了房间");
            RefreshRoomInfo();
        }

        private void OnPlayerLeft(string playerId)
        {
            AddSystemMessage("玩家离开了房间");
            RefreshRoomInfo();
        }

        private void OnPlayerReadyChanged(string playerId, bool isReady)
        {
            string status = isReady ? "已准备" : "取消准备";
            AddSystemMessage($"玩家{status}");
            RefreshRoomInfo();
        }

        private void OnGameStarting()
        {
            AddSystemMessage("游戏即将开始！");
            OnGameStarted?.Invoke();
        }

        private void OnHubPlayerJoined(string playerId, string playerName)
        {
            AddSystemMessage($"{playerName} 加入了房间");
            RefreshRoomInfo();
        }

        private void OnHubPlayerLeft(string playerId, string playerName)
        {
            AddSystemMessage($"{playerName} 离开了房间");
            RefreshRoomInfo();
        }

        private void OnHubChatMessage(string senderId, string senderName, string message)
        {
            string localUserId = AuthSystem.Instance?.CurrentUser?.Id ?? "";
            if (senderId != localUserId)
            {
                AddChatMessage(senderName, message, false);
            }
        }

        private void OnHubReadyChanged(string playerId, bool isReady)
        {
            string status = isReady ? "已准备" : "取消准备";
            AddSystemMessage($"玩家{status}");
            RefreshRoomInfo();
        }

        private void OnHubGameStarting(string seed, string roomId)
        {
            AddSystemMessage($"游戏即将开始！Seed: {seed}");
            OnGameStarted?.Invoke();
        }

        private void OnHubBotAdded(string botName)
        {
            AddSystemMessage($"机器人 {botName} 已加入");
            RefreshRoomInfo();
        }

        private void OnHubBotRemoved(string botName)
        {
            AddSystemMessage($"机器人 {botName} 已移除");
            RefreshRoomInfo();
        }

        private void OnHubModeChanged(string roomId, string mode)
        {
            AddSystemMessage($"房间模式已更改为 {mode}");
            RefreshRoomInfo();
        }

        private void OnHubSlotSwapped(int fromSlot, int toSlot)
        {
            AddSystemMessage($"玩家位置交换: #{fromSlot + 1} ↔ #{toSlot + 1}");
            RefreshRoomInfo();
        }
    }
}
