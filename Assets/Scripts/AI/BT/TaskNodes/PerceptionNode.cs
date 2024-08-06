using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerceptionNode : BTNode
{
    private BTAIController controller;
    private PerceptionComponent perception;

    public PerceptionNode(BTAIController controller, PerceptionComponent perception)
    {
        this.controller = controller;
        this.perception = perception;
    }

    public override NodeState Evaluate()
    {
        GameObject player = perception.GetPercievedPlayer();
        if (player == null)
        {
            controller.Percepted = false;
            return NodeState.Failure;
        }

        Debug.Log("°¨ÁöµÊ");

        controller.Percepted = true; 
        return NodeState.Failure;
    }

}
