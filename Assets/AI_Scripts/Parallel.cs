using System.Collections.Generic;

public class Parallel : Node
{
    protected List<Node> children = new List<Node>();
    public Parallel(List<Node> nodes) { children = nodes; }

    public override NodeStatus Tick()
    {
        int successCount = 0;
        int failureCount = 0;

        foreach (var child in children)
        {
            NodeStatus status = child.Tick();
            if (status == NodeStatus.SUCCESS) successCount++;
            if (status == NodeStatus.FAILURE) failureCount++;
        }

        // Example Policy: Fail if any child fails, succeed if all succeed
        if (failureCount > 0) return NodeStatus.FAILURE;
        if (successCount == children.Count) return NodeStatus.SUCCESS;

        return NodeStatus.RUNNING;
    }
}