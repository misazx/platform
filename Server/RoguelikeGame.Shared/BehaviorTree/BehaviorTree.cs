namespace RoguelikeGame.Shared.BehaviorTree
{
    public class BehaviorTree
    {
        public string Name { get; set; } = "";
        public Blackboard Blackboard { get; } = new();
        public BTNode? Root { get; set; }

        private ulong _tickCount;
        private BTNodeStatus _lastStatus = BTNodeStatus.Failure;

        public BehaviorTree(string name = "")
        {
            Name = name;
        }

        public BTNodeStatus Tick(double deltaTime)
        {
            if (Root == null)
            {
                return BTNodeStatus.Failure;
            }

            _tickCount++;
            var context = new BTContext(Blackboard, deltaTime, _tickCount);
            _lastStatus = Root.Execute(context);
            return _lastStatus;
        }

        public void Reset()
        {
            Root?.Reset();
            _tickCount = 0;
            _lastStatus = BTNodeStatus.Failure;
        }

        public void Abort()
        {
            Root?.Abort();
            _tickCount = 0;
            _lastStatus = BTNodeStatus.Failure;
        }

        public void ClearBlackboard()
        {
            Blackboard.Clear();
        }
    }
}
