public abstract class Decorator : Node
{
    protected Node child;
    public Decorator(Node node) { child = node; }
}

public class Inverter : Decorator
{
    public Inverter(Node node) : base(node) { }
    public override NodeStatus Tick()
    {
        NodeStatus status = child.Tick();
        if (status == NodeStatus.SUCCESS) return NodeStatus.FAILURE;
        if (status == NodeStatus.FAILURE) return NodeStatus.SUCCESS;
        return status;
    }
}

public class Timeout : Decorator
{
    private float timeLimit;
    private float startTime;

    public Timeout(Node node, float limit) : base(node) { timeLimit = limit; }

    public override NodeStatus Tick()
    {
        if (startTime == 0) startTime = UnityEngine.Time.time;

        if (UnityEngine.Time.time - startTime > timeLimit)
        {
            startTime = 0; // Reset for next time
            return NodeStatus.FAILURE;
        }

        NodeStatus status = child.Tick();
        if (status != NodeStatus.RUNNING) startTime = 0;
        return status;
    }
}

public class Cooldown : Decorator
{
    private float cooldownDuration;
    private float nextAllowedTime;

    public Cooldown(Node node, float duration) : base(node) { cooldownDuration = duration; }

    public override NodeStatus Tick()
    {
        if (UnityEngine.Time.time < nextAllowedTime)
            return NodeStatus.FAILURE;

        // Run the child (the Attack)
        NodeStatus status = child.Tick();

        // CHANGE: Start the cooldown as long as the child didn't fail 
        // or simply every time it's called!
        nextAllowedTime = UnityEngine.Time.time + cooldownDuration;

        return status;
    }
}

public class Repeater : Decorator
{
    private int count; // Use -1 for infinite
    private int currentIteration = 0;

    public Repeater(Node node, int iterations) : base(node) { count = iterations; }

    public override NodeStatus Tick()
    {
        NodeStatus status = child.Tick();

        if (status == NodeStatus.SUCCESS)
        {
            currentIteration++;
            if (count != -1 && currentIteration >= count)
            {
                currentIteration = 0;
                return NodeStatus.SUCCESS;
            }
        }

        return NodeStatus.RUNNING;
    }
}