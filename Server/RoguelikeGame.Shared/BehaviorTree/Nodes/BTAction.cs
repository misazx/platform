namespace RoguelikeGame.Shared.BehaviorTree
{
    public class BTAction : BTNode
    {
        private readonly Func<BTContext, BTNodeStatus> _action;

        public BTAction(Func<BTContext, BTNodeStatus> action, string name = "")
        {
            _action = action;
            Name = name;
        }

        public override BTNodeStatus Execute(BTContext context)
        {
            LastStatus = _action(context);
            return LastStatus;
        }
    }
}
