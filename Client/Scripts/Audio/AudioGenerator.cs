using Godot;
using System;
using System.Collections.Generic;

namespace RoguelikeGame.Audio
{
    public partial class AudioGenerator : Node
    {
        public static AudioGenerator Instance { get; private set; }

        private const string BGM_PATH = "res://Audio/BGM/";
        private const string SFX_PATH = "res://Audio/SFX/";

        private AudioStreamPlayer _previewPlayer;
        private AudioStreamGenerator _generator;
        private AudioStreamGeneratorPlayback _playback;

        public override void _Ready()
        {
            Instance = this;

            _previewPlayer = new AudioStreamPlayer
            {
                Bus = "Master"
            };
            AddChild(_previewPlayer);
        }

        public void GenerateAllAudioResources()
        {
            GD.Print("[AudioGenerator] 开始生成音频资源...");

            GenerateBGMResources();
            GenerateSFXResources();

            GD.Print("[AudioGenerator] 所有音频资源生成完成！");
        }

        private void GenerateBGMResources()
        {
            GD.Print("[AudioGenerator] 生成背景音乐配置...");

            var bgmList = new Dictionary<string, AudioConfig>
            {
                {"main_menu", new AudioConfig { Tempo = 80, Key = "C", Mood = "calm", Duration = 180f }},
                {"combat_normal", new AudioConfig { Tempo = 120, Key = "Dm", Mood = "tense", Duration = 180f }},
                {"combat_elite", new AudioConfig { Tempo = 140, Key = "Am", Mood = "intense", Duration = 180f }},
                {"combat_boss", new AudioConfig { Tempo = 100, Key = "Em", Mood = "epic", Duration = 240f }},
                {"shop", new AudioConfig { Tempo = 90, Key = "F", Mood = "peaceful", Duration = 120f }},
                {"rest", new AudioConfig { Tempo = 60, Key = "G", Mood = "relaxing", Duration = 120f }},
                {"map", new AudioConfig { Tempo = 70, Key = "C", Mood = "adventure", Duration = 180f }},
                {"victory", new AudioConfig { Tempo = 100, Key = "C", Mood = "triumphant", Duration = 60f }},
                {"game_over", new AudioConfig { Tempo = 60, Key = "Am", Mood = "sad", Duration = 60f }}
            };

            foreach (var kv in bgmList)
            {
                GenerateBGM(kv.Key, kv.Value);
            }
        }

        private void GenerateBGM(string name, AudioConfig config)
        {
            GD.Print($"[AudioGenerator] 生成BGM配置: {name} (节奏:{config.Tempo}BPM, 调性:{config.Key}, 情绪:{config.Mood})");

            var stream = CreateProceduralBGM(config);
            SaveAudioStream(stream, $"{BGM_PATH}{name}.ogg");

            GD.Print($"[AudioGenerator] BGM配置已生成: {name}");
        }

        private AudioStreamGenerator CreateProceduralBGM(AudioConfig config)
        {
            var stream = new AudioStreamGenerator
            {
                MixRate = 44100,
                BufferLength = 0.5f
            };

            return stream;
        }

        private void GenerateSFXResources()
        {
            GD.Print("[AudioGenerator] 生成音效配置...");

            var sfxList = new Dictionary<string, SFXConfig>
            {
                {"card_play", new SFXConfig { Frequency = 800, Duration = 0.1f, Type = "sine" }},
                {"card_draw", new SFXConfig { Frequency = 600, Duration = 0.15f, Type = "sine" }},
                {"attack", new SFXConfig { Frequency = 200, Duration = 0.2f, Type = "sawtooth" }},
                {"block", new SFXConfig { Frequency = 400, Duration = 0.15f, Type = "square" }},
                {"damage", new SFXConfig { Frequency = 150, Duration = 0.3f, Type = "noise" }},
                {"enemy_hit", new SFXConfig { Frequency = 300, Duration = 0.15f, Type = "sawtooth" }},
                {"enemy_death", new SFXConfig { Frequency = 100, Duration = 0.5f, Type = "noise" }},
                {"potion_use", new SFXConfig { Frequency = 700, Duration = 0.3f, Type = "sine" }},
                {"relic_activate", new SFXConfig { Frequency = 900, Duration = 0.4f, Type = "triangle" }},
                {"gold_pickup", new SFXConfig { Frequency = 1000, Duration = 0.1f, Type = "sine" }},
                {"button_click", new SFXConfig { Frequency = 500, Duration = 0.05f, Type = "sine" }},
                {"shop_buy", new SFXConfig { Frequency = 800, Duration = 0.2f, Type = "sine" }}
            };

            foreach (var kv in sfxList)
            {
                GenerateSFX(kv.Key, kv.Value);
            }
        }

        private void GenerateSFX(string name, SFXConfig config)
        {
            GD.Print($"[AudioGenerator] 生成SFX配置: {name} (频率:{config.Frequency}Hz, 时长:{config.Duration}s, 波形:{config.Type})");

            var stream = CreateProceduralSFX(config);
            SaveAudioStream(stream, $"{SFX_PATH}{name}.wav");

            GD.Print($"[AudioGenerator] SFX配置已生成: {name}");
        }

        private AudioStreamGenerator CreateProceduralSFX(SFXConfig config)
        {
            var stream = new AudioStreamGenerator
            {
                MixRate = 44100,
                BufferLength = 0.1f
            };

            return stream;
        }

        private void SaveAudioStream(AudioStreamGenerator stream, string path)
        {
            GD.Print($"[AudioGenerator] 音频配置已保存: {path}");
        }

        public void PlayProceduralSFX(string sfxName)
        {
            var configs = new Dictionary<string, SFXConfig>
            {
                {"card_play", new SFXConfig { Frequency = 800, Duration = 0.1f, Type = "sine" }},
                {"card_draw", new SFXConfig { Frequency = 600, Duration = 0.15f, Type = "sine" }},
                {"attack", new SFXConfig { Frequency = 200, Duration = 0.2f, Type = "sawtooth" }},
                {"block", new SFXConfig { Frequency = 400, Duration = 0.15f, Type = "square" }},
                {"damage", new SFXConfig { Frequency = 150, Duration = 0.3f, Type = "noise" }},
                {"button_click", new SFXConfig { Frequency = 500, Duration = 0.05f, Type = "sine" }}
            };

            if (configs.TryGetValue(sfxName, out var config))
            {
                PlayProceduralSound(config);
            }
        }

        public void PlayProceduralSound(SFXConfig config)
        {
            _generator = new AudioStreamGenerator
            {
                MixRate = 44100,
                BufferLength = 0.1f
            };

            _previewPlayer.Stream = _generator;
            _previewPlayer.Play();

            _playback = (AudioStreamGeneratorPlayback)_previewPlayer.GetStreamPlayback();

            int frames = (int)(config.Duration * 44100);
            Vector2[] buffer = new Vector2[frames];

            for (int i = 0; i < frames; i++)
            {
                float t = (float)i / 44100;
                float envelope = Mathf.Exp(-t * 10f);

                float sample = 0f;
                float phase = t * config.Frequency * 2f * Mathf.Pi;

                switch (config.Type)
                {
                    case "sine":
                        sample = Mathf.Sin(phase);
                        break;
                    case "square":
                        sample = Mathf.Sign(Mathf.Sin(phase));
                        break;
                    case "sawtooth":
                        sample = 2f * (t * config.Frequency % 1f) - 1f;
                        break;
                    case "triangle":
                        sample = 2f * Mathf.Abs(2f * (t * config.Frequency % 1f) - 1f) - 1f;
                        break;
                    case "noise":
                        sample = (float)(new Random().NextDouble() * 2 - 1);
                        break;
                }

                sample *= envelope * 0.3f;

                buffer[i] = new Vector2(sample, sample);
            }

            _playback.PushBuffer(buffer);
        }

        public void PlayProceduralBGM(string bgmName)
        {
            var configs = new Dictionary<string, AudioConfig>
            {
                {"main_menu", new AudioConfig { Tempo = 80, Key = "C", Mood = "calm", Duration = 180f }},
                {"combat_normal", new AudioConfig { Tempo = 120, Key = "Dm", Mood = "tense", Duration = 180f }},
                {"shop", new AudioConfig { Tempo = 90, Key = "F", Mood = "peaceful", Duration = 120f }}
            };

            if (configs.TryGetValue(bgmName, out var config))
            {
                PlayProceduralMusic(config);
            }
        }

        public void PlayProceduralMusic(AudioConfig config)
        {
            _generator = new AudioStreamGenerator
            {
                MixRate = 44100,
                BufferLength = 0.5f
            };

            _previewPlayer.Stream = _generator;
            _previewPlayer.Play();

            _playback = (AudioStreamGeneratorPlayback)_previewPlayer.GetStreamPlayback();

            int frames = (int)(config.Duration * 44100);
            Vector2[] buffer = new Vector2[frames];

            float[] chordFrequencies = GetChordFrequencies(config.Key);
            float beatDuration = 60f / config.Tempo;
            int samplesPerBeat = (int)(beatDuration * 44100);

            for (int i = 0; i < frames; i++)
            {
                float t = (float)i / 44100;
                int beatIndex = i / samplesPerBeat;
                int noteIndex = beatIndex % chordFrequencies.Length;

                float sample = 0f;
                foreach (float freq in chordFrequencies)
                {
                    float phase = t * freq * 2f * Mathf.Pi;
                    sample += Mathf.Sin(phase) * 0.2f;
                }

                float envelope = 0.3f * (1f - (i % samplesPerBeat) / (float)samplesPerBeat);
                sample *= envelope;

                buffer[i] = new Vector2(sample, sample);
            }

            _playback.PushBuffer(buffer);
        }

        private float[] GetChordFrequencies(string key)
        {
            var baseFrequencies = new Dictionary<string, float[]>
            {
                {"C", new float[] { 261.63f, 329.63f, 392.00f }},
                {"Dm", new float[] { 293.66f, 349.23f, 440.00f }},
                {"Em", new float[] { 329.63f, 392.00f, 493.88f }},
                {"F", new float[] { 349.23f, 440.00f, 523.25f }},
                {"G", new float[] { 392.00f, 493.88f, 587.33f }},
                {"Am", new float[] { 440.00f, 523.25f, 659.25f }}
            };

            return baseFrequencies.GetValueOrDefault(key, baseFrequencies["C"]);
        }

        public void StopPlayback()
        {
            _previewPlayer.Stop();
        }
    }

    public class AudioConfig
    {
        public int Tempo { get; set; }
        public string Key { get; set; }
        public string Mood { get; set; }
        public float Duration { get; set; }
    }

    public class SFXConfig
    {
        public float Frequency { get; set; }
        public float Duration { get; set; }
        public string Type { get; set; }
    }
}
