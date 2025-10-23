using System.Collections.Generic;

public class SequenceNode : BTNode
{
    private List<BTNode> children;
    public SequenceNode(params BTNode[] nodes) { children = new List<BTNode>(nodes); }
    public override NodeState Tick()
    {
        foreach (var child in children)
        {
            var state = child.Tick();
            if (state == NodeState.FAILURE) return NodeState.FAILURE;
            if (state == NodeState.RUNNING) return NodeState.RUNNING;
        }
        return NodeState.SUCCESS;
    }
}

public class SelectorNode : BTNode
{
    private List<BTNode> children;
    public SelectorNode(params BTNode[] nodes) { children = new List<BTNode>(nodes); }
    public override NodeState Tick()
    {
        foreach (var child in children)
        {
            var state = child.Tick();
            if (state == NodeState.SUCCESS) return NodeState.SUCCESS;
            if (state == NodeState.RUNNING) return NodeState.RUNNING;
        }
        return NodeState.FAILURE;
    }
}
