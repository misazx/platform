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

namespace RoguelikeGame
{
    public partial class Main : SingletonBase<Main>
    {
        private Control _currentSceneContainer;
        private Node _currentScene;
        private Control _settingsPanel;
        private bool _combatActive = false;
        private int _currentGoldReward = 20;
        private Generation.NodeType _lastClickedNodeType = Generation.NodeType.Monster;

        public void SetLastClickedNodeType(Generation.NodeType type) => _lastClickedNodeType = type;
        public Generation.NodeType GetLastClickedNodeType() => _lastClickedNodeType;

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

        private void OnPackageSelected(string packageId)
        {
            GD.Print($"[Main] Package selected: {packageId}");
            ClearCurrentScene();

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
                _packageDetail.Connect("back_pressed", Callable.From(OnOpenPackageSelector));
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
                var saveSlot = EnhancedSaveSystem.Instance?.LoadGame(slotId);
                if (saveSlot != null)
                {
                    GD.Print($"[Main] Loaded save: {saveSlot.CharacterId} at floor {saveSlot.CurrentFloor}");
                    MapView.ResetPersistentState();
                    if (GameManager.Instance != null)
                        GameManager.Instance.StartNewRun(saveSlot.CharacterId);
                    GoToMap();
                }
                else
                {
                    GD.Print("[Main] No save found, starting new game");
                    GoToCharacterSelect();
                }
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

            var lobbyPanel = new LobbyPanel();
            AddChild(lobbyPanel);
            lobbyPanel.OnLeave += () =>
            {
                RemoveChild(lobbyPanel);
                lobbyPanel.QueueFree();
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

            var lobbyPanel = new LobbyPanel();
            AddChild(lobbyPanel);
            lobbyPanel.OnLeave += () =>
            {
                RemoveChild(lobbyPanel);
                lobbyPanel.QueueFree();
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

            var charSelect = LoadScene("res://GameModes/base_game/Scenes/CharacterSelect.tscn");
            if (charSelect is CharacterSelect cs)
            {
                _currentScene = charSelect;
                _currentSceneContainer.AddChild(charSelect);
                
                cs.CharacterSelected += OnCharacterSelected;
                
                GD.Print("[Main] CharacterSelect loaded and signals connected");
            }
            else
            {
                GD.PushError("[Main] Failed to load CharacterSelect!");
            }
        }

        private void OnCharacterSelected(string characterId)
        {
            GD.Print($"[Main] CharacterSelected signal received: {characterId}");
            
            MapView.ResetPersistentState();
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartNewRun(characterId);
            }
            
            GoToMap();
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
                GD.Print("[Main] Combat loaded");

                InitializeCombatSystem(combatScene, enemyId);
            }
        }

        public void GoToShop()
        {
            GD.Print("[Main] Loading Shop...");
            ClearCurrentScene();

            var shopPanel = new ShopPanel();
            _currentScene = shopPanel;
            _currentSceneContainer.AddChild(shopPanel);
            shopPanel.Closed += GoToMap;
        }

        public void GoToRest()
        {
            GD.Print("[Main] Loading Rest Site...");
            ClearCurrentScene();

            var restPanel = new RestSitePanel();
            _currentScene = restPanel;
            _currentSceneContainer.AddChild(restPanel);
            restPanel.Closed += GoToMap;
        }

        public void GoToEvent()
        {
            GD.Print("[Main] Loading Event...");
            ClearCurrentScene();

            var eventPanel = new EventPanel();
            _currentScene = eventPanel;
            _currentSceneContainer.AddChild(eventPanel);
            eventPanel.Closed += GoToMap;
        }

        public void GoToTreasure()
        {
            GD.Print("[Main] Loading Treasure...");
            ClearCurrentScene();

            var treasurePanel = new TreasurePanel();
            _currentScene = treasurePanel;
            _currentSceneContainer.AddChild(treasurePanel);
            treasurePanel.Closed += GoToMap;
        }

        private void InitializeCombatSystem(Node combatScene, string enemyId)
        {
            GD.Print($"[Main] Initializing STS combat system for enemy: {enemyId}");

            var combatHUD = combatScene as CombatHUD;
            if (combatHUD == null)
            {
                GD.PushError("[Main] CombatScene is not a CombatHUD instance!");
                return;
            }

            int floor = GameManager.Instance?.CurrentRun?.CurrentFloor ?? 1;
            NodeType nodeType = GetLastClickedNodeType();

            var encounter = EncounterGenerator.GenerateEncounter(enemyId, nodeType, floor);
            GD.Print($"[Main] Encounter: {encounter.Description}, {encounter.Enemies.Count} enemies");

            uint seed = (uint)DateTime.Now.Ticks;

            var engine = new StsCombatEngine();
            AddChild(engine);
            engine.InitializeCombat(encounter.Enemies, seed);

            foreach (var enemy in encounter.Enemies)
            {
                int idx = 0;
                for (int i = 0; i < encounter.Enemies.Count; i++)
                    if (encounter.Enemies[i] == enemy) { idx = i; break; }
                combatHUD.AddEnemy(enemy.Name, enemy.MaxHp);
                if (enemy.CurrentIntent != null)
                    combatHUD.UpdateEnemyIntent(idx, enemy.CurrentIntent.Description, enemy.CurrentIntent.Icon);
            }

            ConnectStsSignals(combatHUD, engine);
            UpdateUIFromEngine(combatHUD, engine);

            _currentGoldReward = encounter.GoldReward;

            combatHUD.ShowPileViewRequested += (pileType) =>
            {
                List<CardData> cards = new();
                if (pileType == "抽牌堆")
                {
                    foreach (var c in engine.Player.DrawPile)
                        cards.Add(ConvertToCardData(c));
                }
                else
                {
                    foreach (var c in engine.Player.DiscardPile)
                        cards.Add(ConvertToCardData(c));
                }

                var pileView = new PileViewPanel(pileType, cards);
                AddChild(pileView);
                pileView.Closed += () => { RemoveChild(pileView); pileView.QueueFree(); };
            };

            GD.Print("[Main] STS combat system initialized successfully");
        }

        private void ConnectStsSignals(CombatHUD hud, StsCombatEngine engine)
        {
            hud.CardPlayed += (cardId) =>
            {
                if (!_combatActive || !IsInstanceValid(engine)) return;
                if (!engine.IsPlayerTurn || engine.IsCombatOver) return;

                GD.Print($"[Main] 🃏 Card played: {cardId}");

                var card = engine.Player.Hand.Find(c => c.Id == cardId);
                if (card != null && engine.CanPlayCard(card))
                {
                    var result = engine.PlayCard(card);

                    UpdateEnemyHealthFromEngine(hud, engine);
                    UpdateUIFromEngine(hud, engine);
                }
            };

            hud.CardPlayedWithTarget += (cardId, targetIndex) =>
            {
                if (!_combatActive || !IsInstanceValid(engine)) return;
                if (!engine.IsPlayerTurn || engine.IsCombatOver) return;

                GD.Print($"[Main] 🃏 Card played with target: {cardId} → enemy {targetIndex}");

                var card = engine.Player.Hand.Find(c => c.Id == cardId);
                if (card != null && engine.CanPlayCard(card))
                {
                    var result = engine.PlayCard(card, targetIndex);

                    UpdateEnemyHealthFromEngine(hud, engine);
                    UpdateUIFromEngine(hud, engine);
                }
            };

            hud.EndTurn += () =>
            {
                if (!_combatActive || !IsInstanceValid(engine)) return;
                if (engine.IsCombatOver) return;

                GD.Print("[Main] ⚔️ End turn - enemy phase starting");
                hud.SetPhase(false);

                CallDeferred(nameof(ExecuteEnemyTurn), hud, engine);
            };

            engine.OnDamageDealt += (targetIndex, targetName, damage) =>
            {
                if (!_combatActive) return;

                bool isPlayer = targetIndex < 0;
                Vector2 pos = isPlayer ? new Vector2(150, 350) : new Vector2(600 + targetIndex * 180, 180);
                FloatingText.ShowDamage(hud, damage, pos, isPlayer);
                AudioManager.Instance?.PlaySFX("hit");

                if (isPlayer)
                    hud.ShowPlayerHitFeedback();
                else if (targetIndex >= 0)
                    hud.ShowEnemyHitFeedback(targetIndex);
            };

            engine.OnBlockGained += (amount, totalBlock) =>
            {
                if (!_combatActive) return;
                FloatingText.ShowBlock(hud, amount, new Vector2(150, 380));
            };

            engine.OnStatusApplied += (targetName, effect) =>
            {
                if (!_combatActive) return;
                GD.Print($"[Main] ✨ Status applied to {targetName}: {effect.Name} +{effect.Stacks}");
                FloatingText.ShowStatus(hud, $"{effect.Name} +{effect.Stacks}", new Vector2(600, 250));
            };

            engine.OnTurnStarted += (turn) =>
            {
                if (!_combatActive || !IsInstanceValid(engine)) return;
                if (engine.IsCombatOver) return;

                GD.Print($"[Main] 🔄 Turn {turn} started - player phase");
                hud.SetPhase(true);
                UpdateUIFromEngine(hud, engine);
                UpdateEnemyIntentsFromEngine(hud, engine);
            };

            engine.OnCombatWon += () =>
            {
                if (!_combatActive) return;
                _combatActive = false;

                GD.Print("[Main] 🏆 VICTORY!");
                hud.SetPhase(false);
                UpdateEnemyHealthFromEngine(hud, engine);
                FloatingText.ShowStatus(hud, "🏆 胜利!", new Vector2(600, 300));
                AudioManager.Instance?.PlayBGM("victory_fanfare");

                var run = GameManager.Instance?.CurrentRun;
                if (run != null)
                {
                    run.TotalEnemiesDefeated++;
                    run.EndTime = DateTime.Now;
                }

                GameManager.Instance?.EndCombat(true);

                AchievementManager.Instance?.UpdateProgress("kill_100_enemies", 1);
                AchievementManager.Instance?.UpdateProgress("first_victory", 1);
                EnhancedSaveSystem.Instance?.SaveGame(1, run);

                SubmitRunToServer(run, true);

                bool isBoss = engine.Enemies.Any(e => e.IsDead && (e.Id?.Contains("Guardian") == true || e.Id?.Contains("Collector") == true || e.Id?.Contains("Automaton") == true || e.Id?.Contains("Awakener") == true));

                GetTree().CreateTimer(2.0f).Timeout += () =>
                {
                    if (isBoss && run != null && run.CurrentFloor >= 3)
                    {
                        GD.Print("[Main] Boss defeated on final floor - showing VictoryScreen");
                        var victoryScreen = new VictoryScreen(run);
                        AddChild(victoryScreen);
                        victoryScreen.Closed += () =>
                        {
                            RemoveChild(victoryScreen);
                            victoryScreen.QueueFree();
                            if (engine != null && IsInstanceValid(engine))
                                engine.QueueFree();
                        };
                    }
                    else
                    {
                        GD.Print("[Main] Showing reward screen");
                        var cardChoices = new List<CardData>();
                        var cardDb = CardDatabase.Instance;
                        if (cardDb != null)
                        {
                            var allCards = cardDb.GetAllCards();
                            var rng = new Random();
                            for (int i = 0; i < 3 && allCards.Count > 0; i++)
                            {
                                int idx = rng.Next(allCards.Count);
                                cardChoices.Add(allCards[idx]);
                                allCards.RemoveAt(idx);
                            }
                        }

                        var rewardPanel = new RewardPanel(new Random().Next(15, 40), cardChoices);
                        AddChild(rewardPanel);
                        rewardPanel.Closed += () =>
                        {
                            RemoveChild(rewardPanel);
                            rewardPanel.QueueFree();
                            if (engine != null && IsInstanceValid(engine))
                                engine.QueueFree();
                            GoToMap();
                        };
                    }
                };
            };

            engine.OnCombatLost += () =>
            {
                if (!_combatActive) return;
                _combatActive = false;

                GD.Print("[Main] 💀 DEFEATED...");
                hud.SetPhase(false);
                FloatingText.ShowStatus(hud, "💀 战败...", new Vector2(600, 300));
                AudioManager.Instance?.PlayBGM("game_over");

                var run = GameManager.Instance?.CurrentRun;
                if (run != null)
                {
                    run.EndTime = DateTime.Now;
                    run.IsVictory = false;
                }

                GameManager.Instance?.EndCombat(false);

                SubmitRunToServer(run, false);

                GetTree().CreateTimer(2.5f).Timeout += () =>
                {
                    GD.Print("[Main] Showing GameOverScreen");
                    var gameOverScreen = new GameOverScreen(run);
                    AddChild(gameOverScreen);
                    gameOverScreen.Closed += () =>
                    {
                        RemoveChild(gameOverScreen);
                        gameOverScreen.QueueFree();
                        if (engine != null && IsInstanceValid(engine))
                            engine.QueueFree();
                    };
                };
            };
        }

        private void ExecuteEnemyTurn(CombatHUD hud, StsCombatEngine engine)
        {
            if (!_combatActive || !IsInstanceValid(engine)) return;
            if (engine.IsCombatOver) return;

            GD.Print("[Main] 👾 Executing enemy turn...");
            hud.SetProcessing(true);

            engine.EndTurn();

            UpdateEnemyHealthFromEngine(hud, engine);
            UpdateUIFromEngine(hud, engine);

            GetTree().CreateTimer(0.8f).Timeout += () =>
            {
                hud.SetProcessing(false);
                GD.Print("[Main] Enemy turn complete");
            };
        }

        private void UpdateUIFromEngine(CombatHUD hud, StsCombatEngine engine)
        {
            if (!IsInstanceValid(engine)) return;

            var player = engine.Player;

            List<CardData> cardDataList = new();
            foreach (var stsCard in player.Hand)
            {
                cardDataList.Add(ConvertToCardData(stsCard));
            }
            hud.UpdateHand(cardDataList);

            hud.UpdateEnergy(player.Energy, player.MaxEnergy);
            hud.UpdateHealth(player.CurrentHp, player.MaxHp);
            hud.UpdateBlock(player.Block);
            hud.UpdateDrawPile(player.DrawPile.Count);
            hud.UpdateDiscardPile(player.DiscardPile.Count);
            hud.SetTurnNumber(engine.TurnNumber);

            var gm = GameManager.Instance;
            if (gm?.CurrentRun != null)
                hud.SetFloorNumber(gm.CurrentRun.CurrentFloor);
        }

        private void UpdateEnemyHealthFromEngine(CombatHUD hud, StsCombatEngine engine)
        {
            if (!IsInstanceValid(engine)) return;

            int i = 0;
            foreach (var enemy in engine.Enemies)
            {
                if (enemy.IsDead)
                {
                    hud.UpdateEnemyHealth(i, 0, enemy.MaxHp);
                }
                else
                {
                    hud.UpdateEnemyHealth(i, enemy.CurrentHp, enemy.MaxHp);
                }
                i++;
            }
        }

        private void UpdateEnemyIntentsFromEngine(CombatHUD hud, StsCombatEngine engine)
        {
            if (!IsInstanceValid(engine)) return;

            int i = 0;
            foreach (var enemy in engine.Enemies)
            {
                if (!enemy.IsDead && enemy.CurrentIntent != null)
                {
                    hud.UpdateEnemyIntent(i, enemy.CurrentIntent.Description, enemy.CurrentIntent.Icon);
                }
                i++;
            }
        }

        private CardData ConvertToCardData(StsCardData stsCard)
        {
            return new CardData
            {
                Id = stsCard.Id,
                Name = stsCard.Name,
                Description = stsCard.Description,
                Cost = stsCard.Cost,
                Type = ConvertToCardType(stsCard.Type),
                Damage = stsCard.Damage
            };
        }

        private CardType ConvertToCardType(StsCardType stsType) => stsType switch
        {
            StsCardType.Attack => CardType.Attack,
            StsCardType.Skill => CardType.Skill,
            StsCardType.Power => CardType.Power,
            _ => CardType.Status
        };

        private int GetEnemyHealth(string enemyId) => enemyId switch
        {
            "Cultist" or "cultist" => 50,
            "JawWorm" or "jawworm" => 60,
            "Louse" or "louse" => 35,
            "Gremlin_Nob" or "gremlin_nob" => 82,
            "The_Guardian" or "the_guardian" => 200,
            _ => 48
        };

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

        private async void SubmitRunToServer(RunData run, bool isVictory)
        {
            try
            {
                if (run == null) return;

                var authSystem = Network.Auth.AuthSystem.Instance;
                if (authSystem?.IsAuthenticated != true)
                {
                    GD.Print("[Main] 未登录，跳过服务器提交");
                    return;
                }

                long score = (long)(run.Gold * 10 + run.TotalEnemiesDefeated * 50 + run.CurrentFloor * 100);
                if (isVictory) score += 5000;

                double playTimeSeconds = (run.EndTime - run.StartTime).TotalSeconds;

                var requestData = new
                {
                    score,
                    floorReached = run.CurrentFloor,
                    killCount = run.TotalEnemiesDefeated,
                    playTimeSeconds,
                    characterUsed = run.CharacterId ?? "ironclad",
                    isVictory
                };

                var json = System.Text.Json.JsonSerializer.Serialize(requestData);

                using var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authSystem.Token);
                client.Timeout = TimeSpan.FromSeconds(5);

                var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"http://127.0.0.1:5000/api/leaderboard/{_currentPackageId}/submit", content);

                if (response.IsSuccessStatusCode)
                {
                    GD.Print($"[Main] ✅ 分数已提交到服务器: {score} (胜利={isVictory})");
                }
                else
                {
                    GD.PrintErr($"[Main] 提交分数失败: {response.StatusCode}");
                }

                SyncAchievementsToServer();
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[Main] 提交分数异常: {ex.Message}");
            }
        }

        private async void SyncAchievementsToServer()
        {
            try
            {
                var authSystem = Network.Auth.AuthSystem.Instance;
                if (authSystem?.IsAuthenticated != true) return;

                var achievements = new List<object>
                {
                    new { achievementId = "first_victory", achievementName = "初次胜利", description = "首次通关游戏", isUnlocked = false, progress = 0, target = 1 },
                    new { achievementId = "kill_100_enemies", achievementName = "百人斩", description = "击败100个敌人", isUnlocked = false, progress = 0, target = 100 },
                    new { achievementId = "all_relics", achievementName = "收藏家", description = "收集所有遗物", isUnlocked = false, progress = 0, target = 1 },
                    new { achievementId = "no_damage", achievementName = "无伤通关", description = "不受伤完成一场战斗", isUnlocked = false, progress = 0, target = 1 }
                };

                var achManager = AchievementManager.Instance;
                if (achManager != null)
                {
                    achievements.Clear();
                    foreach (var ach in achManager.GetAllDefinitions(true))
                    {
                        achievements.Add(new
                        {
                            achievementId = ach.Id,
                            achievementName = ach.Name,
                            description = ach.Description,
                            isUnlocked = ach.IsUnlocked,
                            progress = ach.CurrentProgress,
                            target = ach.MaxProgress
                        });
                    }
                }

                var requestData = new { achievements };
                var json = System.Text.Json.JsonSerializer.Serialize(requestData);

                using var client = new System.Net.Http.HttpClient();
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authSystem.Token);
                client.Timeout = TimeSpan.FromSeconds(5);

                var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"http://127.0.0.1:5000/api/achievement/{_currentPackageId}/sync", content);

                if (response.IsSuccessStatusCode)
                    GD.Print("[Main] ✅ 成就已同步到服务器");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[Main] 同步成就异常: {ex.Message}");
            }
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
