using Godot;
using System;
using System.Collections.Generic;

namespace RoguelikeGame.Systems
{
    public class ObjectPool<T> where T : Node, new()
    {
        private readonly Queue<T> _pool = new();
        private readonly Func<T> _createFunc;
        private readonly Action<T> _resetAction;
        private readonly Action<T> _destroyAction;
        private readonly int _maxSize;
        private readonly string _poolName;

        public int Count => _pool.Count;
        public int MaxSize => _maxSize;

        public ObjectPool(
            string poolName,
            int initialSize,
            int maxSize,
            Func<T> createFunc = null,
            Action<T> resetAction = null,
            Action<T> destroyAction = null)
        {
            _poolName = poolName;
            _maxSize = maxSize;
            _createFunc = createFunc ?? (() => new T());
            _resetAction = resetAction;
            _destroyAction = destroyAction;

            for (int i = 0; i < initialSize; i++)
            {
                var obj = CreateObject();
                obj.SetProcess(false);
                obj.SetPhysicsProcess(false);
                if (obj is CanvasItem ci) ci.Visible = false;
                _pool.Enqueue(obj);
            }

            GD.Print($"[ObjectPool] Created pool '{_poolName}' with {initialSize} objects");
        }

        public T Get()
        {
            T obj;

            if (_pool.Count > 0)
            {
                obj = _pool.Dequeue();
            }
            else
            {
                obj = CreateObject();
                GD.Print($"[ObjectPool] Created new object in '{_poolName}'");
            }

            obj.SetProcess(true);
            obj.SetPhysicsProcess(true);
            if (obj is CanvasItem ci2) ci2.Visible = true;

            return obj;
        }

        public void Return(T obj)
        {
            if (obj == null)
                return;

            if (_pool.Count >= _maxSize)
            {
                _destroyAction?.Invoke(obj);
                obj.QueueFree();
                return;
            }

            _resetAction?.Invoke(obj);

            obj.SetProcess(false);
            obj.SetPhysicsProcess(false);
            if (obj is CanvasItem ci3) ci3.Visible = false;

            if (obj.IsInsideTree())
            {
                obj.GetParent()?.RemoveChild(obj);
            }

            _pool.Enqueue(obj);
        }

        public void Clear()
        {
            while (_pool.Count > 0)
            {
                var obj = _pool.Dequeue();
                _destroyAction?.Invoke(obj);
                obj.QueueFree();
            }

            GD.Print($"[ObjectPool] Cleared pool '{_poolName}'");
        }

        private T CreateObject()
        {
            var obj = _createFunc();
            obj.SetMeta("pool_name", _poolName);
            return obj;
        }
    }

    public partial class ObjectPoolManager : Node
    {
        public static ObjectPoolManager Instance { get; private set; }

        private readonly Dictionary<string, object> _pools = new();
        private readonly Dictionary<string, List<Node>> _activeObjects = new();

        [Signal]
        public delegate void ObjectCreatedEventHandler(string poolName, Node obj);

        [Signal]
        public delegate void ObjectReturnedEventHandler(string poolName, Node obj);

        public override void _Ready()
        {
            if (Instance != null && Instance != this)
            {
                QueueFree();
                return;
            }
            Instance = this;

            InitializeDefaultPools();
        }

        private void InitializeDefaultPools()
        {
            CreateBulletPool(50);
            CreateEffectPool(30);
            CreateDamageNumberPool(20);

            GD.Print($"[ObjectPoolManager] Initialized {_pools.Count} pools");
        }

        public void CreatePool<T>(
            string poolName,
            int initialSize,
            int maxSize,
            Func<T> createFunc = null,
            Action<T> resetAction = null,
            Action<T> destroyAction = null) where T : Node, new()
        {
            if (_pools.ContainsKey(poolName))
            {
                GD.PrintErr($"[ObjectPoolManager] Pool already exists: {poolName}");
                return;
            }

            var pool = new ObjectPool<T>(poolName, initialSize, maxSize, createFunc, resetAction, destroyAction);
            _pools[poolName] = pool;
            _activeObjects[poolName] = new List<Node>();

            GD.Print($"[ObjectPoolManager] Created pool: {poolName}");
        }

        public T Get<T>(string poolName) where T : Node, new()
        {
            if (!_pools.TryGetValue(poolName, out var poolObj))
            {
                GD.PrintErr($"[ObjectPoolManager] Pool not found: {poolName}");
                return null;
            }

            var pool = (ObjectPool<T>)poolObj;
            var obj = pool.Get();

            _activeObjects[poolName].Add(obj);

            EmitSignal(SignalName.ObjectCreated, poolName, obj);

            return obj;
        }

        public void Return<T>(string poolName, T obj) where T : Node, new()
        {
            if (!_pools.TryGetValue(poolName, out var poolObj))
            {
                GD.PrintErr($"[ObjectPoolManager] Pool not found: {poolName}");
                return;
            }

            var pool = (ObjectPool<T>)poolObj;
            pool.Return(obj);

            _activeObjects[poolName].Remove(obj);

            EmitSignal(SignalName.ObjectReturned, poolName, obj);
        }

        public void ReturnAll(string poolName)
        {
            if (!_activeObjects.TryGetValue(poolName, out var activeList))
                return;

            var objectsToReturn = new List<Node>(activeList);
            foreach (var obj in objectsToReturn)
            {
                if (IsInstanceValid(obj))
                {
                    obj.Call("ReturnToPool");
                }
            }

            activeList.Clear();
            GD.Print($"[ObjectPoolManager] Returned all objects in pool: {poolName}");
        }

        public void ClearPool(string poolName)
        {
            if (!_pools.TryGetValue(poolName, out var poolObj))
                return;

            ReturnAll(poolName);

            var poolType = poolObj.GetType();
            var clearMethod = poolType.GetMethod("Clear");
            clearMethod?.Invoke(poolObj, null);

            GD.Print($"[ObjectPoolManager] Cleared pool: {poolName}");
        }

        public void ClearAllPools()
        {
            foreach (var poolName in _pools.Keys)
            {
                ClearPool(poolName);
            }

            GD.Print("[ObjectPoolManager] Cleared all pools");
        }

        public int GetActiveCount(string poolName)
        {
            return _activeObjects.TryGetValue(poolName, out var list) ? list.Count : 0;
        }

        public int GetPoolSize(string poolName)
        {
            return _pools.TryGetValue(poolName, out var pool) ? (int)pool.GetType().GetProperty("Count")?.GetValue(pool) : 0;
        }

        private void CreateBulletPool(int size)
        {
            CreatePool<Node2D>(
                poolName: "bullets",
                initialSize: size,
                maxSize: size * 2,
                createFunc: () =>
                {
                    var bullet = new Node2D();
                    bullet.Name = "Bullet";
                    return bullet;
                },
                resetAction: (bullet) =>
                {
                    bullet.Position = Vector2.Zero;
                    bullet.Rotation = 0f;
                }
            );
        }

        private void CreateEffectPool(int size)
        {
            CreatePool<Node2D>(
                poolName: "effects",
                initialSize: size,
                maxSize: size * 2,
                createFunc: () =>
                {
                    var effect = new Node2D();
                    var particles = new GpuParticles2D();
                    particles.OneShot = true;
                    effect.AddChild(particles);
                    return effect;
                },
                resetAction: (effect) =>
                {
                    effect.Position = Vector2.Zero;
                    foreach (var child in effect.GetChildren())
                    {
                        if (child is GpuParticles2D particles)
                        {
                            particles.Restart();
                        }
                    }
                }
            );
        }

        private void CreateDamageNumberPool(int size)
        {
            CreatePool<Label>(
                poolName: "damage_numbers",
                initialSize: size,
                maxSize: size * 2,
                createFunc: () =>
                {
                    var label = new Label();
                    label.AddThemeFontSizeOverride("font_size", 20);
                    label.AddThemeColorOverride("font_color", Colors.Red);
                    return label;
                },
                resetAction: (label) =>
                {
                    label.Text = "";
                    label.Position = Vector2.Zero;
                    label.Modulate = Colors.White;
                }
            );
        }
    }
}
