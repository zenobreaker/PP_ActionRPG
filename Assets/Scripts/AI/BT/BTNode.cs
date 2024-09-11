using UnityEngine;

public abstract class BTNode
{

    public enum NodeState { Running, Success, Failure, }
    protected NodeState state;
    protected GameObject owner;

    public BTNode(GameObject owner) => this.owner = owner;

    public abstract NodeState Evaluate();
    
}
