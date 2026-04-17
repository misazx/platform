namespace RoguelikeGame.Shared.BehaviorTree
{
    public class BTCondition : BTNode
    {
        private readonly Func<BTContext, bool> _condition;

        public BTCondition(Func<BTContext, bool> condition, string name = "")
        {
            _condition = condition;
            Name = name;
        }

        public override BTNodeStatus Execute(BTContext context)
        {
            bool result = _condition(context);
            LastStatus = result ? BTNodeStatus.Success : BTNodeStatus.Failure;
            return LastStatus;
        }
    }
}
