using System;

namespace GameAI.BT
{
    public enum NodeState { Success, Failure, Running }

    /// <summary>Base contract for every node in the tree.</summary>
    public interface IBehaviorNode
    {
        NodeState Tick(Blackboard bb);
        void Reset(); // clears any internal running-state (e.g. cooldown timers, sequence index)
    }

    public abstract class BehaviorNode : IBehaviorNode
    {
        public abstract NodeState Tick(Blackboard bb);
        public virtual void Reset() { }
    }

    /// <summary>Leaf node wrapping a plain function — for quick one-off actions.</summary>
    public class ActionNode : BehaviorNode
    {
        private readonly Func<Blackboard, NodeState> _action;
        public ActionNode(Func<Blackboard, NodeState> action) => _action = action;
        public override NodeState Tick(Blackboard bb) => _action(bb);
    }

    /// <summary>Leaf node that checks a boolean condition. Never returns Running.</summary>
    public class ConditionNode : BehaviorNode
    {
        private readonly Func<Blackboard, bool> _condition;
        public ConditionNode(Func<Blackboard, bool> condition) => _condition = condition;
        public override NodeState Tick(Blackboard bb) => _condition(bb) ? NodeState.Success : NodeState.Failure;
    }
}