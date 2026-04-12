using Godot;

namespace RoguelikeGame.Core
{
    public partial class SingletonBase<T> : Node where T : Node
    {
        private static T _instance;
        
        public static T Instance
        {
            get
            {
                if (_instance == null)
                {
                    var node = Engine.GetMainLoop() as SceneTree;
                    if (node != null)
                    {
                        var path = $"/root/{typeof(T).Name}";
                        _instance = node.Root.GetNodeOrNull<T>(path);
                    }
                }
                return _instance;
            }
        }

        public override void _Ready()
        {
            if (_instance != null && _instance != this)
            {
                GD.PrintErr($"[Singleton] Duplicate {typeof(T).Name} detected! Keeping original.");
                QueueFree();
                return;
            }
            _instance = (T)(object)this;
            ProcessMode = ProcessModeEnum.Always;
            
            OnInitialize();
        }

        public override void _ExitTree()
        {
            if (_instance == this)
                _instance = null;
        }

        protected virtual void OnInitialize() { }
    }
}
