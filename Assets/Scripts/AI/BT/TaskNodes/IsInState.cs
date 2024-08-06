using System.Collections;
using UnityEngine;


public class IsInState : BTNode
{

    private BTAIController ai;
    private BTAIController.Type type;

    public IsInState(BTAIController ai, BTAIController.Type type)
    {
        this.ai = ai;
        this.type = type;
    }

    public override NodeState Evaluate()
    {
        Debug.Log($"Is in State {type} / result : {ai.MyType == type}");
        return ai.MyType == type ? NodeState.Success : NodeState.Failure;
    }
}
