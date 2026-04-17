namespace RoguelikeGame.Shared.BehaviorTree
{
    public class BTContext
    {
        public Blackboard Blackboard { get; }
        public double DeltaTime { get; }
        public ulong TickCount { get; }

        public BTContext(Blackboard blackboard, double deltaTime, ulong tickCount)
        {
            Blackboard = blackboard;
            DeltaTime = deltaTime;
            TickCount = tickCount;
        }
    }
}
