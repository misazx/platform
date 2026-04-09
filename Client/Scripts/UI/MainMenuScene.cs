using Godot;
using RoguelikeGame.Core;
using RoguelikeGame.Audio;
using RoguelikeGame.UI.Panels;

namespace RoguelikeGame.UI
{
    public partial class MainMenuScene : Control
    {
        [Signal]
        public delegate void StartGameRequestedEventHandler();

        [Signal]
        public delegate void SettingsRequestedEventHandler();

        [Signal]
        public delegate void QuitRequestedEventHandler();

        private SaveSlotPanel _saveSlotPanel;
        private AchievementPanel _achievementPanel;

        public override void _Ready()
        {
            SetupButtons();
            GD.Print("[MainMenuScene] Ready");
        }

        private void SetupButtons()
        {
            var startBtn = GetNodeOrNull<Button>("VBoxContainer/StartButton");
            var settingsBtn = GetNodeOrNull<Button>("VBoxContainer/SettingsButton");
            var quitBtn = GetNodeOrNull<Button>("VBoxContainer/QuitButton");
            var achievementBtn = GetNodeOrNull<Button>("VBoxContainer/AchievementButton");
            var continueBtn = GetNodeOrNull<Button>("VBoxContainer/ContinueButton");

            if (startBtn != null)
            {
                startBtn.Pressed += OnStartPressed;
                GD.Print("[MainMenuScene] StartButton connected");
            }
            else
            {
                GD.PushWarning("[MainMenuScene] StartButton not found");
            }

            if (settingsBtn != null)
                settingsBtn.Pressed += OnSettingsPressed;

            if (quitBtn != null)
                quitBtn.Pressed += OnQuitPressed;

            if (achievementBtn != null)
            {
                achievementBtn.Pressed += OnAchievementPressed;
                GD.Print("[MainMenuScene] AchievementButton connected");
            }

            if (continueBtn != null)
            {
                continueBtn.Pressed += OnContinuePressed;
                GD.Print("[MainMenuScene] ContinueButton connected");
            }
        }

        private void OnStartPressed()
        {
            GD.Print("[MainMenuScene] Start button pressed, emitting signal");
            AudioManager.Instance?.PlayButtonClick();
            EmitSignal(SignalName.StartGameRequested);
        }

        private void OnSettingsPressed()
        {
            GD.Print("[MainMenuScene] Settings button pressed, emitting signal");
            AudioManager.Instance?.PlayButtonClick();
            EmitSignal(SignalName.SettingsRequested);
        }

        private void OnQuitPressed()
        {
            GD.Print("[MainMenuScene] Quit button pressed, emitting signal");
            AudioManager.Instance?.PlayButtonClick();
            EmitSignal(SignalName.QuitRequested);
        }

        private void OnAchievementPressed()
        {
            GD.Print("[MainMenuScene] Achievement button pressed");
            AudioManager.Instance?.PlayButtonClick();

            if (_achievementPanel == null)
            {
                _achievementPanel = new AchievementPanel();
                AddChild(_achievementPanel);
                _achievementPanel.Closed += () =>
                {
                    RemoveChild(_achievementPanel);
                    _achievementPanel.QueueFree();
                    _achievementPanel = null;
                };
            }
        }

        private void OnContinuePressed()
        {
            GD.Print("[MainMenuScene] Continue button pressed");
            AudioManager.Instance?.PlayButtonClick();

            if (_saveSlotPanel == null)
            {
                _saveSlotPanel = new SaveSlotPanel();
                AddChild(_saveSlotPanel);
                _saveSlotPanel.SaveSelected += (slotId) =>
                {
                    GD.Print($"[MainMenuScene] Save slot {slotId} selected");
                    OnSaveSlotSelected(slotId);
                };
                _saveSlotPanel.SaveDeleted += (slotId) =>
                {
                    GD.Print($"[MainMenuScene] Save slot {slotId} deleted");
                };
                _saveSlotPanel.Closed += () =>
                {
                    RemoveChild(_saveSlotPanel);
                    _saveSlotPanel.QueueFree();
                    _saveSlotPanel = null;
                };
            }
        }

        private void OnSaveSlotSelected(int slotId)
        {
            GD.Print($"[MainMenuScene] Loading save from slot {slotId}");
            var saveSlot = RoguelikeGame.Systems.EnhancedSaveSystem.Instance?.LoadGame(slotId);
            if (saveSlot != null)
            {
                GD.Print($"[MainMenuScene] Loaded save: {saveSlot.CharacterId} at floor {saveSlot.CurrentFloor}");
            }
            else
            {
                GD.Print($"[MainMenuScene] No save found at slot {slotId}, starting new game");
                EmitSignal(SignalName.StartGameRequested);
            }
        }
    }
}
