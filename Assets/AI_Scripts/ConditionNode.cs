public class ConditionNode : Node
{
    private System.Func<bool> condition;
    public ConditionNode(System.Func<bool> check) { condition = check; }

    public override NodeStatus Tick()
    {
        return condition() ? NodeStatus.SUCCESS : NodeStatus.FAILURE;
    }
}