using Godot;
using System;
using System.Collections.Generic;

namespace RoguelikeGame.Database
{
    public enum TimelineEventType
    {
        CombatStart,
        CombatEnd,
        CardPlayed,
        RelicObtained,
        PotionUsed,
        EventTriggered,
        ShopVisit,
        RestUsed,
        BossDefeated,
        Death,
        Victory,
        LevelUp,
        Custom
    }

    public class TimelineEntry
    {
        public string Id { get; set; }
        public TimelineEventType Type { get; set; }
        public DateTime Timestamp { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object> Data { get; set; } = new();
        public int Floor { get; set; }
        public int Room { get; set; }
    }

    public partial class TimelineManager : Node
    {
        public static TimelineManager Instance { get; private set; }

        private readonly List<TimelineEntry> _timeline = new();
        private readonly Dictionary<TimelineEventType, List<TimelineEntry>> _typedEntries = new();

        [Signal]
        public delegate void TimelineEntryAddedEventHandler(string entryJson);

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            Instance = this;
        }

        public TimelineEntry AddEntry(
            TimelineEventType type,
            string title,
            string description = "",
            Dictionary<string, object> data = null,
            int floor = 0,
            int room = 0)
        {
            var entry = new TimelineEntry
            {
                Id = Guid.NewGuid().ToString(),
                Type = type,
                Timestamp = DateTime.Now,
                Title = title,
                Description = description,
                Data = data ?? new Dictionary<string, object>(),
                Floor = floor,
                Room = room
            };

            _timeline.Add(entry);

            if (!_typedEntries.ContainsKey(type))
                _typedEntries[type] = new List<TimelineEntry>();
            
            _typedEntries[type].Add(entry);

            EmitSignal(SignalName.TimelineEntryAdded, entry.Id);

            return entry;
        }

        public void AddCombatStart(int floor, int room, List<string> enemies)
        {
            AddEntry(
                TimelineEventType.CombatStart,
                $"战斗开始 - 第{floor}层 房间{room}",
                $"遭遇敌人: {string.Join(", ", enemies)}",
                new Dictionary<string, object>
                {
                    { "enemies", enemies }
                },
                floor,
                room
            );
        }

        public void AddCombatEnd(int floor, int room, bool victory, int damageTaken, int damageDealt)
        {
            AddEntry(
                TimelineEventType.CombatEnd,
                victory ? "战斗胜利" : "战斗失败",
                $"造成 {damageDealt} 点伤害，承受 {damageTaken} 点伤害",
                new Dictionary<string, object>
                {
                    { "victory", victory },
                    { "damage_taken", damageTaken },
                    { "damage_dealt", damageDealt }
                },
                floor,
                room
            );
        }

        public void AddCardPlayed(string cardName, int cost, int floor, int room)
        {
            AddEntry(
                TimelineEventType.CardPlayed,
                $"打出卡牌: {cardName}",
                $"消耗 {cost} 点能量",
                new Dictionary<string, object>
                {
                    { "card_name", cardName },
                    { "cost", cost }
                },
                floor,
                room
            );
        }

        public void AddRelicObtained(string relicName, string source, int floor, int room)
        {
            AddEntry(
                TimelineEventType.RelicObtained,
                $"获得遗物: {relicName}",
                $"来源: {source}",
                new Dictionary<string, object>
                {
                    { "relic_name", relicName },
                    { "source", source }
                },
                floor,
                room
            );
        }

        public void AddPotionUsed(string potionName, int floor, int room)
        {
            AddEntry(
                TimelineEventType.PotionUsed,
                $"使用药水: {potionName}",
                "",
                new Dictionary<string, object>
                {
                    { "potion_name", potionName }
                },
                floor,
                room
            );
        }

        public void AddEventTriggered(string eventName, string choice, int floor, int room)
        {
            AddEntry(
                TimelineEventType.EventTriggered,
                $"触发事件: {eventName}",
                $"选择: {choice}",
                new Dictionary<string, object>
                {
                    { "event_name", eventName },
                    { "choice", choice }
                },
                floor,
                room
            );
        }

        public void AddBossDefeated(string bossName, int floor)
        {
            AddEntry(
                TimelineEventType.BossDefeated,
                $"击败Boss: {bossName}",
                $"第 {floor} 层Boss战胜利",
                new Dictionary<string, object>
                {
                    { "boss_name", bossName }
                },
                floor,
                0
            );
        }

        public void AddDeath(int floor, int room, string cause)
        {
            AddEntry(
                TimelineEventType.Death,
                "死亡",
                $"死亡原因: {cause}",
                new Dictionary<string, object>
                {
                    { "cause", cause }
                },
                floor,
                room
            );
        }

        public void AddVictory(uint seed, int totalDamage, int totalCardsPlayed)
        {
            AddEntry(
                TimelineEventType.Victory,
                "游戏通关！",
                $"种子: {seed}, 总伤害: {totalDamage}, 出牌数: {totalCardsPlayed}",
                new Dictionary<string, object>
                {
                    { "seed", seed },
                    { "total_damage", totalDamage },
                    { "total_cards_played", totalCardsPlayed }
                }
            );
        }

        public List<TimelineEntry> GetAllEntries()
        {
            return new List<TimelineEntry>(_timeline);
        }

        public List<TimelineEntry> GetEntriesByType(TimelineEventType type)
        {
            return _typedEntries.TryGetValue(type, out var entries) 
                ? entries 
                : new List<TimelineEntry>();
        }

        public List<TimelineEntry> GetRecentEntries(int count = 20)
        {
            var start = Math.Max(0, _timeline.Count - count);
            return _timeline.GetRange(start, Math.Min(count, _timeline.Count - start));
        }

        public List<TimelineEntry> GetFloorTimeline(int floor)
        {
            var result = new List<TimelineEntry>();
            foreach (var entry in _timeline)
            {
                if (entry.Floor == floor)
                    result.Add(entry);
            }
            return result;
        }

        public void ClearTimeline()
        {
            _timeline.Clear();
            _typedEntries.Clear();
            GD.Print("[TimelineManager] Timeline cleared");
        }

        public int TotalEntries => _timeline.Count;
    }
}
