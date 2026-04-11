using Godot;
using RoguelikeGame.Core;
using System;
using System.Collections.Generic;

namespace RoguelikeGame.Systems
{
    public enum UnitType
    {
        Player,
        Enemy,
        Boss,
        NPC
    }

    public class UnitData
    {
        public string Name { get; set; }
        public UnitType Type { get; set; }
        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public float Speed { get; set; }
        public string ScenePath { get; set; }
        public Dictionary<string, object> CustomData { get; set; } = new();
    }

    public partial class UnitManager : SingletonBase<UnitManager>
    {
        private readonly Dictionary<string, PackedScene> _unitScenes = new();
        private readonly Dictionary<string, UnitData> _unitDefinitions = new();
        private readonly List<Node> _activeUnits = new();
        private readonly Queue<Node> _unitPool = new();

        [Export]
        public int MaxActiveUnits { get; set; } = 100;

        [Export]
        public int PoolSize { get; set; } = 50;

        [Signal]
        public delegate void UnitSpawnedEventHandler(Node unit);

        [Signal]
        public delegate void UnitDiedEventHandler(Node unit);

        [Signal]
        public delegate void WaveCompletedEventHandler(int waveNumber);

        public int ActiveUnitCount => _activeUnits.Count;

        protected override void OnInitialize()
        {
            LoadUnitDefinitions();
        }

        private void LoadUnitDefinitions()
        {
            RegisterUnit("Goblin", new UnitData
            {
                Name = "Goblin",
                Type = UnitType.Enemy,
                MaxHealth = 30,
                Attack = 5,
                Defense = 2,
                Speed = 100f,
                ScenePath = "res://GameModes/base_game/Scenes/Units/Goblin.tscn"
            });

            RegisterUnit("Skeleton", new UnitData
            {
                Name = "Skeleton",
                Type = UnitType.Enemy,
                MaxHealth = 50,
                Attack = 8,
                Defense = 3,
                Speed = 80f,
                ScenePath = "res://GameModes/base_game/Scenes/Units/Skeleton.tscn"
            });

            RegisterUnit("Boss_Dragon", new UnitData
            {
                Name = "Dragon",
                Type = UnitType.Boss,
                MaxHealth = 500,
                Attack = 25,
                Defense = 10,
                Speed = 60f,
                ScenePath = "res://GameModes/base_game/Scenes/Units/BossDragon.tscn"
            });

            GD.Print($"[UnitManager] Loaded {_unitDefinitions.Count} unit definitions");
        }

        public void RegisterUnit(string unitId, UnitData data)
        {
            _unitDefinitions[unitId] = data;
        }

        public UnitData GetUnitData(string unitId)
        {
            return _unitDefinitions.TryGetValue(unitId, out var data) ? data : null;
        }

        public Node SpawnUnit(string unitId, Vector2 position, Node parent = null)
        {
            if (_activeUnits.Count >= MaxActiveUnits)
            {
                GD.PrintErr($"[UnitManager] Max active units reached ({MaxActiveUnits})");
                return null;
            }

            var unitData = GetUnitData(unitId);
            if (unitData == null)
            {
                GD.PrintErr($"[UnitManager] Unit not found: {unitId}");
                return null;
            }

            Node unit;

            if (_unitPool.Count > 0)
            {
                unit = _unitPool.Dequeue();
                unit.Set("Position", position);
            }
            else
            {
                if (!_unitScenes.TryGetValue(unitId, out var scene))
                {
                    if (!ResourceLoader.Exists(unitData.ScenePath))
                    {
                        GD.PrintErr($"[UnitManager] Scene not found: {unitData.ScenePath}");
                        return null;
                    }
                    scene = ResourceLoader.Load<PackedScene>(unitData.ScenePath);
                    _unitScenes[unitId] = scene;
                }

                unit = scene.Instantiate();
                unit.Set("Position", position);
            }

            parent ??= GetTree().CurrentScene;
            parent.AddChild(unit);

            _activeUnits.Add(unit);

            EmitSignal(SignalName.UnitSpawned, unit.Name);
            EventBus.Instance.Publish(GameEvents.EnemySpawned, unit.Name);

            GD.Print($"[UnitManager] Spawned {unitId} at {position}");
            return unit;
        }

        public void DespawnUnit(Node unit, bool returnToPool = true)
        {
            if (!_activeUnits.Contains(unit))
                return;

            _activeUnits.Remove(unit);

            if (returnToPool && _unitPool.Count < PoolSize)
            {
                unit.GetParent()?.RemoveChild(unit);
                _unitPool.Enqueue(unit);
            }
            else
            {
                unit.QueueFree();
            }

            EmitSignal(SignalName.UnitDied, unit.Name);
            EventBus.Instance.Publish(GameEvents.EnemyDied, unit.Name);
        }

        public void DespawnAllUnits()
        {
            foreach (var unit in _activeUnits.ToArray())
            {
                DespawnUnit(unit, true);
            }
            GD.Print("[UnitManager] All units despawned");
        }

        public List<Node> GetUnitsInRange(Vector2 center, float radius, UnitType? filterType = null)
        {
            var result = new List<Node>();
            var radiusSquared = radius * radius;

            foreach (var unit in _activeUnits)
            {
                var pos = (Vector2)unit.Get("Position");
                if (pos.DistanceSquaredTo(center) <= radiusSquared)
                {
                    if (filterType == null)
                    {
                        result.Add(unit);
                    }
                    else
                    {
                        var unitType = (UnitType)unit.Get("UnitType").AsInt32();
                        if (unitType == filterType)
                            result.Add(unit);
                    }
                }
            }

            return result;
        }

        public List<Node> GetAllUnits(UnitType? filterType = null)
        {
            if (filterType == null)
                return new List<Node>(_activeUnits);

            var result = new List<Node>();
            foreach (var unit in _activeUnits)
            {
                var unitType = (UnitType)unit.Get("UnitType").AsInt32();
                if (unitType == filterType)
                    result.Add(unit);
            }
            return result;
        }
    }
}
