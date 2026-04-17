namespace RoguelikeGame.Shared.BehaviorTree
{
    public class Blackboard
    {
        private readonly Dictionary<string, object> _data = new();
        private readonly Dictionary<string, double> _timers = new();

        public void Set<T>(string key, T value)
        {
            _data[key] = value!;
        }

        public T? Get<T>(string key)
        {
            if (_data.TryGetValue(key, out var value) && value is T typed)
            {
                return typed;
            }
            return default;
        }

        public bool Has(string key)
        {
            return _data.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return _data.Remove(key);
        }

        public void Clear()
        {
            _data.Clear();
            _timers.Clear();
        }

        public void SetTimer(string key, double duration)
        {
            _timers[key] = duration;
        }

        public bool IsTimerExpired(string key, double deltaTime)
        {
            if (!_timers.ContainsKey(key)) return true;

            _timers[key] -= deltaTime;
            if (_timers[key] <= 0)
            {
                _timers.Remove(key);
                return true;
            }
            return false;
        }

        public double GetTimerRemaining(string key)
        {
            return _timers.GetValueOrDefault(key, 0);
        }
    }
}
