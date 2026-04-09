using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;
using RoguelikeGame.Systems;

namespace RoguelikeGame.Entities
{
    public enum AIState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Flee,
        Dead
    }

    public enum AIType
    {
        Aggressive,
        Passive,
        Neutral,
        Boss
    }

    public partial class EnemyAI : Node
    {
        [Export]
        public AIType Type { get; set; } = AIType.Aggressive;

        [Export]
        public float DetectionRange { get; set; } = 200f;

        [Export]
        public float AttackRange { get; set; } = 50f;

        [Export]
        public float AttackCooldown { get; set; } = 1.5f;

        [Export]
        public float MoveSpeed { get; set; } = 100f;

        [Export]
        public float PatrolRadius { get; set; } = 100f;

        [Export]
        public float FleeHealthPercent { get; set; } = 0.2f;

        private CharacterBody2D _owner;
        private AIState _currentState = AIState.Idle;
        private Node _target;
        private Vector2 _spawnPosition;
        private Vector2 _patrolTarget;
        private float _attackTimer = 0f;
        private float _stateTimer = 0f;
        private RandomNumberGenerator _rng = new();

        private readonly Dictionary<AIState, Action> _stateActions;

        public AIState CurrentState
        {
            get => _currentState;
            set
            {
                if (_currentState != value)
                {
                    var oldState = _currentState;
                    _currentState = value;
                    OnStateChange(oldState, value);
                }
            }
        }

        public EnemyAI()
        {
            _stateActions = new Dictionary<AIState, Action>
            {
                { AIState.Idle, UpdateIdle },
                { AIState.Patrol, UpdatePatrol },
                { AIState.Chase, UpdateChase },
                { AIState.Attack, UpdateAttack },
                { AIState.Flee, UpdateFlee },
                { AIState.Dead, UpdateDead }
            };
        }

        public override void _Ready()
        {
            _owner = GetParent<CharacterBody2D>();
            if (_owner == null)
            {
                GD.PrintErr("[EnemyAI] Parent is not CharacterBody2D");
                return;
            }

            _spawnPosition = _owner.Position;
            _patrolTarget = _spawnPosition;
            _rng.Randomize();

            ChangeState(AIState.Idle);

            GD.Print($"[EnemyAI] Initialized for {_owner.Name}");
        }

        public override void _PhysicsProcess(double delta)
        {
            var dt = (float)delta;

            if (_attackTimer > 0)
                _attackTimer -= dt;

            if (_stateTimer > 0)
                _stateTimer -= dt;

            if (_stateActions.TryGetValue(_currentState, out var action))
            {
                action?.Invoke();
            }

            _owner.MoveAndSlide();
        }

        private void UpdateIdle()
        {
            _owner.Velocity = Vector2.Zero;

            if (_stateTimer <= 0)
            {
                if (Type == AIType.Passive || Type == AIType.Neutral)
                {
                    ChangeState(AIState.Patrol);
                }
                else
                {
                    var player = FindPlayerInRange(DetectionRange);
                    if (player != null)
                    {
                        _target = player;
                        ChangeState(AIState.Chase);
                    }
                    else
                    {
                        ChangeState(AIState.Patrol);
                    }
                }
            }
        }

        private void UpdatePatrol()
        {
            if (Type == AIType.Aggressive)
            {
                var player = FindPlayerInRange(DetectionRange);
                if (player != null)
                {
                    _target = player;
                    ChangeState(AIState.Chase);
                    return;
                }
            }

            var distanceToTarget = _owner.Position.DistanceTo(_patrolTarget);
            
            if (distanceToTarget < 10f)
            {
                _patrolTarget = GetRandomPatrolPoint();
                ChangeState(AIState.Idle, 1f);
                return;
            }

            var direction = (_patrolTarget - _owner.Position).Normalized();
            _owner.Velocity = direction * MoveSpeed * 0.5f;
        }

        private void UpdateChase()
        {
            if (_target == null || !IsInstanceValid(_target))
            {
                _target = null;
                ChangeState(AIState.Idle);
                return;
            }

            var distanceToTarget = _owner.Position.DistanceTo((Vector2)_target.Get("Position"));

            if (distanceToTarget <= AttackRange)
            {
                ChangeState(AIState.Attack);
                return;
            }

            if (distanceToTarget > DetectionRange * 1.5f)
            {
                _target = null;
                ChangeState(AIState.Idle);
                return;
            }

            if (Type != AIType.Aggressive && ShouldFlee())
            {
                ChangeState(AIState.Flee);
                return;
            }

            var direction = ((Vector2)_target.Get("Position") - _owner.Position).Normalized();
            _owner.Velocity = direction * MoveSpeed;
        }

        private void UpdateAttack()
        {
            _owner.Velocity = Vector2.Zero;

            if (_target == null || !IsInstanceValid(_target))
            {
                _target = null;
                ChangeState(AIState.Idle);
                return;
            }

            var distanceToTarget = _owner.Position.DistanceTo((Vector2)_target.Get("Position"));

            if (distanceToTarget > AttackRange * 1.2f)
            {
                ChangeState(AIState.Chase);
                return;
            }

            if (_attackTimer <= 0)
            {
                PerformAttack();
                _attackTimer = AttackCooldown;
            }

            if (ShouldFlee())
            {
                ChangeState(AIState.Flee);
            }
        }

        private void UpdateFlee()
        {
            if (_target == null || !IsInstanceValid(_target))
            {
                ChangeState(AIState.Idle);
                return;
            }

            var fleeDirection = (_owner.Position - (Vector2)_target.Get("Position")).Normalized();
            _owner.Velocity = fleeDirection * MoveSpeed * 1.2f;

            var distanceToTarget = _owner.Position.DistanceTo((Vector2)_target.Get("Position"));
            if (distanceToTarget > DetectionRange * 2f)
            {
                ChangeState(AIState.Idle);
            }
        }

        private void UpdateDead()
        {
            _owner.Velocity = Vector2.Zero;
        }

        private void ChangeState(AIState newState, float duration = 0f)
        {
            CurrentState = newState;
            _stateTimer = duration;
        }

        private void OnStateChange(AIState oldState, AIState newState)
        {
            GD.Print($"[EnemyAI] {_owner.Name}: {oldState} -> {newState}");
        }

        private Node FindPlayerInRange(float range)
        {
            var players = UnitManager.Instance?.GetAllUnits(UnitType.Player);
            if (players == null || players.Count == 0)
                return null;

            var rangeSquared = range * range;
            foreach (var player in players)
            {
                var playerPos = (Vector2)player.Get("Position");
                if (_owner.Position.DistanceSquaredTo(playerPos) <= rangeSquared)
                {
                    return player;
                }
            }

            return null;
        }

        private Vector2 GetRandomPatrolPoint()
        {
            var angle = _rng.RandfRange(0, Mathf.Tau);
            var distance = _rng.RandfRange(0, PatrolRadius);
            
            return _spawnPosition + new Vector2(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance
            );
        }

        private bool ShouldFlee()
        {
            if (Type == AIType.Aggressive)
                return false;

            if (!_owner.HasMethod("GetCurrentHealth") || !_owner.HasMethod("GetMaxHealth"))
                return false;

            var currentHealth = (int)_owner.Call("GetCurrentHealth");
            var maxHealth = (int)_owner.Call("GetMaxHealth");
            
            return (float)currentHealth / maxHealth < FleeHealthPercent;
        }

        private void PerformAttack()
        {
            if (_target == null)
                return;

            var damageInfo = DamageSystem.Instance?.CreateDamageInfo(
                amount: 10,
                type: DamageType.Physical,
                source: _owner
            );

            if (damageInfo != null)
            {
                damageInfo.Target = _target;
                DamageSystem.Instance?.ApplyDamage(damageInfo);
            }

            GD.Print($"[EnemyAI] {_owner.Name} attacks {_target.Name}");
        }

        public void OnTakeDamage(DamageInfo info)
        {
            if (_target == null && info.Source != null)
            {
                _target = info.Source;
            }

            if (CurrentState == AIState.Idle || CurrentState == AIState.Patrol)
            {
                if (Type == AIType.Aggressive)
                {
                    ChangeState(AIState.Chase);
                }
                else if (ShouldFlee())
                {
                    ChangeState(AIState.Flee);
                }
            }
        }

        public void OnDeath()
        {
            ChangeState(AIState.Dead);

            if (_owner.HasMethod("GetUnitId"))
            {
                var unitId = (string)_owner.Call("GetUnitId");
                var lootTable = GetLootTableForEnemy(unitId);
                LootSystem.Instance?.DropLootFromEnemy(lootTable, _owner);
            }

            WaveManager.Instance?.OnEnemyDied(_owner);
        }

        private string GetLootTableForEnemy(string unitId)
        {
            if (unitId.Contains("boss", StringComparison.OrdinalIgnoreCase))
                return "boss";
            else if (unitId.Contains("elite", StringComparison.OrdinalIgnoreCase))
                return "elite_enemy";
            else
                return "common_enemy";
        }
    }
}
