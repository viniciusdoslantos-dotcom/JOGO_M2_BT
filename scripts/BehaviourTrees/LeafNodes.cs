using System;
using UnityEngine;

// Action node - runs a function that returns NodeState
public class ActionNode : BTNode
{
    private Func<NodeState> action;
    public ActionNode(Func<NodeState> action) { this.action = action; }
    public override NodeState Tick() => action();
}

// Condition node - returns SUCCESS if condition true, else FAILURE
public class ConditionNode : BTNode
{
    private Func<bool> condition;
    public ConditionNode(Func<bool> condition) { this.condition = condition; }
    public override NodeState Tick() => condition() ? NodeState.SUCCESS : NodeState.FAILURE;
}
