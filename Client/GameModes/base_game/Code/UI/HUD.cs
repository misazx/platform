using Godot;
using System;
using RoguelikeGame.Core;
using RoguelikeGame.Systems;
using RoguelikeGame.Generation;

namespace RoguelikeGame.UI
{
    public partial class HUD : CanvasLayer
    {
        private Label _floorLabel;
        private Label _roomLabel;
        private Label _waveLabel;
        private Label _enemiesLabel;
        private ProgressBar _healthBar;

        public override void _Ready()
        {
            SetupUI();
            ConnectSignals();
        }

        private void SetupUI()
        {
            var marginContainer = new MarginContainer();
            marginContainer.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
            marginContainer.AddThemeConstantOverride("margin_left", 20);
            marginContainer.AddThemeConstantOverride("margin_top", 20);
            AddChild(marginContainer);

            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 10);
            marginContainer.AddChild(vbox);

            _floorLabel = new Label();
            _floorLabel.Text = "Floor: 1";
            _floorLabel.AddThemeFontSizeOverride("font_size", 20);
            vbox.AddChild(_floorLabel);

            _roomLabel = new Label();
            _roomLabel.Text = "Room: 0/10";
            _roomLabel.AddThemeFontSizeOverride("font_size", 18);
            vbox.AddChild(_roomLabel);

            _waveLabel = new Label();
            _waveLabel.Text = "Wave: 0";
            _waveLabel.AddThemeFontSizeOverride("font_size", 18);
            vbox.AddChild(_waveLabel);

            _enemiesLabel = new Label();
            _enemiesLabel.Text = "Enemies: 0";
            _enemiesLabel.AddThemeFontSizeOverride("font_size", 16);
            vbox.AddChild(_enemiesLabel);

            _healthBar = new ProgressBar();
            _healthBar.MinValue = 0;
            _healthBar.MaxValue = 100;
            _healthBar.Value = 100;
            _healthBar.CustomMinimumSize = new Vector2(200, 20);
            vbox.AddChild(_healthBar);
        }

        private void ConnectSignals()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.FloorChanged += OnFloorChanged;
            }

            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.WaveStarted += OnWaveStarted;
                WaveManager.Instance.WaveCompleted += OnWaveCompleted;
            }

            EventBus.Instance.Subscribe<int>(GameEvents.PlayerHealthChanged, OnPlayerHealthChanged);
        }

        private void OnFloorChanged(int floor)
        {
            _floorLabel.Text = $"Floor: {floor}";
        }

        private void OnRoomChanged(int room)
        {
            var total = GameManager.Instance?.CurrentRun?.CurrentFloor ?? 10;
            _roomLabel.Text = $"Room: {room}/{total}";
        }

        private void OnWaveStarted(int wave)
        {
            _waveLabel.Text = $"Wave: {wave}";
        }

        private void OnWaveCompleted(int wave)
        {
            _waveLabel.Text = $"Wave {wave} Complete!";
        }

        private void OnPlayerHealthChanged(int health)
        {
            var maxHealth = 100;
            _healthBar.MaxValue = maxHealth;
            _healthBar.Value = health;
        }

        public void UpdateEnemyCount(int count)
        {
            _enemiesLabel.Text = $"Enemies: {count}";
        }
    }
}
