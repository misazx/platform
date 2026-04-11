using Godot;
using System;
using System.Collections.Generic;
using RoguelikeGame.Core;

namespace RoguelikeGame.Systems
{
    public enum SkillType
    {
        Active,
        Passive,
        Toggle
    }

    public enum TargetType
    {
        Self,
        SingleEnemy,
        AreaOfEffect,
        Direction,
        Position
    }

    public class SkillData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SkillType Type { get; set; }
        public TargetType TargetType { get; set; }
        public float Cooldown { get; set; }
        public float ManaCost { get; set; }
        public int MaxLevel { get; set; } = 5;
        public Dictionary<int, Dictionary<string, float>> LevelStats { get; set; } = new();
        public string IconPath { get; set; }
        public string EffectPath { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    public class SkillInstance
    {
        public string SkillId { get; set; }
        public int Level { get; set; } = 1;
        public float CurrentCooldown { get; set; }
        public bool IsUnlocked { get; set; }
        public bool IsActive { get; set; }

        public bool IsReady => CurrentCooldown <= 0 && IsUnlocked;
    }

    public abstract class SkillEffect
    {
        public abstract void Execute(Node caster, Node target, SkillData skill, int level);
        public abstract void OnLearn(Node owner, SkillData skill, int level);
        public abstract void OnForget(Node owner, SkillData skill, int level);
    }

    public partial class SkillManager : SingletonBase<SkillManager>
    {
        private readonly Dictionary<string, SkillData> _skillDefinitions = new();
        private readonly Dictionary<string, SkillInstance> _learnedSkills = new();
        private readonly Dictionary<string, SkillEffect> _skillEffects = new();

        [Export]
        public int MaxSkillSlots { get; set; } = 4;

        [Signal]
        public delegate void SkillLearnedEventHandler(string skillId, int level);

        [Signal]
        public delegate void SkillUsedEventHandler(string skillId, Node caster);

        [Signal]
        public delegate void SkillCooldownStartedEventHandler(string skillId, float duration);

        protected override void OnInitialize()
        {
            LoadSkillDefinitions();
        }

        private void LoadSkillDefinitions()
        {
            RegisterSkill(new SkillData
            {
                Id = "fireball",
                Name = "Fireball",
                Description = "Launch a fireball dealing damage",
                Type = SkillType.Active,
                TargetType = TargetType.Direction,
                Cooldown = 2.0f,
                ManaCost = 20f,
                LevelStats = new Dictionary<int, Dictionary<string, float>>
                {
                    { 1, new Dictionary<string, float> { { "damage", 30f }, { "speed", 400f } } },
                    { 2, new Dictionary<string, float> { { "damage", 45f }, { "speed", 450f } } },
                    { 3, new Dictionary<string, float> { { "damage", 60f }, { "speed", 500f } } },
                    { 4, new Dictionary<string, float> { { "damage", 80f }, { "speed", 550f } } },
                    { 5, new Dictionary<string, float> { { "damage", 100f }, { "speed", 600f } } }
                },
                IconPath = "res://GameModes/base_game/Resources/Icons/Skills/fireball.png",
                Tags = new List<string> { "fire", "projectile", "damage" }
            });

            RegisterSkill(new SkillData
            {
                Id = "heal",
                Name = "Heal",
                Description = "Restore health points",
                Type = SkillType.Active,
                TargetType = TargetType.Self,
                Cooldown = 10.0f,
                ManaCost = 30f,
                LevelStats = new Dictionary<int, Dictionary<string, float>>
                {
                    { 1, new Dictionary<string, float> { { "heal_amount", 25f } } },
                    { 2, new Dictionary<string, float> { { "heal_amount", 40f } } },
                    { 3, new Dictionary<string, float> { { "heal_amount", 60f } } },
                    { 4, new Dictionary<string, float> { { "heal_amount", 85f } } },
                    { 5, new Dictionary<string, float> { { "heal_amount", 100f } } }
                },
                IconPath = "res://GameModes/base_game/Resources/Icons/Skills/heal.png",
                Tags = new List<string> { "healing", "self" }
            });

            RegisterSkill(new SkillData
            {
                Id = "dash",
                Name = "Dash",
                Description = "Quickly dash forward",
                Type = SkillType.Active,
                TargetType = TargetType.Direction,
                Cooldown = 3.0f,
                ManaCost = 10f,
                LevelStats = new Dictionary<int, Dictionary<string, float>>
                {
                    { 1, new Dictionary<string, float> { { "distance", 150f }, { "speed", 600f } } },
                    { 2, new Dictionary<string, float> { { "distance", 180f }, { "speed", 650f } } },
                    { 3, new Dictionary<string, float> { { "distance", 220f }, { "speed", 700f } } },
                    { 4, new Dictionary<string, float> { { "distance", 260f }, { "speed", 750f } } },
                    { 5, new Dictionary<string, float> { { "distance", 300f }, { "speed", 800f } } }
                },
                IconPath = "res://GameModes/base_game/Resources/Icons/Skills/dash.png",
                Tags = new List<string> { "movement", "utility" }
            });

            RegisterSkill(new SkillData
            {
                Id = "iron_skin",
                Name = "Iron Skin",
                Description = "Increase defense passively",
                Type = SkillType.Passive,
                TargetType = TargetType.Self,
                Cooldown = 0f,
                ManaCost = 0f,
                LevelStats = new Dictionary<int, Dictionary<string, float>>
                {
                    { 1, new Dictionary<string, float> { { "defense_bonus", 5f } } },
                    { 2, new Dictionary<string, float> { { "defense_bonus", 10f } } },
                    { 3, new Dictionary<string, float> { { "defense_bonus", 15f } } },
                    { 4, new Dictionary<string, float> { { "defense_bonus", 20f } } },
                    { 5, new Dictionary<string, float> { { "defense_bonus", 30f } } }
                },
                IconPath = "res://GameModes/base_game/Resources/Icons/Skills/iron_skin.png",
                Tags = new List<string> { "passive", "defense" }
            });

            GD.Print($"[SkillManager] Loaded {_skillDefinitions.Count} skill definitions");
        }

        public void RegisterSkill(SkillData skillData)
        {
            _skillDefinitions[skillData.Id] = skillData;
        }

        public SkillData GetSkillData(string skillId)
        {
            return _skillDefinitions.TryGetValue(skillId, out var data) ? data : null;
        }

        public bool LearnSkill(string skillId, Node owner)
        {
            if (_learnedSkills.ContainsKey(skillId))
            {
                GD.Print($"[SkillManager] Skill already learned: {skillId}");
                return false;
            }

            var skillData = GetSkillData(skillId);
            if (skillData == null)
            {
                GD.PrintErr($"[SkillManager] Skill not found: {skillId}");
                return false;
            }

            var instance = new SkillInstance
            {
                SkillId = skillId,
                Level = 1,
                IsUnlocked = true,
                CurrentCooldown = 0
            };

            _learnedSkills[skillId] = instance;

            if (skillData.Type == SkillType.Passive)
            {
                ApplyPassiveEffect(owner, skillData, instance.Level);
            }

            EmitSignal(SignalName.SkillLearned, skillId, instance.Level);
            GD.Print($"[SkillManager] Learned skill: {skillData.Name}");

            return true;
        }

        public bool UpgradeSkill(string skillId, Node owner)
        {
            if (!_learnedSkills.TryGetValue(skillId, out var instance))
            {
                GD.PrintErr($"[SkillManager] Skill not learned: {skillId}");
                return false;
            }

            var skillData = GetSkillData(skillId);
            if (skillData == null)
                return false;

            if (instance.Level >= skillData.MaxLevel)
            {
                GD.Print($"[SkillManager] Skill at max level: {skillId}");
                return false;
            }

            if (skillData.Type == SkillType.Passive)
            {
                RemovePassiveEffect(owner, skillData, instance.Level);
            }

            instance.Level++;

            if (skillData.Type == SkillType.Passive)
            {
                ApplyPassiveEffect(owner, skillData, instance.Level);
            }

            EmitSignal(SignalName.SkillLearned, skillId, instance.Level);
            GD.Print($"[SkillManager] Upgraded {skillData.Name} to level {instance.Level}");

            return true;
        }

        public bool UseSkill(string skillId, Node caster, Node target = null, Vector2? direction = null)
        {
            if (!_learnedSkills.TryGetValue(skillId, out var instance))
            {
                GD.PrintErr($"[SkillManager] Skill not learned: {skillId}");
                return false;
            }

            if (!instance.IsReady)
            {
                GD.Print($"[SkillManager] Skill on cooldown: {skillId}");
                return false;
            }

            var skillData = GetSkillData(skillId);
            if (skillData == null)
                return false;

            if (skillData.Type == SkillType.Passive)
            {
                GD.Print($"[SkillManager] Cannot use passive skill: {skillId}");
                return false;
            }

            if (caster.HasMethod("GetMana"))
            {
                float currentMana = (float)caster.Call("GetMana");
                if (currentMana < skillData.ManaCost)
                {
                    GD.Print($"[SkillManager] Not enough mana for {skillData.Name}");
                    return false;
                }

                caster.Call("UseMana", skillData.ManaCost);
            }

            ExecuteSkillEffect(skillData, caster, target, direction, instance.Level);

            instance.CurrentCooldown = skillData.Cooldown;

            EmitSignal(SignalName.SkillUsed, skillId, caster);
            EmitSignal(SignalName.SkillCooldownStarted, skillId, skillData.Cooldown);

            GD.Print($"[SkillManager] Used skill: {skillData.Name}");

            return true;
        }

        private void ExecuteSkillEffect(SkillData skill, Node caster, Node target, Vector2? direction, int level)
        {
            var stats = skill.LevelStats.ContainsKey(level) ? skill.LevelStats[level] : new Dictionary<string, float>();

            switch (skill.Id)
            {
                case "fireball":
                    ExecuteFireball(caster, direction ?? Vector2.Right, stats);
                    break;
                case "heal":
                    ExecuteHeal(caster, stats);
                    break;
                case "dash":
                    ExecuteDash(caster, direction ?? Vector2.Right, stats);
                    break;
                default:
                    GD.Print($"[SkillManager] No effect defined for skill: {skill.Id}");
                    break;
            }
        }

        private void ExecuteFireball(Node caster, Vector2 direction, Dictionary<string, float> stats)
        {
            GD.Print($"[SkillManager] Fireball! Damage: {stats.GetValueOrDefault("damage", 30f)}");
        }

        private void ExecuteHeal(Node caster, Dictionary<string, float> stats)
        {
            float healAmount = stats.GetValueOrDefault("heal_amount", 25f);
            if (caster.HasMethod("Heal"))
            {
                caster.Call("Heal", (int)healAmount);
            }
            GD.Print($"[SkillManager] Healed for {healAmount}");
        }

        private void ExecuteDash(Node caster, Vector2 direction, Dictionary<string, float> stats)
        {
            float distance = stats.GetValueOrDefault("distance", 150f);
            GD.Print($"[SkillManager] Dash! Distance: {distance}");
        }

        private void ApplyPassiveEffect(Node owner, SkillData skill, int level)
        {
            var stats = skill.LevelStats.ContainsKey(level) ? skill.LevelStats[level] : new Dictionary<string, float>();

            foreach (var stat in stats)
            {
                if (owner.HasMethod("ModifyStat"))
                {
                    owner.Call("ModifyStat", stat.Key, stat.Value, false);
                }
            }

            GD.Print($"[SkillManager] Applied passive {skill.Name} level {level}");
        }

        private void RemovePassiveEffect(Node owner, SkillData skill, int level)
        {
            var stats = skill.LevelStats.ContainsKey(level) ? skill.LevelStats[level] : new Dictionary<string, float>();

            foreach (var stat in stats)
            {
                if (owner.HasMethod("ModifyStat"))
                {
                    owner.Call("ModifyStat", stat.Key, -stat.Value, false);
                }
            }
        }

        public override void _Process(double delta)
        {
            var dt = (float)delta;

            foreach (var instance in _learnedSkills.Values)
            {
                if (instance.CurrentCooldown > 0)
                {
                    instance.CurrentCooldown -= dt;
                }
            }
        }

        public float GetCooldownRemaining(string skillId)
        {
            return _learnedSkills.TryGetValue(skillId, out var instance) ? instance.CurrentCooldown : 0f;
        }

        public bool IsSkillReady(string skillId)
        {
            return _learnedSkills.TryGetValue(skillId, out var instance) && instance.IsReady;
        }

        public List<SkillInstance> GetLearnedSkills()
        {
            return new List<SkillInstance>(_learnedSkills.Values);
        }
    }
}
