public class ActionNode : Node
{
    private System.Action action;
    public ActionNode(System.Action task) { action = task; }

    public override NodeStatus Tick()
    {
        action(); // Execute the task (e.g., AttackPlayer())
        return NodeStatus.SUCCESS; // Or return RUNNING if it takes time
    }
}