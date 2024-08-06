using UnityEngine;

public class DecoratorNode : BTNode
{
    protected BTNode childNode;

    public DecoratorNode(BTNode childNode)
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
