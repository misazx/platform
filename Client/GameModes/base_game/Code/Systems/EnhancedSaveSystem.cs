using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using RoguelikeGame.Core;
using RoguelikeGame.Database;

namespace RoguelikeGame.Systems
{
    public class SaveSlot
    {
        public int SlotId { get; set; }
        public DateTime SaveTime { get; set; }
        public string CharacterId { get; set; }
        public int CurrentFloor { get; set; }
        public int CurrentHealth { get; set; }
        public int MaxHealth { get; set; }
        public int Gold { get; set; }
        public int TotalKills { get; set; }
        public int TotalDamageDealt { get; set; }
        public List<string> DeckIds { get; set; } = new();
        public List<string> RelicIds { get; set; } = new();
        public List<string> PotionIds { get; set; } = new();
        public Dictionary<string, object> CustomData { get; set; } = new();
        public uint Seed { get; set; }
        public double PlayTimeSeconds { get; set; }
        public string ScreenshotPath { get; set; }
    }

    public partial class EnhancedSaveSystem : SingletonBase<EnhancedSaveSystem>
    {
        private const int MAX_SLOTS = 3;
        private const string SAVE_DIR = "user://saves/";
        private const string SCREENSHOT_DIR = "user://screenshots/";

        protected override void OnInitialize()
        {
            EnsureDirectoriesExist();
        }

        private void EnsureDirectoriesExist()
        {
            var dir = DirAccess.Open("user://");
            if (dir != null)
            {
                dir.MakeDirRecursive("saves");
                dir.MakeDirRecursive("screenshots");
            }
        }

        public bool HasSave(int slotId) => FileAccess.FileExists(GetSavePath(slotId));

        public SaveSlot SaveGame(int slotId, RunData runData)
        {
            var slot = new SaveSlot
            {
                SlotId = slotId,
                SaveTime = DateTime.Now,
                CharacterId = runData.CharacterId,
                CurrentFloor = runData.CurrentFloor,
                CurrentHealth = runData.CurrentHealth,
                MaxHealth = runData.MaxHealth,
                Gold = runData.Gold,
                TotalKills = runData.TotalEnemiesDefeated,
                TotalDamageDealt = runData.TotalDamageDealt,
                Seed = runData.Seed,
                PlayTimeSeconds = (DateTime.Now - runData.StartTime).TotalSeconds,
                DeckIds = ExtractCardIds(runData.Deck),
                RelicIds = new List<string>(runData.Relics),
                PotionIds = new List<string>(runData.Potions),
                CustomData = new Dictionary<string, object>(runData.CustomData)
            };

            var json = JsonSerializer.Serialize(slot, new JsonSerializerOptions { WriteIndented = true });
            using (var file = FileAccess.Open(GetSavePath(slotId), FileAccess.ModeFlags.Write))
            {
                file.StoreString(json);
            }

            TakeScreenshot(slotId);
            slot.ScreenshotPath = GetScreenshotPath(slotId);
            
            GD.Print($"[Save] Game saved to slot {slotId} at {slot.SaveTime}");
            return slot;
        }

        public SaveSlot LoadGame(int slotId)
        {
            var path = GetSavePath(slotId);
            if (!FileAccess.FileExists(path))
            {
                GD.PrintErr($"[Save] No save found at slot {slotId}");
                return null;
            }

            using (var file = FileAccess.Open(path, FileAccess.ModeFlags.Read))
            {
                var json = file.GetAsText();
                return JsonSerializer.Deserialize<SaveSlot>(json);
            }
        }

        public void DeleteSave(int slotId)
        {
            var path = GetSavePath(slotId);
            if (FileAccess.FileExists(path))
                DirAccess.RemoveAbsolute(path);
            
            var screenshotPath = GetScreenshotPath(slotId);
            if (FileAccess.FileExists(screenshotPath))
                DirAccess.RemoveAbsolute(screenshotPath);
        }

        public List<SaveSlot> GetAllSaves()
        {
            var saves = new List<SaveSlot>();
            for (int i = 0; i < MAX_SLOTS; i++)
            {
                if (HasSave(i))
                {
                    var slot = LoadGame(i);
                    if (slot != null)
                        saves.Add(slot);
                }
            }
            return saves;
        }

        public string GetSaveInfo(int slotId)
        {
            var slot = LoadGame(slotId);
            if (slot == null) return "空存档";

            var timeDiff = DateTime.Now - slot.SaveTime;
            string timeStr = timeDiff.TotalHours > 1 
                ? $"{(int)timeDiff.TotalHours}小时前" 
                : timeDiff.TotalMinutes > 1 
                    ? $"{(int)timeDiff.TotalMinutes}分钟前"
                    : "刚刚";

            return $"角色: {slot.CharacterId}\n" +
                   $"第{slot.CurrentFloor}层 | HP:{slot.CurrentHealth}/{slot.MaxHealth}\n" +
                   $"💰{slot.Gold} | 击敌:{slot.TotalKills}\n" +
                   $"保存于: {timeStr}";
        }

        private List<string> ExtractCardIds(List<CardData> deck)
        {
            var ids = new List<string>();
            foreach (var card in deck)
                ids.Add(card.Id);
            return ids;
        }

        private string GetSavePath(int slotId) => $"{SAVE_DIR}save_{slotId}.json";
        private string GetScreenshotPath(int slotId) => $"{SCREENSHOT_DIR}save_{slotId}.png";

        private void TakeScreenshot(int slotId)
        {
            try
            {
                var img = GetViewport().GetTexture().GetImage();
                if (img != null)
                {
                    img.SavePng(GetScreenshotPath(slotId));
                }
            }
            catch (System.Exception ex)
            {
                GD.PushWarning($"[Save] Screenshot failed: {ex.Message}");
            }
        }

        public RunData RestoreRun(SaveSlot slot)
        {
            var db = CardDatabase.Instance ?? new CardDatabase();
            var deck = new List<CardData>();
            foreach (var id in slot.DeckIds)
            {
                var card = db.GetCard(id);
                if (card != null) deck.Add(card);
            }

            return new RunData
            {
                Seed = slot.Seed,
                CharacterId = slot.CharacterId,
                CurrentFloor = slot.CurrentFloor,
                CurrentHealth = slot.CurrentHealth,
                MaxHealth = slot.MaxHealth,
                Gold = slot.Gold,
                Relics = new List<string>(slot.RelicIds),
                Potions = new List<string>(slot.PotionIds),
                Deck = deck,
                CustomData = new Dictionary<string, object>(slot.CustomData),
                StartTime = DateTime.Now.AddSeconds(-slot.PlayTimeSeconds),
                IsVictory = false,
                TotalEnemiesDefeated = slot.TotalKills,
                TotalDamageDealt = slot.TotalDamageDealt
            };
        }

        public void ExportSaveAsJson(int slotId, string outputPath)
        {
            var slot = LoadGame(slotId);
            if (slot == null) return;

            var json = JsonSerializer.Serialize(slot, new JsonSerializerOptions { WriteIndented = true });
            using (var file = FileAccess.Open(outputPath, FileAccess.ModeFlags.Write))
            {
                file.StoreString(json);
            }
        }

        public SaveSlot ImportSaveFromJson(string inputPath)
        {
            using (var file = FileAccess.Open(inputPath, FileAccess.ModeFlags.Read))
            {
                var json = file.GetAsText();
                return JsonSerializer.Deserialize<SaveSlot>(json);
            }
        }
    }
}
