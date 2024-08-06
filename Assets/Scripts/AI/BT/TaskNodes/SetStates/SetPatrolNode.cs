using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class SetPatrolNode : BTNode
{
    //private float speed;
    //private float runSpeed; 
    //private float angluarSpeed;
    //private float stopDistance;

    private BTAIController controller;
    private PatrolComponent patrol;

    public SetPatrolNode(BTAIController controller, PatrolComponent patrol)
    {
        this.controller = controller;
        this.patrol = patrol;
    }

    bool bInit = false;
    private void InitPatrol()
    {
        if (bInit == true)
            return;

        bInit = true;

    }

    public override NodeState Evaluate()
    {
        bool bCheck = false;
        bCheck |= (controller == null);
        bCheck |= (patrol == null);
        if (bCheck)
            return NodeState.Failure;

        if(controller.Percepted)
            return NodeState.Failure;

        if (controller.PatrolMode == false)
        {
            controller.SetPatrolMode();
        }

        return NodeState.Success;
    }
}

