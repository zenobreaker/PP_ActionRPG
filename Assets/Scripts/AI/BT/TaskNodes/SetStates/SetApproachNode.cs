using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetApproachNode : BTNode
{


    private BTAIController controller;

    public SetApproachNode(BTAIController controller)
    {
        this.controller = controller;
    }

    public override NodeState Evaluate()
    {
        if (controller == null)
            return NodeState.Failure;

        controller.SetApproachMode();

        return NodeState.Success;

    }


}
