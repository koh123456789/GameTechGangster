using System.Collections.Generic;

public class Sequence : Node
{
    protected List<Node> children = new List<Node>();
    public Sequence(List<Node> nodes) { children = nodes; }

    public override NodeStatus Tick()
    {
        foreach (var child in children)
        {
            NodeStatus childStatus = child.Tick();
            if (childStatus == NodeStatus.RUNNING) return NodeStatus.RUNNING;
            if (childStatus == NodeStatus.FAILURE) return NodeStatus.FAILURE;
            // Continue to next child if SUCCESS
        }
        return NodeStatus.SUCCESS;
    }
}