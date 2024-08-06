using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SelectorNode : BTNode
{
    protected  List<BTNode> children;
   
    public SelectorNode(List<BTNode> children)
    {
        this.children = children;
    }

    public  override NodeState Evaluate()
    {
        foreach (BTNode node in children)
        {
            switch (node.Evaluate())
            {
                case NodeState.Running:
                return NodeState.Running;   
                case NodeState.Success:
                return NodeState.Success;
            }
        }
        return NodeState.Failure;
    }
}
