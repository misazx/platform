namespace RoguelikeGame.Shared.BehaviorTree
{
    public class BTDecorator : BTNode
    {
        public enum DecoratorType
        {
            Inverter,
            Repeater,
            Delay,
            UntilFail
        }

        private readonly BTNode _child;
        private readonly DecoratorType _type;
        private readonly int _repeatCount;
        private readonly double _delaySeconds;
        private int _currentRepeat;
        private double _elapsedDelay;

        public BTDecorator(BTNode child, DecoratorType type, string name = "",
            int repeatCount = -1, double delaySeconds = 0)
        {
            _child = child;
            _type = type;
            _repeatCount = repeatCount;
            _delaySeconds = delaySeconds;
            Name = name;
        }

        public static BTDecorator Inverter(BTNode child, string name = "")
        {
            return new BTDecorator(child, DecoratorType.Inverter, name);
        }

        public static BTDecorator Repeater(BTNode child, int count = -1, string name = "")
        {
            return new BTDecorator(child, DecoratorType.Repeater, name, count);
        }

        public static BTDecorator Delay(BTNode child, double seconds, string name = "")
        {
            return new BTDecorator(child, DecoratorType.Delay, name, delaySeconds: seconds);
        }

        public static BTDecorator UntilFail(BTNode child, string name = "")
        {
            return new BTDecorator(child, DecoratorType.UntilFail, name);
        }

        public override BTNodeStatus Execute(BTContext context)
        {
            switch (_type)
            {
                case DecoratorType.Inverter:
                    return ExecuteInverter(context);
                case DecoratorType.Repeater:
                    return ExecuteRepeater(context);
                case DecoratorType.Delay:
                    return ExecuteDelay(context);
                case DecoratorType.UntilFail:
                    return ExecuteUntilFail(context);
                default:
                    LastStatus = BTNodeStatus.Failure;
                    return BTNodeStatus.Failure;
            }
        }

        private BTNodeStatus ExecuteInverter(BTContext context)
        {
            var status = _child.Execute(context);
            switch (status)
            {
                case BTNodeStatus.Success:
                    LastStatus = BTNodeStatus.Failure;
                    return BTNodeStatus.Failure;
                case BTNodeStatus.Failure:
                    LastStatus = BTNodeStatus.Success;
                    return BTNodeStatus.Success;
                default:
                    LastStatus = BTNodeStatus.Running;
                    return BTNodeStatus.Running;
            }
        }

        private BTNodeStatus ExecuteRepeater(BTContext context)
        {
            var status = _child.Execute(context);

            if (status == BTNodeStatus.Failure)
            {
                _currentRepeat = 0;
                _child.Reset();
                LastStatus = BTNodeStatus.Failure;
                return BTNodeStatus.Failure;
            }

            if (status == BTNodeStatus.Success)
            {
                _currentRepeat++;
                _child.Reset();

                if (_repeatCount > 0 && _currentRepeat >= _repeatCount)
                {
                    _currentRepeat = 0;
                    LastStatus = BTNodeStatus.Success;
                    return BTNodeStatus.Success;
                }

                LastStatus = BTNodeStatus.Running;
                return BTNodeStatus.Running;
            }

            LastStatus = BTNodeStatus.Running;
            return BTNodeStatus.Running;
        }

        private BTNodeStatus ExecuteDelay(BTContext context)
        {
            _elapsedDelay += context.DeltaTime;

            if (_elapsedDelay < _delaySeconds)
            {
                LastStatus = BTNodeStatus.Running;
                return BTNodeStatus.Running;
            }

            var status = _child.Execute(context);
            LastStatus = status;
            return status;
        }

        private BTNodeStatus ExecuteUntilFail(BTContext context)
        {
            var status = _child.Execute(context);

            if (status == BTNodeStatus.Failure)
            {
                LastStatus = BTNodeStatus.Success;
                return BTNodeStatus.Success;
            }

            LastStatus = BTNodeStatus.Running;
            return BTNodeStatus.Running;
        }

        public override void Reset()
        {
            _currentRepeat = 0;
            _elapsedDelay = 0;
            _child.Reset();
            base.Reset();
        }

        public override void Abort()
        {
            _child.Abort();
            _currentRepeat = 0;
            _elapsedDelay = 0;
            base.Abort();
        }
    }
}
