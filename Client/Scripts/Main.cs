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
    public partial class Main : Node
    {
        public static Main Instance { get; private set; }

        private Control _currentSceneContainer;
        private Node _currentScene;
        private Control _settingsPanel;
        private bool _combatActive = false;
        private int _currentGoldReward = 20;
        private Generation.NodeType _lastClickedNodeType = Generation.NodeType.Monster;

        public void SetLastClickedNodeType(Generation.NodeType type) => _lastClickedNodeType = type;
        public Generation.NodeType GetLastClickedNodeType() => _lastClickedNodeType;
        private PackageStoreUI _packageStoreUI;

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            Instance = this;

            GetWindow().Size = new Vector2I(1280, 720);
            GetViewport().TransparentBg = false;

            SetupSceneContainer();
            CreatePackageStoreOverlay();
            GoToMainMenu();

            GD.Print("[Main] Game initialized successfully with Package System");
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

        private void CreatePackageStoreOverlay()
        {
            _packageStoreUI = new PackageStoreUI();
            _packageStoreUI.Visible = false;
            _packageStoreUI.Name = "PackageStoreUI";
            AddChild(_packageStoreUI);
            
            _packageStoreUI.PackageLaunchRequested += OnPackageLaunchFromStore;
            
            GD.Print("[Main] Package Store UI created (hidden)");
        }

        public void GoToMainMenu()
        {
            GD.Print("[Main] Loading MainMenu...");
            ClearCurrentScene();

            var mainMenu = LoadScene("res://Scenes/MainMenu.tscn");
            if (mainMenu is MainMenuScene menuScene)
            {
                _currentScene = mainMenu;
                _currentSceneContainer.AddChild(mainMenu);
                
                menuScene.StartGameRequested += OnStartGameRequested;
                menuScene.SettingsRequested += OnSettingsRequested;
                menuScene.QuitRequested += OnQuitRequested;
                
                InjectPackageButton(menuScene);
                
                GD.Print("[Main] MainMenu loaded with Package Store integration");
            }
            else
            {
                GD.PushError("[Main] Failed to load MainMenu!");
            }
        }

        private void InjectPackageButton(MainMenuScene menuScene)
        {
            try
            {
                var vboxContainer = menuScene.GetNodeOrNull<VBoxContainer>("VBoxContainer");
                if (vboxContainer == null)
                {
                    GD.PushWarning("[Main] VBoxContainer not found in MainMenu, skipping package button");
                    return;
                }

                var packageBtn = new Button
                {
                    Text = "📦 游戏包商店",
                    CustomMinimumSize = new Vector2(220, 50),
                    Modulate = new Color(0.9f, 0.85f, 1f)
                };
                
                packageBtn.AddThemeFontSizeOverride("font_size", 18);
                packageBtn.Pressed += () => 
                {
                    GD.Print("[Main] Opening Package Store...");
                    AudioManager.Instance?.PlayButtonClick();
                    OpenPackageStore();
                };

                var quitBtn = vboxContainer.GetNodeOrNull<Button>("QuitButton");
                if (quitBtn != null)
                {
                    int quitIndex = quitBtn.GetIndex();
                    vboxContainer.MoveChild(packageBtn, quitIndex);
                }
                else
                {
                    vboxContainer.AddChild(packageBtn);
                }

                GD.Print("[Main] ✅ Package Store button injected into MainMenu");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[Main] Failed to inject package button: {ex.Message}");
            }
        }

        private void OpenPackageStore()
        {
            if (_packageStoreUI == null)
            {
                GD.PrintErr("[Main] Package Store UI not initialized!");
                return;
            }

            _packageStoreUI.Visible = true;
            _packageStoreUI.RefreshPackageList();
            
            GD.Print("[Main] Package Store opened");
        }

        private void OnPackageLaunchFromStore(PackageData package)
        {
            GD.Print($"[Main] Launching package from store: {package.Name}");
            _packageStoreUI.Visible = false;
            
            if (package.Id == "base_game")
            {
                OnStartGameRequested();
            }
            else
            {
                GD.Print($"[Main] Custom package launch: {package.EntryScene}");
                // TODO: 处理自定义包的启动逻辑
            }
        }

        private void OnStartGameRequested()
        {
            GD.Print("[Main] StartGameRequested signal received");
            GoToCharacterSelect();
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

        public void GoToCharacterSelect()
        {
            GD.Print("[Main] Loading CharacterSelect...");
            ClearCurrentScene();

            var charSelect = LoadScene("res://Scenes/CharacterSelect.tscn");
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
            var mapScene = LoadScene("res://Scenes/MapScene.tscn");

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

            var combatScene = LoadScene("res://Scenes/CombatScene.tscn");
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

        public void QuitGame()
        {
            GD.Print("[Main] Quitting game");
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
