using System.Collections.Generic;

public class Selector : Node
{
    protected List<Node> children = new List<Node>();

    public Selector(List<Node> nodes) { children = nodes; }

    public override NodeStatus Tick()
    {
        foreach (var child in children)
        {
            switch (child.Tick())
            {
                case NodeStatus.SUCCESS: return NodeStatus.SUCCESS;
                case NodeStatus.RUNNING: return NodeStatus.RUNNING;
                case NodeStatus.FAILURE: continue;
            }
        }
        return NodeStatus.FAILURE;
    }
}