using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;

namespace RoguelikeGame.Systems
{
    public enum DamageType
    {
        Physical,
        Magical,
        Fire,
        Ice,
        Lightning,
        Poison,
        True
    }

    public class DamageInfo
    {
        public int Amount { get; set; }
        public DamageType Type { get; set; } = DamageType.Physical;
        public Node Source { get; set; }
        public Node Target { get; set; }
        public bool IsCritical { get; set; }
        public float KnockbackForce { get; set; }
        public Vector2 KnockbackDirection { get; set; }
        public Dictionary<string, object> CustomData { get; set; } = new();

        public DamageInfo(int amount, DamageType type = DamageType.Physical)
        {
            Amount = amount;
            Type = type;
        }
    }

    public class DamageResult
    {
        public int OriginalDamage { get; set; }
        public int FinalDamage { get; set; }
        public int DamageBlocked { get; set; }
        public bool WasCritical { get; set; }
        public bool WasDodged { get; set; }
        public bool KilledTarget { get; set; }
        public Dictionary<string, object> Modifiers { get; set; } = new();
    }

    public partial class DamageSystem : Node
    {
        public static DamageSystem Instance { get; private set; }

        [Export]
        public float CriticalMultiplier { get; set; } = 2.0f;

        [Export]
        public float CriticalChance { get; set; } = 0.1f;

        [Export]
        public float KnockbackMultiplier { get; set; } = 1.0f;

        private readonly Dictionary<DamageType, float> _typeResistances = new();
        private readonly List<string> _damageModifiers = new();

        [Signal]
        public delegate void DamageDealtEventHandler(Node source, Node target, int amount);

        [Signal]
        public delegate void DamageTakenEventHandler(Node target, int amount);

        [Signal]
        public delegate void CriticalHitEventHandler(Node source, Node target, int amount);

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            Instance = this;
        }

        public DamageResult CalculateDamage(DamageInfo info)
        {
            var result = new DamageResult
            {
                OriginalDamage = info.Amount
            };

            if (info.Target == null)
                return result;

            float finalDamage = info.Amount;

            if (info.Source != null)
            {
                if (info.Source.HasMethod("GetCriticalChance"))
                {
                    float critChance = (float)info.Source.Call("GetCriticalChance");
                    if (GD.Randf() < critChance)
                    {
                        info.IsCritical = true;
                        result.WasCritical = true;
                        finalDamage *= CriticalMultiplier;
                    }
                }
            }

            if (info.Target.HasMethod("GetDefense"))
            {
                int defense = (int)info.Target.Call("GetDefense");
                float damageReduction = defense / (defense + 100f);
                int blocked = Mathf.RoundToInt(finalDamage * damageReduction);
                result.DamageBlocked = blocked;
                finalDamage -= blocked;
            }

            if (info.Target.HasMethod("GetResistance"))
            {
                float resistance = (float)info.Target.Call("GetResistance", (int)info.Type);
                finalDamage *= (1f - resistance);
            }

            if (info.Target.HasMethod("GetDodgeChance"))
            {
                float dodgeChance = (float)info.Target.Call("GetDodgeChance");
                if (GD.Randf() < dodgeChance)
                {
                    result.WasDodged = true;
                    finalDamage = 0;
                }
            }

            finalDamage = Mathf.Max(1, finalDamage);
            result.FinalDamage = Mathf.RoundToInt(finalDamage);

            return result;
        }

        public DamageResult ApplyDamage(DamageInfo info)
        {
            if (info.Target == null)
            {
                GD.PrintErr("[DamageSystem] Target is null");
                return new DamageResult();
            }

            var result = CalculateDamage(info);

            if (result.WasDodged)
            {
                GD.Print($"[DamageSystem] Attack dodged!");
                return result;
            }

            if (info.Target.HasMethod("TakeDamage"))
            {
                info.Target.Call("TakeDamage", result.FinalDamage);
            }
            else if (info.Target.HasMethod("Set"))
            {
                var currentHealth = (int)info.Target.Get("CurrentHealth");
                var newHealth = Mathf.Max(0, currentHealth - result.FinalDamage);
                info.Target.Set("CurrentHealth", newHealth);

                if (newHealth <= 0)
                {
                    result.KilledTarget = true;
                    if (info.Target.HasMethod("Die"))
                    {
                        info.Target.Call("Die");
                    }
                }
            }

            if (info.KnockbackForce > 0 && info.Target is CharacterBody2D body)
            {
                ApplyKnockback(body, info.KnockbackDirection, info.KnockbackForce);
            }

            EmitSignal(SignalName.DamageDealt, info.Source, info.Target, result.FinalDamage);
            EmitSignal(SignalName.DamageTaken, info.Target, result.FinalDamage);

            if (result.WasCritical)
            {
                EmitSignal(SignalName.CriticalHit, info.Source, info.Target, result.FinalDamage);
            }

            GD.Print($"[DamageSystem] {result.OriginalDamage} -> {result.FinalDamage} damage (blocked: {result.DamageBlocked}, crit: {result.WasCritical})");

            return result;
        }

        private void ApplyKnockback(CharacterBody2D target, Vector2 direction, float force)
        {
            if (target == null)
                return;

            var knockbackVelocity = direction.Normalized() * force * KnockbackMultiplier;
            target.Velocity = knockbackVelocity;

            GD.Print($"[DamageSystem] Knockback applied: {knockbackVelocity}");
        }

        public DamageInfo CreateDamageInfo(int amount, DamageType type = DamageType.Physical, Node source = null)
        {
            return new DamageInfo(amount, type)
            {
                Source = source
            };
        }

        public void AddDamageModifier(string modifierId)
        {
            if (!_damageModifiers.Contains(modifierId))
            {
                _damageModifiers.Add(modifierId);
                GD.Print($"[DamageSystem] Added damage modifier: {modifierId}");
            }
        }

        public void RemoveDamageModifier(string modifierId)
        {
            _damageModifiers.Remove(modifierId);
            GD.Print($"[DamageSystem] Removed damage modifier: {modifierId}");
        }

        public float GetDamageMultiplier(DamageType type)
        {
            float multiplier = 1.0f;

            foreach (var modifier in _damageModifiers)
            {
                // Apply modifier logic here
            }

            return multiplier;
        }
    }
}
