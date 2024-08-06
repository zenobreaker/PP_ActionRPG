using UnityEngine;

public class PerceptionDecorator : DecoratorNode
{
    private PerceptionComponent perception;

    public PerceptionDecorator(BTNode childNode, PerceptionComponent perception) : base(childNode)
    {
        this.childNode = childNode;
        this.perception = perception;
    }


    public override NodeState Evaluate()
    {
        if(childNode == null)
            base.Evaluate();

        GameObject player = perception.GetPercievedPlayer();
        if (player != null)
        {
            Debug.Log("°¨ÁöµÊ");
            return NodeState.Failure;
        }

        

        return childNode.Evaluate();
    }
    
}
