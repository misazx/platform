using Godot;
using System;
using RoguelikeGame.Core;
using RoguelikeGame.Systems;
using RoguelikeGame.Generation;

namespace RoguelikeGame.UI
{
    public partial class MainMenu : Control
    {
        private Button _newGameButton;
        private Button _continueButton;
        private Button _quitButton;
        private Label _titleLabel;

        [Export]
        public string GameTitle { get; set; } = "Roguelike Game";

        public override void _Ready()
        {
            SetupUI();
            ConnectSignals();
        }

        private void SetupUI()
        {
            SetAnchorsPreset(Control.LayoutPreset.FullRect);
            
            var centerContainer = new CenterContainer();
            centerContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
            AddChild(centerContainer);

            var vbox = new VBoxContainer();
            vbox.Alignment = BoxContainer.AlignmentMode.Center;
            vbox.AddThemeConstantOverride("separation", 20);
            centerContainer.AddChild(vbox);

            _titleLabel = new Label();
            _titleLabel.Text = GameTitle;
            _titleLabel.AddThemeFontSizeOverride("font_size", 48);
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            vbox.AddChild(_titleLabel);

            var spacer = new Control();
            spacer.CustomMinimumSize = new Vector2(0, 50);
            vbox.AddChild(spacer);

            _newGameButton = CreateButton("New Game");
            vbox.AddChild(_newGameButton);

            _continueButton = CreateButton("Continue");
            _continueButton.Disabled = true;
            vbox.AddChild(_continueButton);

            _quitButton = CreateButton("Quit");
            vbox.AddChild(_quitButton);
        }

        private Button CreateButton(string text)
        {
            var button = new Button();
            button.Text = text;
            button.CustomMinimumSize = new Vector2(200, 50);
            button.AddThemeFontSizeOverride("font_size", 20);
            return button;
        }

        private void ConnectSignals()
        {
            _newGameButton.Pressed += OnNewGamePressed;
            _continueButton.Pressed += OnContinuePressed;
            _quitButton.Pressed += OnQuitPressed;
        }

        private void OnNewGamePressed()
        {
            GD.Print("[MainMenu] Starting new game");
            Hide();
            GameInitializer.QuickStart();
        }

        private void OnContinuePressed()
        {
            GD.Print("[MainMenu] Continue not implemented yet");
        }

        private void OnQuitPressed()
        {
            GD.Print("[MainMenu] Quitting game");
            GetTree().Quit();
        }
    }
}
