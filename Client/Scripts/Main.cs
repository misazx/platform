using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using RoguelikeGame.Core;
using RoguelikeGame.Systems;
using RoguelikeGame.Database;
using RoguelikeGame.Audio;
using RoguelikeGame.UI;
using RoguelikeGame.UI.Panels;
using RoguelikeGame.Generation;
using RoguelikeGame.Packages;
using RoguelikeGame.Combat;

namespace RoguelikeGame
{
    public partial class Main : SingletonBase<Main>
    {
        private Control _currentSceneContainer;
        private Node _currentScene;
        private Control _settingsPanel;
        private bool _combatActive = false;
        private Generation.NodeType _lastClickedNodeType = Generation.NodeType.Monster;
        private int _lastClickedNodeId = -1;
        public void SetLastClickedNodeType(Generation.NodeType type) => _lastClickedNodeType = type;
        public Generation.NodeType GetLastClickedNodeType() => _lastClickedNodeType;
        public void SetLastClickedNodeId(int id) => _lastClickedNodeId = id;
        public int GetLastClickedNodeId() => _lastClickedNodeId;

        private string _selectedCharacterId = "ironclad";
        private string _selectedCharacterName = "铁甲战士";
        private int _selectedCharacterMaxHp = 80;
        private int _selectedCharacterGold = 99;
        private string[] _selectedCharacterDeck = Array.Empty<string>();
        private string _selectedCharacterRelic = "";

        public void SetSelectedCharacter(string characterId, string characterName, int maxHp, int gold, string[] deck, string relic)
        {
            _selectedCharacterId = characterId;
            _selectedCharacterName = characterName;
            _selectedCharacterMaxHp = maxHp;
            _selectedCharacterGold = gold;
            _selectedCharacterDeck = deck;
            _selectedCharacterRelic = relic;
            GD.Print($"[Main] Selected character: {characterId} ({characterName}), HP={maxHp}, Gold={gold}, Relic={relic}");
        }

        public string GetSelectedCharacterId() => _selectedCharacterId;
        public string GetSelectedCharacterName() => _selectedCharacterName;
        public int GetSelectedCharacterMaxHp() => _selectedCharacterMaxHp;
        public int GetSelectedCharacterGold() => _selectedCharacterGold;
        public string[] GetSelectedCharacterDeck() => _selectedCharacterDeck;
        public string GetSelectedCharacterRelic() => _selectedCharacterRelic;

        private Control _lobby;
        private Control _packageSelector;
        private Control _packageDetail;
        private Variant _packageRegistryData = new Variant();
        private string _currentPackageId = "base_game";

        protected override void OnInitialize()
        {
            GetWindow().Size = new Vector2I(1280, 720);
            GetViewport().TransparentBg = false;

            SetupSceneContainer();
            LoadPackageRegistry();
            GoToLobby();

            GD.Print("[Main] Game initialized with new Lobby flow");
        }

        private void LoadPackageRegistry()
        {
            string configPath = "res://Config/Data/package_registry.json";
            if (ResourceLoader.Exists(configPath))
            {
                var file = FileAccess.Open(configPath, FileAccess.ModeFlags.Read);
                if (file != null)
                {
                    var json = new Json();
                    var err = json.Parse(file.GetAsText());
                    if (err == Error.Ok)
                    {
                        _packageRegistryData = json.Data;
                        GD.Print("[Main] Package registry loaded");
                    }
                }
            }
        }

        private Godot.Collections.Dictionary GetPackageData(string packageId)
        {
            if (_packageRegistryData.VariantType == Variant.Type.Dictionary)
            {
                var reg = _packageRegistryData.AsGodotDictionary();
                if (reg.TryGetValue("packages", out var packagesVar) && packagesVar.VariantType == Variant.Type.Array)
                {
                    var packages = packagesVar.AsGodotArray();
                    foreach (var pkgVar in packages)
                    {
                        if (pkgVar.VariantType == Variant.Type.Dictionary)
                        {
                            var pkg = pkgVar.AsGodotDictionary();
                            if (pkg.TryGetValue("id", out var id) && id.AsString() == packageId)
                                return pkg;
                        }
                    }
                }
            }
            return new Godot.Collections.Dictionary();
        }

        private Control InstantiateGDScript(string scriptPath)
        {
            var script = GD.Load<GDScript>(scriptPath);
            if (script == null)
            {
                GD.PushError($"[Main] Failed to load GDScript: {scriptPath}");
                return null;
            }
            var instance = (Control)script.New();
            return instance;
        }

        public void GoToLobby()
        {
            GD.Print("[Main] Loading Lobby...");
            ClearCurrentScene();

            _lobby = InstantiateGDScript("res://Scripts/UI/Flow/game_lobby.gd");
            if (_lobby != null)
            {
                _lobby.Name = "GameLobby";
                _currentScene = _lobby;
                _currentSceneContainer.AddChild(_lobby);

                _lobby.Connect("open_package_selector", Callable.From(OnOpenPackageSelector));
                _lobby.Connect("open_settings", Callable.From(OnSettingsRequested));
                _lobby.Connect("quit_game", Callable.From(OnQuitRequested));
                _lobby.Connect("open_login", Callable.From(OnLoginRequested));
                _lobby.Connect("open_leaderboard", Callable.From(OnLeaderboardRequested));
            }

            GD.Print("[Main] Lobby loaded");
        }

        private void OnOpenPackageSelector()
        {
            GD.Print("[Main] Opening Package Selector...");
            ClearCurrentScene();

            _packageSelector = InstantiateGDScript("res://Scripts/UI/Flow/package_selector.gd");
            if (_packageSelector != null)
            {
                _packageSelector.Name = "PackageSelector";
                _currentScene = _packageSelector;
                _currentSceneContainer.AddChild(_packageSelector);

                _packageSelector.Connect("package_selected", Callable.From<string>(OnPackageSelected));
                _packageSelector.Connect("back_pressed", Callable.From(GoToLobby));
            }

            GD.Print("[Main] Package Selector loaded");
        }

        private void OnBackFromPackageDetail()
        {
            GD.Print("[Main] Back from Package Detail, returning to lobby");
            ClearCurrentScene();

            if (_packageDetail != null && IsInstanceValid(_packageDetail))
            {
                _packageDetail.QueueFree();
                _packageDetail = null;
            }

            if (_lobby != null && IsInstanceValid(_lobby))
            {
                _lobby.Visible = true;
                _currentScene = _lobby;
            }
            else
            {
                GoToLobby();
            }
        }

        private void OnPackageSelected(string packageId)
        {
            GD.Print($"[Main] Package selected: {packageId}");
            ClearCurrentScene();

            if (_lobby != null && IsInstanceValid(_lobby))
            {
                _lobby.Visible = false;
            }

            var pkgData = GetPackageData(packageId);

            _packageDetail = InstantiateGDScript("res://Scripts/UI/Flow/package_detail.gd");
            if (_packageDetail != null)
            {
                _packageDetail.Name = "PackageDetail";
                _currentScene = _packageDetail;
                _currentSceneContainer.AddChild(_packageDetail);

                _packageDetail.Call("setup", packageId, pkgData);
                _packageDetail.Connect("start_game_requested", Callable.From<string>(OnLaunchPackage));
                _packageDetail.Connect("continue_game_requested", Callable.From<string, int>(OnContinueGame));
                _packageDetail.Connect("back_pressed", Callable.From(OnBackFromPackageDetail));
                _packageDetail.Connect("create_room_requested", Callable.From<string>(OnCreateRoomRequested));
                _packageDetail.Connect("join_room_requested", Callable.From<string>(OnJoinRoomRequested));
            }

            GD.Print($"[Main] Package Detail loaded for: {packageId}");
        }

        private void OnLaunchPackage(string packageId)
        {
            GD.Print($"[Main] Launching package: {packageId}");
            _currentPackageId = packageId;

            if (packageId == "base_game")
            {
                GoToCharacterSelect();
            }
            else if (packageId == "light_shadow_traveler")
            {
                LaunchLightShadowTraveler();
            }
            else
            {
                var pkgData = GetPackageData(packageId);
                string entryScene = pkgData.TryGetValue("entryScene", out var es) ? es.AsString() : "";
                LaunchCustomPackage(entryScene);
            }
        }

        private void OnContinueGame(string packageId, int slotId)
        {
            GD.Print($"[Main] Continue game: {packageId} slot {slotId}");
            _currentPackageId = packageId;

            if (packageId == "base_game")
            {
                GD.Print("[Main] Loading save game via GDScript");
                GoToMap();
            }
            else
            {
                OnLaunchPackage(packageId);
            }
        }

        public void LaunchLightShadowTraveler()
        {
            GD.Print("[Main] Launching 光影旅者...");
            ClearCurrentScene();

            var gameScene = LoadScene("res://GameModes/light_shadow_traveler/Scenes/GameScene.tscn");
            if (gameScene != null)
            {
                _currentScene = gameScene;
                _currentSceneContainer.AddChild(gameScene);
                GD.Print("[Main] 光影旅者 loaded successfully");
            }
            else
            {
                GD.PushError("[Main] Failed to load 光影旅者 scene!");
                GoToLobby();
            }
        }

        private void LaunchCustomPackage(string entryScene)
        {
            if (string.IsNullOrEmpty(entryScene))
            {
                GD.PrintErr("[Main] Custom package has no entry scene!");
                return;
            }

            ClearCurrentScene();
            var scene = LoadScene(entryScene);
            if (scene != null)
            {
                _currentScene = scene;
                _currentSceneContainer.AddChild(scene);
                GD.Print($"[Main] Custom package loaded: {entryScene}");
            }
            else
            {
                GD.PushError($"[Main] Failed to load custom package scene: {entryScene}");
                GoToLobby();
            }
        }

        private void OnSettingsRequested()
        {
            GD.Print("[Main] SettingsRequested signal received");
            ShowSettings();
        }

        private void OnQuitRequested()
        {
            GD.Print("[Main] QuitRequested signal received");
            QuitGame();
        }

        private void OnLoginRequested()
        {
            GD.Print("[Main] Login requested");
            var loginPanel = new LoginPanel();
            AddChild(loginPanel);
            loginPanel.OnLoginSuccess += () =>
            {
                RemoveChild(loginPanel);
                loginPanel.QueueFree();

                if (_lobby != null && IsInstanceValid(_lobby))
                    _lobby.Call("on_login_success");
            };
            loginPanel.OnBack += () =>
            {
                RemoveChild(loginPanel);
                loginPanel.QueueFree();
            };
        }

        private void OnLeaderboardRequested()
        {
            GD.Print("[Main] Leaderboard requested");
            var leaderboardPanel = new LeaderboardPanel();
            AddChild(leaderboardPanel);
            leaderboardPanel.OnBack += () =>
            {
                RemoveChild(leaderboardPanel);
                leaderboardPanel.QueueFree();
            };
        }

        private void OnCreateRoomRequested(string packageId)
        {
            GD.Print($"[Main] Create room requested for: {packageId}");

            if (Network.Auth.AuthSystem.Instance?.IsAuthenticated != true)
            {
                OnLoginRequested();
                return;
            }

            if (_packageDetail != null && IsInstanceValid(_packageDetail))
            {
                _packageDetail.QueueFree();
                _packageDetail = null;
            }

            if (_lobby != null && IsInstanceValid(_lobby))
            {
                _lobby.Visible = false;
            }

            var lobbyPanel = new LobbyPanel();
            lobbyPanel.Name = "LobbyPanel";
            AddChild(lobbyPanel);

            lobbyPanel.OnBackToMenu += () =>
            {
                RemoveChild(lobbyPanel);
                lobbyPanel.QueueFree();
                GoToLobby();
            };

            lobbyPanel.OnLeave += () =>
            {
                RemoveChild(lobbyPanel);
                lobbyPanel.QueueFree();
                GoToLobby();
            };

            lobbyPanel.OnCreateRoom += () =>
            {
                GD.Print("[Main] OnCreateRoom triggered from LobbyPanel");
            };

            lobbyPanel.OnRoomCreatedAndJoined += () =>
            {
                GD.Print("[Main] Room created and joined, opening RoomPanel");
                RemoveChild(lobbyPanel);
                lobbyPanel.QueueFree();
                OpenRoomPanel();
            };

            lobbyPanel.OnJoinRoom += (roomInfo) =>
            {
                GD.Print($"[Main] OnJoinRoom triggered: {roomInfo?.Name}");
                JoinRoomFromLobby(lobbyPanel, roomInfo);
            };

            lobbyPanel.OnLogout += () =>
            {
                Network.Auth.AuthSystem.Instance?.PerformLogout();
                RemoveChild(lobbyPanel);
                lobbyPanel.QueueFree();
                GoToLobby();
            };
        }

        private void OnJoinRoomRequested(string packageId)
        {
            GD.Print($"[Main] Join room requested for: {packageId}");

            if (Network.Auth.AuthSystem.Instance?.IsAuthenticated != true)
            {
                OnLoginRequested();
                return;
            }

            if (_packageDetail != null && IsInstanceValid(_packageDetail))
            {
                _packageDetail.QueueFree();
                _packageDetail = null;
            }

            if (_lobby != null && IsInstanceValid(_lobby))
            {
                _lobby.Visible = false;
            }

            var lobbyPanel = new LobbyPanel();
            lobbyPanel.Name = "LobbyPanel";
            AddChild(lobbyPanel);

            lobbyPanel.OnBackToMenu += () =>
            {
                RemoveChild(lobbyPanel);
                lobbyPanel.QueueFree();
                GoToLobby();
            };

            lobbyPanel.OnLeave += () =>
            {
                RemoveChild(lobbyPanel);
                lobbyPanel.QueueFree();
                GoToLobby();
            };

            lobbyPanel.OnRoomCreatedAndJoined += () =>
            {
                GD.Print("[Main] Room created and joined from join flow, opening RoomPanel");
                RemoveChild(lobbyPanel);
                lobbyPanel.QueueFree();
                OpenRoomPanel();
            };

            lobbyPanel.OnJoinRoom += (roomInfo) =>
            {
                GD.Print($"[Main] OnJoinRoom triggered: {roomInfo?.Name}");
                JoinRoomFromLobby(lobbyPanel, roomInfo);
            };

            lobbyPanel.OnLogout += () =>
            {
                Network.Auth.AuthSystem.Instance?.PerformLogout();
                RemoveChild(lobbyPanel);
                lobbyPanel.QueueFree();
                GoToLobby();
            };
        }

        private async void JoinRoomFromLobby(LobbyPanel lobbyPanel, Network.Rooms.RoomInfo roomInfo)
        {
            if (roomInfo == null)
            {
                GD.PrintErr("[Main] JoinRoomFromLobby: roomInfo is null");
                return;
            }

            GD.Print($"[Main] Joining room: {roomInfo.Name} ({roomInfo.Id})");

            var result = await Network.Rooms.RoomManager.Instance.JoinRoomAsync(roomInfo.Id);

            if (result.Success)
            {
                RemoveChild(lobbyPanel);
                lobbyPanel.QueueFree();
                OpenRoomPanel();
            }
            else
            {
                GD.PrintErr($"[Main] 加入房间失败: {result.Message}");
            }
        }

        private void OpenRoomPanel()
        {
            var roomPanel = new RoomPanel();
            roomPanel.Name = "RoomPanel";
            AddChild(roomPanel);

            roomPanel.OnLeaveRoom += () =>
            {
                RemoveChild(roomPanel);
                roomPanel.QueueFree();
                GoToLobby();
            };

            roomPanel.OnGameStarted += () =>
            {
                RemoveChild(roomPanel);
                roomPanel.QueueFree();
                OnLaunchPackage(_currentPackageId);
            };
        }

        private void SetupSceneContainer()
        {
            _currentSceneContainer = new Control
            {
                Name = "SceneContainer",
                MouseFilter = Control.MouseFilterEnum.Ignore
            };
            _currentSceneContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(_currentSceneContainer);
            GD.Print("[Main] Scene container created");
        }

        public void GoToCharacterSelect()
        {
            GD.Print("[Main] Loading CharacterSelect...");
            ClearCurrentScene();

            var charSelectScene = LoadScene("res://GameModes/base_game/Scenes/CharacterSelect.tscn");
            if (charSelectScene != null)
            {
                _currentScene = charSelectScene;
                _currentSceneContainer.AddChild(charSelectScene);
                GD.Print("[Main] CharacterSelect loaded");
            }
            else
            {
                GD.PushError("[Main] Failed to load CharacterSelect!");
            }
        }

        public void GoToMap()
        {
            GD.Print("========== [Main] GoToMap() START ==========");
            ClearCurrentScene();

            GD.Print("[Main] Loading MapScene.tscn...");
            var mapScene = LoadScene("res://GameModes/base_game/Scenes/MapScene.tscn");

            if (mapScene == null)
            {
                GD.PushError("[Main] MapScene load returned NULL!");
                return;
            }

            GD.Print($"[Main] MapScene loaded, type: {mapScene.GetType().Name}");

            _currentScene = mapScene;
            _currentSceneContainer.AddChild(mapScene);

            if (_lastClickedNodeId >= 0)
            {
                if (mapScene.HasMethod("mark_node_completed"))
                {
                    mapScene.Call("mark_node_completed", _lastClickedNodeId);
                }
                else
                {
                    var childCount = mapScene.GetChildCount();
                    for (int i = 0; i < childCount; i++)
                    {
                        var child = mapScene.GetChild(i);
                        if (child.HasMethod("mark_node_completed"))
                        {
                            child.Call("mark_node_completed", _lastClickedNodeId);
                            break;
                        }
                    }
                }
            }

            GD.Print($"[Main] MapScene added to container, child count: {_currentSceneContainer.GetChildCount()}");
            GD.Print("========== [Main] GoToMap() END ==========");
        }

        public void GoToCombat(string enemyId = "")
        {
            GD.Print("[Main] Loading Combat...");
            ClearCurrentScene();
            _combatActive = true;

            var combatScene = LoadScene("res://GameModes/base_game/Scenes/CombatScene.tscn");
            if (combatScene != null)
            {
                _currentScene = combatScene;
                _currentSceneContainer.AddChild(combatScene);
                GD.Print("[Main] Combat scene loaded - GDScript will handle initialization");
            }
        }

        public void GoToShop()
        {
            GD.Print("[Main] Loading Shop...");
            ClearCurrentScene();

            var shopScene = LoadScene("res://GameModes/base_game/Scenes/ShopPanel.tscn");
            if (shopScene != null)
            {
                _currentScene = shopScene;
                _currentSceneContainer.AddChild(shopScene);
            }
        }

        public void GoToRest()
        {
            GD.Print("[Main] Loading Rest Site...");
            ClearCurrentScene();

            var restScene = LoadScene("res://GameModes/base_game/Scenes/RestSitePanel.tscn");
            if (restScene != null)
            {
                _currentScene = restScene;
                _currentSceneContainer.AddChild(restScene);
            }
        }

        public void GoToEvent()
        {
            GD.Print("[Main] Loading Event...");
            ClearCurrentScene();

            var eventScene = LoadScene("res://GameModes/base_game/Scenes/EventPanel.tscn");
            if (eventScene != null)
            {
                _currentScene = eventScene;
                _currentSceneContainer.AddChild(eventScene);
            }
        }

        public void GoToTreasure()
        {
            GD.Print("[Main] Loading Treasure...");
            ClearCurrentScene();

            var treasureScene = LoadScene("res://GameModes/base_game/Scenes/TreasurePanel.tscn");
            if (treasureScene != null)
            {
                _currentScene = treasureScene;
                _currentSceneContainer.AddChild(treasureScene);
            }
        }

        public void ShowSettings()
        {
            GD.Print("[Main] Showing Settings panel");

            if (_settingsPanel == null)
            {
                _settingsPanel = CreateSettingsPanel();
                AddChild(_settingsPanel);
            }

            _settingsPanel.Visible = true;
        }

        public void HideSettings()
        {
            if (_settingsPanel != null)
            {
                _settingsPanel.Visible = false;
            }
        }

        private Control CreateSettingsPanel()
        {
            var panel = new PanelContainer
            {
                Name = "SettingsPanel",
                AnchorsPreset = (int)Control.LayoutPreset.Center,
                CustomMinimumSize = new Vector2(400, 300)
            };

            var style = new StyleBoxFlat
            {
                BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f),
                CornerRadiusTopLeft = 12,
                CornerRadiusTopRight = 12,
                CornerRadiusBottomLeft = 12,
                CornerRadiusBottomRight = 12,
                BorderWidthLeft = 2,
                BorderWidthRight = 2,
                BorderWidthTop = 2,
                BorderWidthBottom = 2,
                BorderColor = new Color(0.3f, 0.3f, 0.4f)
            };
            panel.AddThemeStyleboxOverride("panel", style);

            var vbox = new VBoxContainer();
            panel.AddChild(vbox);

            var title = new Label
            {
                Text = "设置",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            title.AddThemeFontSizeOverride("font_size", 24);
            vbox.AddChild(title);

            var spacer = new Control { CustomMinimumSize = new Vector2(0, 20) };
            vbox.AddChild(spacer);

            var musicLabel = new Label { Text = "音乐音量" };
            vbox.AddChild(musicLabel);

            var musicSlider = new HSlider
            {
                MinValue = 0,
                MaxValue = 100,
                Value = 80,
                CustomMinimumSize = new Vector2(300, 30)
            };
            vbox.AddChild(musicSlider);

            var sfxLabel = new Label { Text = "音效音量" };
            vbox.AddChild(sfxLabel);

            var sfxSlider = new HSlider
            {
                MinValue = 0,
                MaxValue = 100,
                Value = 100,
                CustomMinimumSize = new Vector2(300, 30)
            };
            vbox.AddChild(sfxSlider);

            var spacer2 = new Control { CustomMinimumSize = new Vector2(0, 20) };
            vbox.AddChild(spacer2);

            var closeButton = new Button
            {
                Text = "关闭",
                CustomMinimumSize = new Vector2(120, 40)
            };
            closeButton.Pressed += HideSettings;
            vbox.AddChild(closeButton);

            return panel;
        }

        public void QuitGame()
        {
            GetTree().Quit();
        }

        private Node LoadScene(string path)
        {
            if (!ResourceLoader.Exists(path))
            {
                GD.PushError($"[Main] Scene not found: {path}");
                return null;
            }

            var packed = ResourceLoader.Load<PackedScene>(path);
            if (packed != null)
            {
                var instance = packed.Instantiate();
                GD.Print($"[Main] Scene instantiated: {path}");
                return instance;
            }

            GD.PushError($"[Main] Failed to instantiate: {path}");
            return null;
        }

        private void ClearCurrentScene()
        {
            _combatActive = false;
            if (_currentScene != null)
            {
                _currentScene.QueueFree();
                _currentScene = null;
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("cancel"))
            {
                if (_settingsPanel != null && _settingsPanel.Visible)
                {
                    HideSettings();
                }
            }

            if (@event is InputEventKey key && key.Keycode == Key.F11 && key.Pressed)
            {
                bool fullscreen = GetWindow().Mode != Window.ModeEnum.Fullscreen;
                GetWindow().Mode = fullscreen ? Window.ModeEnum.Fullscreen : Window.ModeEnum.Windowed;
            }
        }
    }
}
