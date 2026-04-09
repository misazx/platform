using Godot;
using System.Collections.Generic;

namespace RoguelikeGame.Core
{
    public partial class ResourceManager : SingletonBase<ResourceManager>
    {
        private readonly Dictionary<string, Texture2D> _textures = new();
        private readonly Dictionary<string, AudioStream> _audioStreams = new();
        private readonly Dictionary<string, PackedScene> _scenes = new();

        public Texture2D LoadTexture(string path)
        {
            if (_textures.TryGetValue(path, out var cached))
                return cached;
            
            if (!ResourceLoader.Exists(path))
            {
                GD.Print($"[ResourceManager] Texture not found: {path}");
                return null;
            }
            
            var tex = ResourceLoader.Load<Texture2D>(path);
            if (tex != null)
                _textures[path] = tex;
            return tex;
        }

        public Texture2D GetCardTexture(string cardId)
        {
            return LoadTexture($"res://Assets/Cards/{cardId}.png");
        }

        public Texture2D GetCharacterPortrait(string characterId)
        {
            return LoadTexture($"res://Assets/Characters/{characterId}_portrait.png");
        }

        public Texture2D GetIcon(string iconName)
        {
            return LoadTexture($"res://Assets/Icons/{iconName}.png");
        }

        public Texture2D GetRelicIcon(string relicId)
        {
            return LoadTexture($"res://Assets/Relics/{relicId}.png");
        }

        public AudioStream LoadAudio(string path)
        {
            if (_audioStreams.TryGetValue(path, out var cached))
                return cached;
            
            if (!ResourceLoader.Exists(path))
            {
                GD.Print($"[ResourceManager] Audio not found: {path}");
                return null;
            }
            
            var audio = ResourceLoader.Load<AudioStream>(path);
            if (audio != null)
                _audioStreams[path] = audio;
            return audio;
        }

        public AudioStream GetBGM(string name) => LoadAudio($"res://Audio/BGM/{name}.ogg");
        public AudioStream GetSFX(string name) => LoadAudio($"res://Audio/SFX/{name}.wav");

        public PackedScene LoadScene(string path)
        {
            if (_scenes.TryGetValue(path, out var cached))
                return cached;
            
            if (!ResourceLoader.Exists(path))
            {
                GD.Print($"[ResourceManager] Scene not found: {path}");
                return null;
            }
            
            var scene = ResourceLoader.Load<PackedScene>(path);
            if (scene != null)
                _scenes[path] = scene;
            return scene;
        }
    }
}
