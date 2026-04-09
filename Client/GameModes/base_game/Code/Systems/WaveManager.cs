using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;

namespace RoguelikeGame.Systems
{
    public class WaveData
    {
        public int WaveNumber { get; set; }
        public List<WaveEnemy> Enemies { get; set; } = new();
        public float DelayBetweenSpawns { get; set; } = 1.0f;
        public float DelayBetweenWaves { get; set; } = 3.0f;
    }

    public class WaveEnemy
    {
        public string UnitId { get; set; }
        public int Count { get; set; }
        public float SpawnWeight { get; set; } = 1.0f;
    }

    public partial class WaveManager : Node
    {
        public static WaveManager Instance { get; private set; }

        private int _currentWave = 0;
        private int _enemiesRemaining = 0;
        private bool _waveInProgress = false;
        private float _waveTimer = 0f;
        private Queue<WaveData> _waveQueue = new();

        private RandomGenerator _rng;

        [Export]
        public int MaxWaves { get; set; } = 10;

        [Export]
        public float BaseDifficulty { get; set; } = 1.0f;

        [Export]
        public float DifficultyScaling { get; set; } = 1.2f;

        [Signal]
        public delegate void WaveStartedEventHandler(int waveNumber);

        [Signal]
        public delegate void WaveCompletedEventHandler(int waveNumber);

        [Signal]
        public delegate void AllWavesCompletedEventHandler();

        public int CurrentWave => _currentWave;
        public int EnemiesRemaining => _enemiesRemaining;
        public bool IsWaveInProgress => _waveInProgress;

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            Instance = this;
        }

        public void Initialize(uint seed)
        {
            _rng = new RandomGenerator(seed);
            _currentWave = 0;
            _waveInProgress = false;
            _waveQueue.Clear();
            
            GenerateWaves();
        }

        private void GenerateWaves()
        {
            for (int i = 1; i <= MaxWaves; i++)
            {
                var wave = GenerateWave(i);
                _waveQueue.Enqueue(wave);
            }
            
            GD.Print($"[WaveManager] Generated {MaxWaves} waves");
        }

        private WaveData GenerateWave(int waveNumber)
        {
            var wave = new WaveData
            {
                WaveNumber = waveNumber,
                DelayBetweenSpawns = Mathf.Max(0.3f, 1.0f - (waveNumber * 0.05f)),
                DelayBetweenWaves = 3.0f
            };

            float difficulty = BaseDifficulty * Mathf.Pow(DifficultyScaling, waveNumber - 1);
            int totalEnemies = Mathf.RoundToInt(5 + (waveNumber * 2) * difficulty);

            var enemyPool = new List<string> { "Goblin" };
            if (waveNumber >= 3)
                enemyPool.Add("Skeleton");
            if (waveNumber >= 5)
                enemyPool.Add("Orc");
            if (waveNumber % 5 == 0)
                enemyPool.Add("Boss_Dragon");

            foreach (var enemyId in enemyPool)
            {
                int count = Mathf.RoundToInt(totalEnemies / enemyPool.Count);
                if (enemyId.StartsWith("Boss"))
                    count = 1;

                wave.Enemies.Add(new WaveEnemy
                {
                    UnitId = enemyId,
                    Count = count
                });
            }

            return wave;
        }

        public void StartNextWave()
        {
            if (_waveInProgress || _waveQueue.Count == 0)
                return;

            var wave = _waveQueue.Dequeue();
            _currentWave = wave.WaveNumber;
            _waveInProgress = true;
            _enemiesRemaining = 0;

            foreach (var enemy in wave.Enemies)
            {
                _enemiesRemaining += enemy.Count;
            }

            GD.Print($"[WaveManager] Starting wave {_currentWave} with {_enemiesRemaining} enemies");
            
            EmitSignal(SignalName.WaveStarted, _currentWave);
            EventBus.Instance.Publish(GameEvents.WaveStarted, _currentWave);

            SpawnWave(wave);
        }

        private async void SpawnWave(WaveData wave)
        {
            foreach (var enemy in wave.Enemies)
            {
                for (int i = 0; i < enemy.Count; i++)
                {
                    SpawnEnemy(enemy.UnitId);
                    await ToSignal(GetTree().CreateTimer(wave.DelayBetweenSpawns), SceneTreeTimer.SignalName.Timeout);
                }
            }
        }

        private void SpawnEnemy(string unitId)
        {
            var spawnPos = GetRandomSpawnPosition();
            UnitManager.Instance.SpawnUnit(unitId, spawnPos);
        }

        private Vector2 GetRandomSpawnPosition()
        {
            var viewport = GetViewport().GetVisibleRect();
            var margin = 50f;

            int side = _rng.Next(4);
            float x, y;

            switch (side)
            {
                case 0:
                    x = _rng.NextFloat(margin, viewport.Size.X - margin);
                    y = -margin;
                    break;
                case 1:
                    x = viewport.Size.X + margin;
                    y = _rng.NextFloat(margin, viewport.Size.Y - margin);
                    break;
                case 2:
                    x = _rng.NextFloat(margin, viewport.Size.X - margin);
                    y = viewport.Size.Y + margin;
                    break;
                default:
                    x = -margin;
                    y = _rng.NextFloat(margin, viewport.Size.Y - margin);
                    break;
            }

            return new Vector2(x, y);
        }

        public void OnEnemyDied(Node enemy)
        {
            _enemiesRemaining--;
            
            if (_enemiesRemaining <= 0 && _waveInProgress)
            {
                CompleteWave();
            }
        }

        private void CompleteWave()
        {
            _waveInProgress = false;
            
            GD.Print($"[WaveManager] Wave {_currentWave} completed!");
            
            EmitSignal(SignalName.WaveCompleted, _currentWave);
            EventBus.Instance.Publish(GameEvents.WaveCompleted, _currentWave);

            if (_waveQueue.Count == 0)
            {
                GD.Print("[WaveManager] All waves completed!");
                EmitSignal(SignalName.AllWavesCompleted);
                EventBus.Instance.Publish(GameEvents.AllWavesCompleted);
            }
        }

        public void SkipToWave(int waveNumber)
        {
            while (_waveQueue.Count > 0 && _waveQueue.Peek().WaveNumber < waveNumber)
            {
                _waveQueue.Dequeue();
            }
            _currentWave = waveNumber - 1;
            GD.Print($"[WaveManager] Skipped to wave {waveNumber}");
        }
    }
}
