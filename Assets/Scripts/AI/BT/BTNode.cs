public abstract class BTNode
{

    public enum NodeState { Running, Success, Failure, }
    protected NodeState state;

    public abstract NodeState Evaluate();
    
}
