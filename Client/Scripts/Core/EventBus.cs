using Godot;
using System;
using System.Collections.Generic;

namespace RoguelikeGame.Core
{
    public partial class EventBus : Node
    {
        public static EventBus Instance { get; private set; }

        private readonly Dictionary<string, List<Delegate>> _eventHandlers = new();

        [Signal]
        public delegate void NetworkConnectedEventHandler();

        [Signal]
        public delegate void NetworkDisconnectedEventHandler();

        [Signal]
        public delegate void NetworkErrorHandler(string error);

        [Signal]
        public delegate void NetworkDataReceivedEventHandler(byte[] data);

        [Signal]
        public delegate void GameStateUpdatedEventHandler(object state);

        public event Action NetworkConnected;
        public event Action NetworkDisconnected;
        public event Action<string> NetworkError;
        public event Action<byte[]> NetworkDataReceived;
        public event Action<object> GameStateUpdated;

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            Instance = this;
        }

        public void Subscribe<T>(string eventName, Action<T> handler)
        {
            if (!_eventHandlers.ContainsKey(eventName))
            {
                _eventHandlers[eventName] = new List<Delegate>();
            }
            _eventHandlers[eventName].Add(handler);
        }

        public void Subscribe(string eventName, Action handler)
        {
            if (!_eventHandlers.ContainsKey(eventName))
            {
                _eventHandlers[eventName] = new List<Delegate>();
            }
            _eventHandlers[eventName].Add(handler);
        }

        public void Unsubscribe<T>(string eventName, Action<T> handler)
        {
            if (_eventHandlers.ContainsKey(eventName))
            {
                _eventHandlers[eventName].Remove(handler);
            }
        }

        public void Unsubscribe(string eventName, Action handler)
        {
            if (_eventHandlers.ContainsKey(eventName))
            {
                _eventHandlers[eventName].Remove(handler);
            }
        }

        public void Publish<T>(string eventName, T eventData)
        {
            if (!_eventHandlers.ContainsKey(eventName))
                return;

            foreach (var handler in _eventHandlers[eventName].ToArray())
            {
                if (handler is Action<T> typedHandler)
                {
                    typedHandler?.Invoke(eventData);
                }
            }
        }

        public void Publish(string eventName)
        {
            if (!_eventHandlers.ContainsKey(eventName))
                return;

            foreach (var handler in _eventHandlers[eventName].ToArray())
            {
                if (handler is Action typedHandler)
                {
                    typedHandler?.Invoke();
                }
            }
        }

        public void Clear()
        {
            _eventHandlers.Clear();
        }
    }

    public static class GameEvents
    {
        public const string GameStarted = "GameStarted";
        public const string GamePaused = "GamePaused";
        public const string GameResumed = "GameResumed";
        public const string GameOver = "GameOver";
        
        public const string RoomEntered = "RoomEntered";
        public const string RoomCleared = "RoomCleared";
        public const string FloorCompleted = "FloorCompleted";
        
        public const string WaveStarted = "WaveStarted";
        public const string WaveCompleted = "WaveCompleted";
        public const string AllWavesCompleted = "AllWavesCompleted";
        
        public const string UnitSpawned = "UnitSpawned";
        public const string UnitDied = "UnitDied";
        
        public const string EnemySpawned = "EnemySpawned";
        public const string EnemyDied = "EnemyDied";
        
        public const string ItemPickedUp = "ItemPickedUp";
        public const string ItemDropped = "ItemDropped";
        
        public const string PlayerHealthChanged = "PlayerHealthChanged";
        public const string PlayerDied = "PlayerDied";

        public const string NetworkConnected = "NetworkConnected";
        public const string NetworkDisconnected = "NetworkDisconnected";
        public const string NetworkError = "NetworkError";
        public const string AuthenticationSuccess = "AuthenticationSuccess";
        public const string AuthenticationFailed = "AuthenticationFailed";
        public const string RoomCreated = "RoomCreated";
        public const string RoomJoined = "RoomJoined";
        public const string RoomLeft = "RoomLeft";
        public const string GameSessionStarted = "GameSessionStarted";
        public const string GameSessionEnded = "GameSessionEnded";
    }
}
