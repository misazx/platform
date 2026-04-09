using Godot;
using System.Collections.Generic;
using RoguelikeGame.Core;

namespace RoguelikeGame.Effects
{
    [GlobalClass]
    public partial class ParticleManager : SingletonBase<ParticleManager>
    {
        private readonly Dictionary<string, PackedScene> _effectTemplates = new();
        private readonly List<ParticleEffect> _activeEffects = new();
        private Node _particleLayer;

        protected override void OnInitialize()
        {
            _particleLayer = new Node { Name = "ParticleLayer" };
            AddChild(_particleLayer);
            RegisterBuiltInEffects();
        }

        private void RegisterBuiltInEffects()
        {
            RegisterEffect("slash_attack", CreateSlashEffect());
            RegisterEffect("magic_cast", CreateMagicCastEffect());
            RegisterEffect("enemy_death", CreateDeathEffect());
            RegisterEffect("block_spark", CreateBlockSparkEffect());
            RegisterEffect("heal_glow", CreateHealGlowEffect());
            RegisterEffect("damage_burst", CreateDamageBurstEffect());
            RegisterEffect("card_play", CreateCardPlayEffect());
            RegisterEffect("relic_activate", CreateRelicActivateEffect());
            RegisterEffect("gold_pickup", CreateGoldPickupEffect());
        }

        private void RegisterEffect(string name, PackedScene scene)
        {
            _effectTemplates[name] = scene;
        }

        public void SpawnEffect(string effectName, Vector2 position, float scale = 1f)
        {
            if (!_effectTemplates.TryGetValue(effectName, out var template))
            {
                GD.PushWarning($"[Particles] Unknown effect: {effectName}");
                return;
            }

            var instance = (ParticleEffect)template.Instantiate();
            instance.Position = position;
            instance.Scale = new Vector2(scale, scale);
            _particleLayer.AddChild(instance);
            _activeEffects.Add(instance);

            instance.Finished += () =>
            {
                _activeEffects.Remove(instance);
                instance.QueueFree();
            };

            instance.Play();
        }

        public void SpawnSlash(Vector2 position, float rotation = 0f)
        {
            SpawnEffect("slash_attack", position, 1f);
            var main = _activeEffects.Count > 0 ? _activeEffects[^1] : null;
            if (main != null)
                main.Rotation = rotation;
        }

        public void SpawnMagicCast(Vector2 position)
        {
            SpawnEffect("magic_cast", position);
        }

        public void SpawnEnemyDeath(Vector2 position)
        {
            SpawnEffect("enemy_death", position, 1.2f);
        }

        public void SpawnBlockSpark(Vector2 position)
        {
            SpawnEffect("block_spark", position, 0.8f);
        }

        public void SpawnHeal(Vector2 position)
        {
            SpawnEffect("heal_glow", position);
        }

        public void SpawnDamageBurst(Vector2 position, int damage)
        {
            SpawnEffect("damage_burst", position, Mathf.Clamp(damage / 10f, 0.5f, 2f));
        }

        private PackedScene CreateSlashEffect()
        {
            var scene = new PackedScene();
            var effect = new SlashEffect();
            scene.Pack(effect);
            return scene;
        }

        private PackedScene CreateMagicCastEffect()
        {
            var scene = new PackedScene();
            var effect = new MagicCastEffect();
            scene.Pack(effect);
            return scene;
        }

        private PackedScene CreateDeathEffect()
        {
            var scene = new PackedScene();
            var effect = new DeathEffect();
            scene.Pack(effect);
            return scene;
        }

        private PackedScene CreateBlockSparkEffect()
        {
            var scene = new PackedScene();
            var effect = new BlockSparkEffect();
            scene.Pack(effect);
            return scene;
        }

        private PackedScene CreateHealGlowEffect()
        {
            var scene = new PackedScene();
            var effect = new HealGlowEffect();
            scene.Pack(effect);
            return scene;
        }

        private PackedScene CreateDamageBurstEffect()
        {
            var scene = new PackedScene();
            var effect = new DamageBurstEffect();
            scene.Pack(effect);
            return scene;
        }

        private PackedScene CreateCardPlayEffect()
        {
            var scene = new PackedScene();
            var effect = new CardPlayEffect();
            scene.Pack(effect);
            return scene;
        }

        private PackedScene CreateRelicActivateEffect()
        {
            var scene = new PackedScene();
            var effect = new RelicActivateEffect();
            scene.Pack(effect);
            return scene;
        }

        private PackedScene CreateGoldPickupEffect()
        {
            var scene = new PackedScene();
            var effect = new GoldPickupEffect();
            scene.Pack(effect);
            return scene;
        }
    }

    [GlobalClass]
    public partial class ParticleEffect : Node2D
    {
        public event System.Action Finished;
        protected float Duration { get; set; } = 0.5f;
        protected float Elapsed { get; set; }

        public virtual void Play()
        {
            SetProcess(true);
        }

        public override void _Process(double delta)
        {
            Elapsed += (float)delta;
            if (Elapsed >= Duration)
            {
                SetProcess(false);
                Finished?.Invoke();
            }
        }
    }

    [GlobalClass]
    public partial class SlashEffect : ParticleEffect
    {
        private Line2D _slashLine;

        public override void _Ready()
        {
            Duration = 0.25f;
            _slashLine = new Line2D
            {
                Width = 3f,
                DefaultColor = Colors.White,
                ZIndex = 50
            };
            AddChild(_slashLine);

            var center = new Vector2(40, 0);
            _slashLine.AddPoint(center + new Vector2(-30, -40));
            _slashLine.AddPoint(center + new Vector2(30, 40));
            Position -= center;
        }

        public override void Play()
        {
            base.Play();
            var tween = CreateTween();
            tween.TweenProperty(_slashLine, "modulate:a", 0f, Duration).SetEase(Tween.EaseType.In);
        }
    }

    [GlobalClass]
    public partial class MagicCastEffect : ParticleEffect
    {
        public override void _Ready()
        {
            Duration = 0.6f;
            for (int i = 0; i < 8; i++)
            {
                var particle = new ColorRect
                {
                    Color = new Color(0.5f, 0.5f, 1f, 0.8f),
                    Position = new Vector2(GD.Randf() * 60 - 30, GD.Randf() * 60 - 30),
                    Size = new Vector2(4 + GD.Randf() * 6, 4 + GD.Randf() * 6),
                    Rotation = GD.Randf() * Mathf.Tau,
                    ZIndex = 10
                };
                AddChild(particle);

                var angle = Mathf.Atan2(particle.Position.Y, particle.Position.X);
                var dist = (float)(GD.Randi() % (70 - 30 + 1)) + 30;
                var tween = particle.CreateTween().SetLoops();
                tween.SetParallel(true);
                tween.TweenProperty(particle, "position:x", particle.Position.X + Mathf.Cos(angle) * dist, Duration);
                tween.TweenProperty(particle, "position:y", particle.Position.Y + Mathf.Sin(angle) * dist, Duration);
                tween.TweenProperty(particle, "modulate:a", 0f, Duration);
                tween.Chain().TweenCallback(Callable.From(particle.QueueFree));
            }

            var core = new ColorRect
            {
                Color = new Color(0.6f, 0.5f, 1f, 0.4f),
                Position = new Vector2(-15, -15),
                Size = new Vector2(30, 30),
                ZIndex = 5
            };
            AddChild(core);
            var coreTween = core.CreateTween();
            coreTween.TweenProperty(core, "size", new Vector2(80, 80), Duration * 0.5f).SetEase(Tween.EaseType.Out);
            coreTween.TweenProperty(core, "modulate:a", 0f, Duration).SetDelay(Duration * 0.5f);
            coreTween.Chain().TweenCallback(Callable.From(core.QueueFree));
        }
    }

    [GlobalClass]
    public partial class DeathEffect : ParticleEffect
    {
        public override void _Ready()
        {
            Duration = 0.8f;
            for (int i = 0; i < 12; i++)
            {
                var particle = new ColorRect
                {
                    Color = new Color(1f, 0.3f, 0.2f, 0.9f),
                    Position = new Vector2(GD.Randf() * 40 - 20, GD.Randf() * 40 - 20),
                    Size = new Vector2(3 + GD.Randf() * 5, 3 + GD.Randf() * 5),
                    Rotation = GD.Randf() * Mathf.Tau,
                    ZIndex = 10
                };
                AddChild(particle);

                var outward = particle.Position.Normalized() * (float)((GD.Randi() % (90 - 40 + 1)) + 40);
                var tween = particle.CreateTween().SetLoops();
                tween.SetParallel(true);
                tween.TweenProperty(particle, "position", particle.Position + outward, Duration);
                tween.TweenProperty(particle, "modulate:a", 0f, Duration);
                tween.TweenProperty(particle, "size", Vector2.Zero, Duration);
                tween.Chain().TweenCallback(Callable.From(particle.QueueFree));
            }

            var burst = new ColorRect
            {
                Color = new Color(1f, 0.4f, 0.3f, 0.6f),
                Position = new Vector2(-20, -20),
                Size = new Vector2(40, 40),
                ZIndex = 5
            };
            AddChild(burst);
            var bTween = burst.CreateTween();
            bTween.TweenProperty(burst, "size", new Vector2(120, 120), Duration * 0.4f).SetEase(Tween.EaseType.Out);
            bTween.TweenProperty(burst, "modulate:a", 0f, Duration).SetDelay(Duration * 0.4f);
            bTween.Chain().TweenCallback(Callable.From(burst.QueueFree));
        }
    }

    [GlobalClass]
    public partial class BlockSparkEffect : ParticleEffect
    {
        public override void _Ready()
        {
            Duration = 0.35f;
            for (int i = 0; i < 6; i++)
            {
                var spark = new ColorRect
                {
                    Color = new Color(0.4f, 0.6f, 1f, 1f),
                    Position = new Vector2(GD.Randf() * 30 - 15, GD.Randf() * 30 - 15),
                    Size = new Vector2(2, 6 + GD.Randf() * 4),
                    Rotation = GD.Randf() * Mathf.Pi,
                    ZIndex = 10
                };
                AddChild(spark);

                var tween = spark.CreateTween();
                tween.SetParallel(true);
                tween.TweenProperty(spark, "position:y", spark.Position.Y - (float)(GD.Randi() % (45 - 20 + 1)) + 20, Duration);
                tween.TweenProperty(spark, "modulate:a", 0f, Duration * 0.8f);
                tween.Chain().TweenCallback(Callable.From(spark.QueueFree));
            }
        }
    }

    [GlobalClass]
    public partial class HealGlowEffect : ParticleEffect
    {
        public override void _Ready()
        {
            Duration = 0.7f;
            var glow = new ColorRect
            {
                Color = new Color(0.3f, 1f, 0.4f, 0.5f),
                Position = new Vector2(-25, -25),
                Size = new Vector2(50, 50),
                ZIndex = 5
            };
            AddChild(glow);

            var tween = glow.CreateTween();
            tween.SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Sine);
            tween.TweenProperty(glow, "size", new Vector2(100, 100), Duration * 0.5f);
            tween.TweenProperty(glow, "modulate:a", 0f, Duration * 0.5f).SetDelay(Duration * 0.3f);
            tween.Chain().TweenCallback(Callable.From(glow.QueueFree));

            for (int i = 0; i < 5; i++)
            {
                var rise = new ColorRect
                {
                    Color = new Color(0.4f, 1f, 0.5f, 0.8f),
                    Position = new Vector2(GD.Randf() * 40 - 20, GD.Randf() * 20),
                    Size = new Vector2(3, 8 + GD.Randf() * 6),
                    ZIndex = 10
                };
                AddChild(rise);
                var rtween = rise.CreateTween();
                rtween.TweenProperty(rise, "position:y", rise.Position.Y - (float)(GD.Randi() % (60 - 30 + 1)) + 30, Duration);
                rtween.TweenProperty(rise, "modulate:a", 0f, Duration);
                rtween.Chain().TweenCallback(Callable.From(rise.QueueFree));
            }
        }
    }

    [GlobalClass]
    public partial class DamageBurstEffect : ParticleEffect
    {
        public override void _Ready()
        {
            Duration = 0.4f;
            var burst = new ColorRect
            {
                Color = new Color(1f, 0.25f, 0.2f, 0.7f),
                Position = new Vector2(-20, -20),
                Size = new Vector2(40, 40),
                ZIndex = 5
            };
            AddChild(burst);

            var tween = burst.CreateTween();
            tween.TweenProperty(burst, "size", new Vector2(90, 90), Duration * 0.4f).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(burst, "modulate:a", 0f, Duration);
            tween.Chain().TweenCallback(Callable.From(burst.QueueFree));
        }
    }

    [GlobalClass]
    public partial class CardPlayEffect : ParticleEffect
    {
        public override void _Ready()
        {
            Duration = 0.3f;
            var flash = new ColorRect
            {
                Color = new Color(1f, 1f, 1f, 0.3f),
                Position = new Vector2(-25, -30),
                Size = new Vector2(50, 70),
                ZIndex = 5
            };
            AddChild(flash);

            var tween = flash.CreateTween();
            tween.TweenProperty(flash, "modulate:a", 0f, Duration);
            tween.Chain().TweenCallback(Callable.From(flash.QueueFree));
        }
    }

    [GlobalClass]
    public partial class RelicActivateEffect : ParticleEffect
    {
        public override void _Ready()
        {
            Duration = 0.6f;
            var ring = new ColorRect
            {
                Color = new Color(1f, 0.85f, 0.3f, 0.5f),
                Position = new Vector2(-30, -30),
                Size = new Vector2(60, 60),
                ZIndex = 5
            };
            AddChild(ring);

            var tween = ring.CreateTween();
            tween.TweenProperty(ring, "size", new Vector2(110, 110), Duration * 0.5f).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(ring, "modulate:a", 0f, Duration * 0.5f).SetDelay(Duration * 0.3f);
            tween.Chain().TweenCallback(Callable.From(ring.QueueFree));

            for (int i = 0; i < 8; i++)
            {
                var star = new ColorRect
                {
                    Color = new Color(1f, 0.92f, 0.4f, 0.9f),
                    Position = new Vector2(Mathf.Cos(i * Mathf.Pi / 4) * 35, Mathf.Sin(i * Mathf.Pi / 4) * 35),
                    Size = new Vector2(4, 4),
                    Rotation = i * Mathf.Pi / 4,
                    ZIndex = 10
                };
                AddChild(star);
                var sTween = star.CreateTween();
                var dir = star.Position.Normalized() * 40;
                sTween.TweenProperty(star, "position", star.Position + dir, Duration);
                sTween.TweenProperty(star, "modulate:a", 0f, Duration);
                sTween.Chain().TweenCallback(Callable.From(star.QueueFree));
            }
        }
    }

    [GlobalClass]
    public partial class GoldPickupEffect : ParticleEffect
    {
        public override void _Ready()
        {
            Duration = 0.5f;
            var coin = new ColorRect
            {
                Color = new Color(1f, 0.85f, 0.2f),
                Position = new Vector2(-12, -12),
                Size = new Vector2(24, 24),
                ZIndex = 10
            };
            AddChild(coin);

            var tween = coin.CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(coin, "position:y", coin.Position.Y - 40, Duration).SetEase(Tween.EaseType.Out);
            tween.TweenProperty(coin, "modulate:a", 0f, Duration * 0.8f);
            tween.Chain().TweenCallback(Callable.From(coin.QueueFree));
        }
    }
}
