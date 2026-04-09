using Godot;
using System;
using RoguelikeGame.Core;
using RoguelikeGame.Systems;

namespace RoguelikeGame.Entities
{
    public partial class Player : CharacterBody2D
    {
        [Export]
        public int MaxHealth { get; set; } = 100;

        [Export]
        public int Attack { get; set; } = 10;

        [Export]
        public int Defense { get; set; } = 5;

        [Export]
        public float Speed { get; set; } = 200f;

        [Export]
        public float DashSpeed { get; set; } = 500f;

        [Export]
        public float DashDuration { get; set; } = 0.2f;

        [Export]
        public float DashCooldown { get; set; } = 1.0f;

        public int CurrentHealth { get; private set; }
        public bool IsDashing { get; private set; }
        public bool IsInvincible { get; private set; }

        private float _dashTimer = 0f;
        private float _dashCooldownTimer = 0f;
        private Vector2 _dashDirection;

        [Signal]
        public delegate void HealthChangedEventHandler(int currentHealth, int maxHealth);

        [Signal]
        public delegate void DiedEventHandler();

        public override void _Ready()
        {
            CurrentHealth = MaxHealth;
            
            EventBus.Instance.Subscribe<int>(GameEvents.PlayerHealthChanged, OnHealthChanged);
            
            GD.Print($"[Player] Ready with {CurrentHealth}/{MaxHealth} HP");
        }

        public override void _PhysicsProcess(double delta)
        {
            var dt = (float)delta;

            if (_dashCooldownTimer > 0)
                _dashCooldownTimer -= dt;

            if (IsDashing)
            {
                _dashTimer -= dt;
                if (_dashTimer <= 0)
                {
                    IsDashing = false;
                    IsInvincible = false;
                }
            }

            Vector2 velocity;

            if (IsDashing)
            {
                velocity = _dashDirection * DashSpeed;
            }
            else
            {
                var inputDir = Vector2.Zero;
                if (Input.IsActionPressed("ui_up"))
                    inputDir.Y -= 1;
                if (Input.IsActionPressed("ui_down"))
                    inputDir.Y += 1;
                if (Input.IsActionPressed("ui_left"))
                    inputDir.X -= 1;
                if (Input.IsActionPressed("ui_right"))
                    inputDir.X += 1;

                velocity = inputDir.Normalized() * Speed;

                if (Input.IsActionJustPressed("dash") && _dashCooldownTimer <= 0 && inputDir != Vector2.Zero)
                {
                    StartDash(inputDir.Normalized());
                }
            }

            Velocity = velocity;
            MoveAndSlide();
        }

        private void StartDash(Vector2 direction)
        {
            IsDashing = true;
            IsInvincible = true;
            _dashDirection = direction;
            _dashTimer = DashDuration;
            _dashCooldownTimer = DashCooldown;
            
            GD.Print("[Player] Dash started");
        }

        public void TakeDamage(int damage)
        {
            if (IsInvincible)
                return;

            int actualDamage = Math.Max(1, damage - Defense);
            CurrentHealth = Math.Max(0, CurrentHealth - actualDamage);

            GD.Print($"[Player] Took {actualDamage} damage, health: {CurrentHealth}/{MaxHealth}");

            EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
            EventBus.Instance.Publish(GameEvents.PlayerHealthChanged, CurrentHealth);

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            CurrentHealth = Math.Min(MaxHealth, CurrentHealth + amount);
            
            GD.Print($"[Player] Healed {amount}, health: {CurrentHealth}/{MaxHealth}");
            
            EmitSignal(SignalName.HealthChanged, CurrentHealth, MaxHealth);
            EventBus.Instance.Publish(GameEvents.PlayerHealthChanged, CurrentHealth);
        }

        private void Die()
        {
            GD.Print("[Player] Player died!");
            
            EmitSignal(SignalName.Died);
            EventBus.Instance.Publish(GameEvents.PlayerDied);
            
            GameManager.Instance?.EndRun(false);
        }

        private void OnHealthChanged(int newHealth)
        {
            CurrentHealth = newHealth;
        }

        public override void _ExitTree()
        {
            EventBus.Instance?.Unsubscribe<int>(GameEvents.PlayerHealthChanged, OnHealthChanged);
        }
    }
}
