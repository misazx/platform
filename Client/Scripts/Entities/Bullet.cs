using Godot;

namespace RoguelikeGame.Entities
{
    public partial class Bullet : Node2D
    {
        [Export]
        public float Speed { get; set; } = 500f;

        [Export]
        public int Damage { get; set; } = 10;

        public Vector2 Velocity { get; set; } = Vector2.Zero;

        private Area2D _hitbox;

        public override void _Ready()
        {
            SetupHitbox();
        }

        private void SetupHitbox()
        {
            _hitbox = new Area2D();
            var collision = new CollisionShape2D();
            var shape = new CircleShape2D { Radius = 8f };
            collision.Shape = shape;
            _hitbox.AddChild(collision);
            AddChild(_hitbox);

            _hitbox.AreaEntered += OnAreaEntered;
            _hitbox.BodyEntered += OnBodyEntered;
        }

        public override void _Process(double delta)
        {
            if (Velocity != Vector2.Zero)
            {
                Position += Velocity * (float)delta;
            }

            if (Position.X < -100 || Position.X > 1380 ||
                Position.Y < -100 || Position.Y > 820)
            {
                QueueFree();
            }
        }

        private void OnAreaEntered(Area2D area)
        {
            if (area.IsInGroup("enemies"))
            {
                var parent = area.GetParent();
                if (parent.HasMethod("TakeDamage"))
                {
                    parent.Call("TakeDamage", Damage);
                }
                QueueFree();
            }
        }

        private void OnBodyEntered(Node2D body)
        {
            if (body.IsInGroup("enemies"))
            {
                if (body.HasMethod("TakeDamage"))
                {
                    body.Call("TakeDamage", Damage);
                }
                QueueFree();
            }
        }

        public void Launch(Vector2 direction)
        {
            Velocity = direction.Normalized() * Speed;
            Rotation = direction.Angle();
        }
    }
}
