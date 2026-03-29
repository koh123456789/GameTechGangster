public class ActionNode : Node
{
    // Func<NodeStatus> means: "A function that returns a NodeStatus"
    private System.Func<NodeStatus> action;

    public ActionNode(System.Func<NodeStatus> task) { action = task; }

    public override NodeStatus Tick()
    {
        // Execute the task and return whatever the task tells us (SUCCESS or RUNNING)
        return action();
    }
}