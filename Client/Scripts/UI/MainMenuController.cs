using Godot;
using RoguelikeGame.Core;
using RoguelikeGame.Audio;

namespace RoguelikeGame.UI
{
    public partial class MainMenuController : SingletonBase<MainMenuController>, IUIScreenController
    {
        [Signal]
        public delegate void StartGamePressedEventHandler();

        [Signal]
        public delegate void SettingsPressedEventHandler();

        [Signal]
        public delegate void QuitPressedEventHandler();

        protected override void OnInitialize()
        {
            GD.Print("[MainMenuController] Initialized");
        }

        public void OnScreenReady(Control screen)
        {
            ConnectButtonSignals(screen);
            AudioManager.Instance?.PlayBGM("main_menu_theme");
            GD.Print("[MainMenuController] Main menu ready");
        }

        public void OnScreenHide()
        {
            GD.Print("[MainMenuController] Main menu hidden");
        }

        private void ConnectButtonSignals(Control screen)
        {
            var startBtn = screen.GetNodeOrNull<Button>("VBoxContainer/StartButton");
            var settingsBtn = screen.GetNodeOrNull<Button>("VBoxContainer/SettingsButton");
            var quitBtn = screen.GetNodeOrNull<Button>("VBoxContainer/QuitButton");

            if (startBtn != null)
                startBtn.Pressed += OnStartPressed;

            if (settingsBtn != null)
                settingsBtn.Pressed += OnSettingsPressed;

            if (quitBtn != null)
                quitBtn.Pressed += OnQuitPressed;
        }

        private void OnStartPressed()
        {
            AudioManager.Instance?.PlayButtonClick();
            EmitSignal(SignalName.StartGamePressed);
            GD.Print("[MainMenuController] Start game pressed");
        }

        private void OnSettingsPressed()
        {
            AudioManager.Instance?.PlayButtonClick();
            EmitSignal(SignalName.SettingsPressed);
            GD.Print("[MainMenuController] Settings pressed");
        }

        private void OnQuitPressed()
        {
            AudioManager.Instance?.PlayButtonClick();
            EmitSignal(SignalName.QuitPressed);
            GetTree().Quit();
            GD.Print("[MainMenuController] Quit pressed");
        }
    }
}
