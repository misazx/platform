using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using RoguelikeGame.Core;

namespace RoguelikeGame.Database
{
    public enum EventType
    {
        Choice,
        Combat,
        Shop,
        Rest,
        Treasure,
        Special,
        Curse
    }

    public class EventChoice
    {
        public string Text { get; set; }
        public string Description { get; set; }
        public Dictionary<string, object> Rewards { get; set; } = new();
        public Dictionary<string, object> Penalties { get; set; } = new();
        public bool RequiresCondition { get; set; }
        public string ConditionKey { get; set; }
        public object ConditionValue { get; set; }
    }

    public class EventData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FlavorText { get; set; }
        
        public EventType Type { get; set; } = EventType.Choice;
        
        public string ImagePath { get; set; }
        public string Location { get; set; }
        
        public List<EventChoice> Choices { get; set; } = new();
        public Dictionary<string, object> CustomData { get; set; } = new();
        
        public float Weight { get; set; } = 1.0f;
        public bool OneTime { get; set; }
        public bool HasSeen { get; set; }
    }

    public partial class EventDatabase : Node
    {
        public static EventDatabase Instance { get; private set; }

        private readonly Dictionary<string, EventData> _events = new();
        private readonly Dictionary<EventType, List<EventData>> _typeEvents = new();

        [Signal]
        public delegate void EventTriggeredEventHandler(string eventId);

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            Instance = this;

            LoadEventsFromConfig();
        }

        private void LoadEventsFromConfig()
        {
            var config = ConfigLoader.LoadConfig<EventConfigData>("events");
            
            if (config == null)
            {
                GD.PrintErr("[EventDatabase] Failed to load events config!");
                return;
            }

            foreach (var eventConfig in config.Events)
            {
                var eventData = ConvertConfigToData(eventConfig);
                RegisterEvent(eventData);
            }

            GD.Print($"[EventDatabase] Loaded {_events.Count} events from config (version: {config.Version})");
        }

        private EventData ConvertConfigToData(EventConfig config)
        {
            return new EventData
            {
                Id = config.Id,
                Name = config.Name,
                Description = config.Description,
                FlavorText = config.FlavorText,
                Type = ParseEventType(config.Type),
                ImagePath = config.ImagePath,
                Location = config.Location,
                Choices = config.Choices.ConvertAll(c => new EventChoice
                {
                    Text = c.Text,
                    Description = c.Description,
                    Rewards = new Dictionary<string, object>(c.Rewards),
                    Penalties = new Dictionary<string, object>(c.Penalties),
                    RequiresCondition = c.RequiresCondition,
                    ConditionKey = c.ConditionKey,
                    ConditionValue = c.ConditionValue
                }),
                CustomData = new Dictionary<string, object>(config.CustomData),
                Weight = config.Weight,
                OneTime = config.OneTime,
                HasSeen = config.HasSeen
            };
        }

        private EventType ParseEventType(string type)
        {
            return type?.ToLower() switch
            {
                "choice" => EventType.Choice,
                "combat" => EventType.Combat,
                "shop" => EventType.Shop,
                "rest" => EventType.Rest,
                "treasure" => EventType.Treasure,
                "special" => EventType.Special,
                "curse" => EventType.Curse,
                _ => EventType.Choice
            };
        }

        public void RegisterEvent(EventData event_)
        {
            _events[event_.Id] = event_;

            if (!_typeEvents.ContainsKey(event_.Type))
                _typeEvents[event_.Type] = new List<EventData>();
            
            _typeEvents[event_.Type].Add(event_);
        }

        public EventData GetEvent(string eventId)
        {
            return _events.TryGetValue(eventId, out var event_) ? event_ : null;
        }

        public List<EventData> GetAllEvents()
        {
            return new List<EventData>(_events.Values);
        }

        public List<EventData> GetEventsByType(EventType type)
        {
            return _typeEvents.TryGetValue(type, out var events) 
                ? events 
                : new List<EventData>();
        }

        public List<EventData> GetEventsByLocation(string location)
        {
            var result = new List<EventData>();
            foreach (var event_ in _events.Values)
            {
                if (event_.Location == location || event_.Location == "anywhere")
                    result.Add(event_);
            }
            return result;
        }

        public EventData GetRandomEvent(string location, RandomNumberGenerator rng = null)
        {
            var availableEvents = GetEventsByLocation(location);
            if (availableEvents.Count == 0)
                return null;

            if (rng != null)
            {
                int idx = (int)(rng.Randf() * availableEvents.Count);
                return availableEvents[idx];
            }
            
            var randomIndex = (int)(GD.Randi() % availableEvents.Count);
            return availableEvents[randomIndex];
        }

        public int TotalEvents => _events.Count;
    }
}
