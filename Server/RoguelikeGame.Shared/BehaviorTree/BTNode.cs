namespace RoguelikeGame.Shared.BehaviorTree
{
    public enum BTNodeStatus
    {
        Success,
        Failure,
        Running
    }

    public abstract class BTNode
    {
        public string Name { get; set; } = "";
        public BTNodeStatus LastStatus { get; protected set; } = BTNodeStatus.Failure;

        public abstract BTNodeStatus Execute(BTContext context);

        public virtual void Reset()
        {
            LastStatus = BTNodeStatus.Failure;
        }

        public virtual void Abort()
        {
            LastStatus = BTNodeStatus.Failure;
        }
    }
}
