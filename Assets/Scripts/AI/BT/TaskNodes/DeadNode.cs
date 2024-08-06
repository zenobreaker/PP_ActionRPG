using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeadNode : BTNode
{

    private BTAIController ai;
    private StateComponent state;


    public DeadNode(BTAIController ai, StateComponent state)
    {
        this.ai = ai;
        this.state = state;
    }



    public override NodeState Evaluate()
    {
        if (ai == null)
            return NodeState.Failure;
        if(state == null)
            return NodeState.Failure;

        //TODO: Á×À½ Ã³¸®
        if (state.DeadMode)
            return NodeState.Success;

        return NodeState.Failure;
    }
}
