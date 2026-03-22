public enum NodeStatus { SUCCESS, FAILURE, RUNNING }

public abstract class Node
{
    public abstract NodeStatus Tick();
}