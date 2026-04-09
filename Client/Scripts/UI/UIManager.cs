using Godot;
using System.Collections.Generic;
using RoguelikeGame.Audio;

namespace RoguelikeGame.UI
{
    [GlobalClass]
    public partial class UIManager : Control
    {
        public static UIManager Instance { get; private set; }
        
        private Control _mainMenu;
        private Control _characterSelect;
        private Control _combatUI;
        private Control _mapView;
        private Control _eventPanel;
        private Control _shopPanel;
        private Control _restSitePanel;
        private Control _gameOverScreen;
        private Control _victoryScreen;
        private Control _achievementPopup;
        private Control _tutorialOverlay;
        private Control _settingsPanel;

        private CanvasLayer _popupLayer;
        private CanvasLayer _animationLayer;

        private readonly Stack<Control> _screenStack = new();

        public override void _Ready()
        {
            Instance = this;
            InitializeLayers();
            LoadAllScreens();
            ShowScreen("MainMenu");
        }

        private void InitializeLayers()
        {
            _popupLayer = new CanvasLayer { Layer = 10 };
            _animationLayer = new CanvasLayer { Layer = 5 };
            AddChild(_popupLayer);
            AddChild(_animationLayer);
        }

        private void LoadAllScreens()
        {
            _mainMenu = LoadUIScreen("MainMenu");
            _characterSelect = LoadUIScreen("CharacterSelect");
            _combatUI = LoadUIScreen("CombatScene");
            _mapView = LoadUIScreen("MapScene");
            _eventPanel = LoadUIScreen("EventPanel");
            _shopPanel = LoadUIScreen("ShopPanel");
            _restSitePanel = LoadUIScreen("RestSitePanel");
            _gameOverScreen = LoadUIScreen("GameOverScreen");
            _victoryScreen = LoadUIScreen("VictoryScreen");
            _achievementPopup = LoadUIScreen("AchievementPopup");
            _tutorialOverlay = LoadUIScreen("TutorialOverlay");
            _settingsPanel = LoadUIScreen("SettingsPanel");
        }

        private Control LoadUIScreen(string screenName)
        {
            var scenePath = $"res://Scenes/{screenName}.tscn";
            
            if (!ResourceLoader.Exists(scenePath))
            {
                GD.Print($"[UIManager] Scene not found: {scenePath}, creating placeholder");
                return CreatePlaceholder(screenName);
            }
            
            var packed = ResourceLoader.Load<PackedScene>(scenePath);
            
            if (packed != null)
            {
                var instance = (Control)packed.Instantiate();
                instance.Visible = false;
                AddChild(instance);
                return instance;
            }

            return CreatePlaceholder(screenName);
        }

        private Control CreatePlaceholder(string name)
        {
            var placeholder = new ColorRect
            {
                Color = new Color(0.1f, 0.1f, 0.15f),
                AnchorsPreset = (int)Control.LayoutPreset.FullRect,
                Visible = false
            };

            var label = new Label
            {
                Text = $"[UI: {name}]",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Modulate = Colors.White
            };

            placeholder.AddChild(label);
            return placeholder;
        }

        public void ShowScreen(string screenName)
        {
            HideAllScreens();
            var screen = GetScreen(screenName);
            if (screen != null)
            {
                screen.Visible = true;
                _screenStack.Push(screen);
                
                AudioManager.Instance?.PlayButtonClick();
                
                if (screen is IUIScreen uiScreen)
                    uiScreen.OnShow();
            }
        }

        public void PopScreen()
        {
            if (_screenStack.Count > 1)
            {
                _screenStack.Pop().Visible = false;
                _screenStack.Peek().Visible = true;
            }
        }

        public void HideAllScreens()
        {
            foreach (var child in GetChildren())
                if (child is Control ctrl)
                    ctrl.Visible = false;
            _screenStack.Clear();
        }

        public Control GetScreen(string name) => name switch
        {
            "MainMenu" => _mainMenu,
            "CharacterSelect" => _characterSelect,
            "CombatScene" => _combatUI,
            "CombatHUD" => _combatUI,
            "MapScene" => _mapView,
            "MapView" => _mapView,
            "EventPanel" => _eventPanel,
            "ShopPanel" => _shopPanel,
            "RestSitePanel" => _restSitePanel,
            "GameOverScreen" => _gameOverScreen,
            "VictoryScreen" => _victoryScreen,
            "SettingsPanel" => _settingsPanel,
            _ => null
        };

        public T GetScreen<T>(string name) where T : Control => GetScreen(name) as T;

        public void ShowPopup(Control popup)
        {
            _popupLayer.AddChild(popup);
        }

        public void DismissPopup(Control popup)
        {
            if (popup.GetParent() == _popupLayer)
                popup.QueueFree();
        }

        public Node GetAnimationLayer() => _animationLayer;

        public void ShowAchievementPopup(string title, string description, Texture2D icon)
        {
            var popup = CreateAchievementPopup(title, description, icon);
            ShowPopup(popup);

            var timer = new Godot.Timer { WaitTime = 3f, OneShot = true };
            timer.Timeout += () => DismissPopup(popup);
            popup.AddChild(timer);
            timer.Start();
        }

        private Control CreateAchievementPopup(string title, string desc, Texture2D icon)
        {
            var container = new PanelContainer
            {
                AnchorsPreset = (int)Control.LayoutPreset.TopWide,
                OffsetTop = 100,
                Position = new Vector2(0, -200)
            };

            var styleBox = new StyleBoxFlat
            {
                BgColor = new Color(0.15f, 0.12f, 0.08f, 0.95f),
                ContentMarginLeft = 20,
                ContentMarginRight = 20,
                ContentMarginTop = 15,
                ContentMarginBottom = 15,
                CornerRadiusTopLeft = 8,
                CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8,
                CornerRadiusBottomRight = 8,
                BorderWidthLeft = 2,
                BorderWidthRight = 2,
                BorderWidthTop = 2,
                BorderWidthBottom = 2,
                BorderColor = new Color(1f, 0.85f, 0.3f, 1f)
            };

            container.AddThemeStyleboxOverride("panel", styleBox);

            var hbox = new HBoxContainer
            {
                Alignment = BoxContainer.AlignmentMode.Center
            };

            if (icon != null)
            {
                var textureRect = new TextureRect
                {
                    Texture = icon,
                    CustomMinimumSize = new Vector2(48, 48),
                    ExpandMode = TextureRect.ExpandModeEnum.FitWidthProportional,
                    StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered
                };
                hbox.AddChild(textureRect);
            }

            var vbox = new VBoxContainer();
            var titleLabel = new Label
            {
                Text = title,

            };
            var descLabel = new Label
            {
                Text = desc,
                Modulate = Colors.LightGray,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            vbox.AddChild(titleLabel);
            vbox.AddChild(descLabel);
            hbox.AddChild(vbox);
            container.AddChild(hbox);

            var tween = container.CreateTween();
            tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
            tween.TweenProperty(container, "position:y", 100, 0.4f);

            return container;
        }

        public void ShowFloatingText(Vector2 worldPos, string text, Color color, float duration = 1f)
        {
            var label = new Label
            {
                Text = text,
                Modulate = color,
                
                
                ZIndex = 100
            };
            label.Position = worldPos;
            _animationLayer.AddChild(label);

            var tween = label.CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(label, "position:y", worldPos.Y - 60, duration);
            tween.TweenProperty(label, "modulate:a", 0f, duration * 0.7f).SetDelay(duration * 0.3f);
            tween.Chain().TweenCallback(Callable.From(label.QueueFree));
        }

        public void ShowDamageNumber(Vector2 worldPos, int damage, bool isHeal = false)
        {
            var color = isHeal ? new Color(0.3f, 1f, 0.4f) : new Color(1f, 0.25f, 0.2f);
            var prefix = isHeal ? "+" : "-";
            ShowFloatingText(worldPos, prefix + damage.ToString(), color, 1.2f);
        }

        public void ShowBlockNumber(Vector2 worldPos, int block)
        {
            ShowFloatingText(worldPos, block.ToString(), new Color(0.4f, 0.6f, 1f), 1f);
        }
    }

    public interface IUIScreen
    {
        void OnShow();
        void OnHide();
    }
}
