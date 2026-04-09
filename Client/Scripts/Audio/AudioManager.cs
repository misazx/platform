using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;

namespace RoguelikeGame.Audio
{
    public partial class AudioManager : SingletonBase<AudioManager>
    {
        private AudioStreamPlayer _bgmPlayer;
        private readonly List<AudioStreamPlayer> _sfxPlayers = new();
        private int _sfxIndex = 0;
        
        private const int SFX_PLAYER_COUNT = 8;
        private const float BGM_VOLUME = -12f;
        private const float SFX_VOLUME = 0f;

        private string _currentBGM = "";
        private readonly Dictionary<string, float> _bgmPositions = new();

        private const string SFX_PATH = "res://Audio/SFX/";
        private const string BGM_PATH = "res://Audio/BGM/";

        public float MusicVolume { get; set; } = 0.8f;
        public float SfxVolume { get; set; } = 1.0f;

        protected override void OnInitialize()
        {
            _bgmPlayer = new AudioStreamPlayer();
            _bgmPlayer.Bus = "BGM";
            _bgmPlayer.VolumeDb = BGM_VOLUME;
            AddChild(_bgmPlayer);

            for (int i = 0; i < SFX_PLAYER_COUNT; i++)
            {
                var sfx = new AudioStreamPlayer();
                sfx.Bus = "SFX";
                sfx.VolumeDb = SFX_VOLUME;
                AddChild(sfx);
                _sfxPlayers.Add(sfx);
            }

            GD.Print("[AudioManager] Initialized");
        }

        public void PlayBGM(string bgmName, bool loop = true, float fadeTime = 0.5f)
        {
            if (_currentBGM == bgmName && _bgmPlayer.Playing)
                return;

            if (!string.IsNullOrEmpty(_currentBGM))
            {
                _bgmPositions[_currentBGM] = _bgmPlayer.GetPlaybackPosition();
                TweenOut(_bgmPlayer, fadeTime);
            }

            var stream = LoadAudio(BGM_PATH + bgmName);
            if (stream != null)
            {
                _bgmPlayer.Stream = stream;
                _bgmPlayer.Play(_bgmPositions.GetValueOrDefault(bgmName, 0f));
                _currentBGM = bgmName;
                TweenIn(_bgmPlayer, fadeTime);
            }
            else
            {
                GD.Print($"[AudioManager] BGM not found: {bgmName}");
            }
        }

        public void PauseBGM(float fadeTime = 0.3f)
        {
            if (!string.IsNullOrEmpty(_currentBGM))
            {
                _bgmPositions[_currentBGM] = _bgmPlayer.GetPlaybackPosition();
                TweenOut(_bgmPlayer, fadeTime, () => _bgmPlayer.Stop());
            }
        }

        public void ResumeBGM(float fadeTime = 0.3f)
        {
            if (!string.IsNullOrEmpty(_currentBGM))
            {
                _bgmPlayer.Play(_bgmPositions[_currentBGM]);
                TweenIn(_bgmPlayer, fadeTime);
            }
        }

        public void StopBGM(float fadeTime = 0.5f)
        {
            TweenOut(_bgmPlayer, fadeTime, () =>
            {
                _bgmPlayer.Stop();
                _currentBGM = "";
                _bgmPositions.Clear();
            });
        }

        public void PlaySFX(string sfxName, float pitchScale = 1f)
        {
            if (_sfxPlayers.Count == 0)
            {
                GD.PrintErr("[AudioManager] SFX players not initialized!");
                return;
            }

            var stream = LoadAudio(SFX_PATH + sfxName);
            if (stream == null)
            {
                GD.Print($"[AudioManager] SFX not found: {sfxName}");
                return;
            }

            var player = _sfxPlayers[_sfxIndex];
            player.PitchScale = pitchScale;
            player.Stream = stream;
            player.Play();

            _sfxIndex = (_sfxIndex + 1) % _sfxPlayers.Count;
        }

        private AudioStream LoadAudio(string path)
        {
            var extensions = new[] { ".wav", ".ogg", ".mp3" };
            
            foreach (var ext in extensions)
            {
                var fullPath = path + ext;
                if (ResourceLoader.Exists(fullPath))
                {
                    return ResourceLoader.Load<AudioStream>(fullPath);
                }
            }

            if (ResourceLoader.Exists(path))
            {
                return ResourceLoader.Load<AudioStream>(path);
            }

            return null;
        }

        public void PlayCardPlay() => PlaySFX("card_play");
        public void PlayCardDraw() => PlaySFX("card_draw", 1.1f);
        public void PlayAttack() => PlaySFX("attack", 0.95f);
        public void PlayBlock() => PlaySFX("block");
        public void PlayDamage() => PlaySFX("damage", 0.85f);
        public void PlayEnemyHit() => PlaySFX("enemy_hit", 0.9f);
        public void PlayEnemyDeath() => PlaySFX("enemy_death", 0.8f);
        public void PlayPotionUse() => PlaySFX("potion_use");
        public void PlayRelicActivate() => PlaySFX("relic_activate");
        public void PlayGoldPickup() => PlaySFX("gold_pickup");
        public void PlayButtonClick() => PlaySFX("button_click", 1.05f);
        public void PlayShopBuy() => PlaySFX("shop_buy");

        public void SetBGMVolume(float db)
        {
            var tween = CreateTween();
            tween.TweenProperty(_bgmPlayer, "volume_db", db, 0.3f);
        }

        public void SetSFXVolume(float db)
        {
            foreach (var p in _sfxPlayers)
                p.VolumeDb = db;
        }

        private void TweenIn(AudioStreamPlayer player, float duration, Action onComplete = null)
        {
            var tween = CreateTween();
            tween.SetEase(Tween.EaseType.Out).TweenProperty(player, "volume_db", BGM_VOLUME, duration);
            if (onComplete != null)
                tween.TweenCallback(Callable.From(onComplete));
        }

        private void TweenOut(AudioStreamPlayer player, float duration, Action onComplete = null)
        {
            var tween = CreateTween();
            tween.SetEase(Tween.EaseType.In).TweenProperty(player, "volume_db", -80f, duration);
            if (onComplete != null)
                tween.TweenCallback(Callable.From(onComplete));
        }
    }
}
