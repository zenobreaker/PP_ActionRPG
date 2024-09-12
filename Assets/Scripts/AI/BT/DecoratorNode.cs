using UnityEngine;

public class DecoratorNode : BTNode
{
    protected BTNode childNode;

    public DecoratorNode(GameObject owner, BTNode childNode)
        : base(owner)
    {
        this.childNode = childNode; 
    }

    public override NodeState Evaluate()
    {
        if(childNode == null)
            return NodeState.Failure;

        return childNode.Evaluate();
    }

}
