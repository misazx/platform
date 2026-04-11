using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using RoguelikeGame.Core;

namespace RoguelikeGame.Systems
{
    public class PlayerData
    {
        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public float Speed { get; set; }
        public Vector2 Position { get; set; }
        public Dictionary<string, int> Inventory { get; set; } = new();
        public List<string> LearnedSkills { get; set; } = new();
        public Dictionary<string, int> SkillLevels { get; set; } = new();
    }

    public class GameData
    {
        public int SaveVersion { get; set; } = 1;
        public DateTime SaveTime { get; set; }
        public uint Seed { get; set; }
        public int CurrentFloor { get; set; }
        public int CurrentRoom { get; set; }
        public int TotalRooms { get; set; }
        public PlayerData Player { get; set; } = new();
        public Dictionary<string, object> CustomData { get; set; } = new();
    }

    public partial class SaveSystem : SingletonBase<SaveSystem>
    {
        private readonly string _savePath = "user://saves/";
        private readonly int _maxSaveSlots = 3;

        [Export]
        public bool AutoSave { get; set; } = true;

        [Export]
        public float AutoSaveInterval { get; set; } = 300f;

        private float _autoSaveTimer = 0f;
        private GameData _currentGameData;

        [Signal]
        public delegate void GameSavedEventHandler(int slot);

        [Signal]
        public delegate void GameLoadedEventHandler(int slot);

        protected override void OnInitialize()
        {
            EnsureSaveDirectory();
        }

        private void EnsureSaveDirectory()
        {
            var dir = DirAccess.Open("user://");
            if (dir != null && !dir.DirExists("saves"))
            {
                dir.MakeDir("saves");
                GD.Print("[SaveSystem] Created saves directory");
            }
        }

        public override void _Process(double delta)
        {
            if (AutoSave && GameManager.Instance?.CurrentPhase == GamePhase.Combat)
            {
                _autoSaveTimer += (float)delta;
                if (_autoSaveTimer >= AutoSaveInterval)
                {
                    _autoSaveTimer = 0f;
                    QuickSave();
                }
            }
        }

        public bool SaveGame(int slot, GameData data)
        {
            if (slot < 0 || slot >= _maxSaveSlots)
            {
                GD.PrintErr($"[SaveSystem] Invalid save slot: {slot}");
                return false;
            }

            try
            {
                data.SaveTime = DateTime.Now;
                data.SaveVersion = 1;

                var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                var filePath = GetSaveFilePath(slot);
                var file = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Write);
                
                if (file == null)
                {
                    GD.PrintErr($"[SaveSystem] Failed to open file: {filePath}");
                    return false;
                }

                file.StoreString(json);
                file.Close();

                _currentGameData = data;

                EmitSignal(SignalName.GameSaved, slot);
                GD.Print($"[SaveSystem] Game saved to slot {slot}");

                return true;
            }
            catch (Exception e)
            {
                GD.PrintErr($"[SaveSystem] Save failed: {e.Message}");
                return false;
            }
        }

        public GameData LoadGame(int slot)
        {
            if (slot < 0 || slot >= _maxSaveSlots)
            {
                GD.PrintErr($"[SaveSystem] Invalid save slot: {slot}");
                return null;
            }

            try
            {
                var filePath = GetSaveFilePath(slot);
                
                if (!Godot.FileAccess.FileExists(filePath))
                {
                    GD.PrintErr($"[SaveSystem] Save file not found: {filePath}");
                    return null;
                }

                var file = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    GD.PrintErr($"[SaveSystem] Failed to open file: {filePath}");
                    return null;
                }

                var json = file.GetAsText();
                file.Close();

                var data = JsonSerializer.Deserialize<GameData>(json);
                _currentGameData = data;

                EmitSignal(SignalName.GameLoaded, slot);
                GD.Print($"[SaveSystem] Game loaded from slot {slot}");

                return data;
            }
            catch (Exception e)
            {
                GD.PrintErr($"[SaveSystem] Load failed: {e.Message}");
                return null;
            }
        }

        public bool DeleteSave(int slot)
        {
            if (slot < 0 || slot >= _maxSaveSlots)
            {
                GD.PrintErr($"[SaveSystem] Invalid save slot: {slot}");
                return false;
            }

            try
            {
                var filePath = GetSaveFilePath(slot);
                
                if (Godot.FileAccess.FileExists(filePath))
                {
                    DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(filePath));
                    GD.Print($"[SaveSystem] Deleted save slot {slot}");
                    return true;
                }

                return false;
            }
            catch (Exception e)
            {
                GD.PrintErr($"[SaveSystem] Delete failed: {e.Message}");
                return false;
            }
        }

        public bool HasSave(int slot)
        {
            if (slot < 0 || slot >= _maxSaveSlots)
                return false;

            var filePath = GetSaveFilePath(slot);
            return Godot.FileAccess.FileExists(filePath);
        }

        public DateTime? GetSaveTime(int slot)
        {
            var data = LoadGame(slot);
            return data?.SaveTime;
        }

        public GameData CreateSaveData()
        {
            var data = new GameData
            {
                Seed = GameManager.Instance?.CurrentRun?.Seed ?? 0u,
                CurrentFloor = GameManager.Instance?.CurrentRun?.CurrentFloor ?? 1,
                CurrentRoom = GameManager.Instance?.CurrentRun?.CurrentRoom ?? 0,
                TotalRooms = 10
            };

            var player = GetTree().GetFirstNodeInGroup("player") as Node;
            if (player != null)
            {
                data.Player = new PlayerData
                {
                    MaxHealth = (int)player.Get("MaxHealth"),
                    CurrentHealth = (int)player.Get("CurrentHealth"),
                    Attack = (int)player.Get("Attack"),
                    Defense = (int)player.Get("Defense"),
                    Speed = (float)player.Get("Speed"),
                    Position = (Vector2)player.Get("Position")
                };

                if (player.HasMethod("GetInventory"))
                {
                    var invVariant = player.Call("GetInventory");
                    if (invVariant.VariantType == Variant.Type.Dictionary && invVariant.AsGodotDictionary() != null)
                    {
                        var dict = new Dictionary<string, int>();
                        foreach (var key in invVariant.AsGodotDictionary().Keys)
                            dict[key.ToString()] = invVariant.AsGodotDictionary()[key].AsInt32();
                        data.Player.Inventory = dict;
                    }
                }

                var skills = SkillManager.Instance?.GetLearnedSkills();
                if (skills != null)
                {
                    foreach (var skill in skills)
                    {
                        data.Player.LearnedSkills.Add(skill.SkillId);
                        data.Player.SkillLevels[skill.SkillId] = skill.Level;
                    }
                }
            }

            return data;
        }

        public void ApplySaveData(GameData data)
        {
            if (data == null)
                return;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.Set("CurrentSeed", data.Seed);
                GameManager.Instance.Set("CurrentFloor", data.CurrentFloor);
                GameManager.Instance.Set("CurrentRoom", data.CurrentRoom);
            }

            var player = GetTree().GetFirstNodeInGroup("player") as Node;
            if (player != null && data.Player != null)
            {
                player.Set("MaxHealth", data.Player.MaxHealth);
                player.Set("CurrentHealth", data.Player.CurrentHealth);
                player.Set("Attack", data.Player.Attack);
                player.Set("Defense", data.Player.Defense);
                player.Set("Speed", data.Player.Speed);
                player.Set("Position", data.Player.Position);

                if (player.HasMethod("SetInventory"))
                {
                    var invDict = new Godot.Collections.Dictionary();
                    foreach (var kvp in data.Player.Inventory)
                        invDict[kvp.Key] = kvp.Value;
                    player.Call("SetInventory", invDict);
                }

                foreach (var skillId in data.Player.LearnedSkills)
                {
                    SkillManager.Instance?.LearnSkill(skillId, player);
                    
                    if (data.Player.SkillLevels.TryGetValue(skillId, out var level))
                    {
                        for (int i = 1; i < level; i++)
                        {
                            SkillManager.Instance?.UpgradeSkill(skillId, player);
                        }
                    }
                }
            }

            GD.Print("[SaveSystem] Save data applied");
        }

        public void QuickSave()
        {
            var data = CreateSaveData();
            SaveGame(0, data);
        }

        public void QuickLoad()
        {
            var data = LoadGame(0);
            if (data != null)
            {
                ApplySaveData(data);
            }
        }

        private string GetSaveFilePath(int slot)
        {
            return $"{_savePath}save_{slot}.json";
        }

        public int GetMaxSaveSlots()
        {
            return _maxSaveSlots;
        }

        public List<SaveSlotInfo> GetAllSaveSlots()
        {
            var slots = new List<SaveSlotInfo>();

            for (int i = 0; i < _maxSaveSlots; i++)
            {
                var info = new SaveSlotInfo
                {
                    Slot = i,
                    Exists = HasSave(i)
                };

                if (info.Exists)
                {
                    info.SaveTime = GetSaveTime(i);
                    var data = LoadGame(i);
                    if (data != null)
                    {
                        info.Floor = data.CurrentFloor;
                        info.Seed = data.Seed;
                    }
                }

                slots.Add(info);
            }

            return slots;
        }
    }

    public class SaveSlotInfo
    {
        public int Slot { get; set; }
        public bool Exists { get; set; }
        public DateTime? SaveTime { get; set; }
        public int Floor { get; set; }
        public uint Seed { get; set; }
    }
}
