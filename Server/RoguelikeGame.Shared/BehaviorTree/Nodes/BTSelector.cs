namespace RoguelikeGame.Shared.BehaviorTree
{
    public class BTSelector : BTNode
    {
        private readonly List<BTNode> _children = new();
        private int _currentChildIndex;

        public BTSelector(string name = "")
        {
            Name = name;
        }

        public BTSelector AddChild(BTNode child)
        {
            _children.Add(child);
            return this;
        }

        public override BTNodeStatus Execute(BTContext context)
        {
            for (int i = _currentChildIndex; i < _children.Count; i++)
            {
                var child = _children[i];
                var status = child.Execute(context);

                switch (status)
                {
                    case BTNodeStatus.Success:
                        _currentChildIndex = 0;
                        LastStatus = BTNodeStatus.Success;
                        return BTNodeStatus.Success;

                    case BTNodeStatus.Running:
                        _currentChildIndex = i;
                        LastStatus = BTNodeStatus.Running;
                        return BTNodeStatus.Running;

                    case BTNodeStatus.Failure:
                        continue;
                }
            }

            _currentChildIndex = 0;
            LastStatus = BTNodeStatus.Failure;
            return BTNodeStatus.Failure;
        }

        public override void Reset()
        {
            _currentChildIndex = 0;
            foreach (var child in _children)
            {
                child.Reset();
            }
            base.Reset();
        }

        public override void Abort()
        {
            if (_currentChildIndex < _children.Count)
            {
                _children[_currentChildIndex].Abort();
            }
            _currentChildIndex = 0;
            base.Abort();
        }
    }
}
