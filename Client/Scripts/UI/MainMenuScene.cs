using Godot;
using RoguelikeGame.Core;
using RoguelikeGame.Audio;
using RoguelikeGame.UI.Panels;
using RoguelikeGame.Packages;
using System;

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

		[Signal]
		public delegate void PackageStoreRequestedEventHandler();

		private SaveSlotPanel _saveSlotPanel;
		private AchievementPanel _achievementPanel;
		private Button _packageStoreBtn;

		public override void _Ready()
		{
			SetupButtons();
			GD.Print("[MainMenuScene] Ready");
		}

		private void SetupButtons()
		{
			var vbox = GetNodeOrNull<VBoxContainer>("VBoxContainer");
			if (vbox == null)
			{
				GD.PushError("[MainMenuScene] VBoxContainer not found!");
				return;
			}

			var startBtn = vbox.GetNodeOrNull<Button>("StartButton");
			var settingsBtn = vbox.GetNodeOrNull<Button>("SettingsButton");
			var quitBtn = vbox.GetNodeOrNull<Button>("QuitButton");
			var achievementBtn = vbox.GetNodeOrNull<Button>("AchievementButton");
			var continueBtn = vbox.GetNodeOrNull<Button>("ContinueButton");

			if (startBtn != null)
			{
				startBtn.Pressed += OnStartPressed;
				GD.Print("[MainMenuScene] StartButton connected");
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

			AddPackageStoreButton(vbox, quitBtn);
		}

		private void AddPackageStoreButton(VBoxContainer vbox, Button quitBtn)
		{
			try
			{
				_packageStoreBtn = new Button
				{
					Text = "📦 游戏包商店",
					CustomMinimumSize = new Vector2(240, 52),
					Modulate = new Color(0.9f, 0.85f, 1f)
				};
				_packageStoreBtn.AddThemeFontSizeOverride("font_size", 18);
				_packageStoreBtn.Pressed += OnPackageStorePressed;

				if (quitBtn != null)
				{
					int quitIndex = quitBtn.GetIndex();
					vbox.AddChild(_packageStoreBtn);
					vbox.MoveChild(_packageStoreBtn, quitIndex);
				}
				else
				{
					vbox.AddChild(_packageStoreBtn);
				}

				GD.Print("[MainMenuScene] ✅ Package Store button added");
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[MainMenuScene] Failed to add package button: {ex.Message}");
			}
		}

		private void OnPackageStorePressed()
		{
			GD.Print("[MainMenuScene] Package Store button pressed");
			AudioManager.Instance?.PlayButtonClick();
			EmitSignal(SignalName.PackageStoreRequested);
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
