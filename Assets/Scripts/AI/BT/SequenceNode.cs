using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceNode : BTNode
{

    List<BTNode> children;

    public SequenceNode(List<BTNode> children)
    {
        this.children = children;
    }



    public override NodeState Evaluate()
    {
        foreach (var child in children)
        {
            NodeState result = child.Evaluate();
            switch(result)
            {
                case NodeState.Running:
                return NodeState.Running;
                case NodeState.Failure:
                return NodeState.Failure;
            }
        }

        return NodeState.Success;
     
    }


}
