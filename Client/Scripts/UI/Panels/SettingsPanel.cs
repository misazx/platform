using Godot;
using RoguelikeGame.Audio;

namespace RoguelikeGame.UI.Panels
{
    public partial class SettingsPanel : Control
    {
        private HSlider _musicSlider;
        private HSlider _sfxSlider;

        public override void _Ready()
        {
            _musicSlider = GetNode<HSlider>("Panel/VBox/MusicSlider");
            _sfxSlider = GetNode<HSlider>("Panel/VBox/SFXSlider");
            var closeBtn = GetNode<Button>("Panel/VBox/CloseButton");

            _musicSlider.ValueChanged += OnMusicVolumeChanged;
            _sfxSlider.ValueChanged += OnSfxVolumeChanged;
            closeBtn.Pressed += Hide;

            LoadSettings();
        }

        private void LoadSettings()
        {
            var musicVol = AudioManager.Instance?.MusicVolume ?? 0.8f;
            var sfxVol = AudioManager.Instance?.SfxVolume ?? 1.0f;

            if (_musicSlider != null)
                _musicSlider.Value = musicVol * 100;

            if (_sfxSlider != null)
                _sfxSlider.Value = sfxVol * 100;
        }

        private void OnMusicVolumeChanged(double value)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.MusicVolume = (float)(value / 100.0);
        }

        private void OnSfxVolumeChanged(double value)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.SfxVolume = (float)(value / 100.0);
        }
    }
}
