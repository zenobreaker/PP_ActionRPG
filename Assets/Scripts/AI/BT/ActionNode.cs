using System;
using System.Collections;
using UnityEngine;


public class ActionNode : BTNode
{

    protected Func<BTNode.NodeState> onUpdate = null; 

    public ActionNode (Func<BTNode.NodeState> onUpdate)
    {
        this.onUpdate = onUpdate;
    }

    public string GetMethodName()
    {
        if (onUpdate != null)
        {
            return onUpdate.Method.Name;
        }
        return "";
    }


    public override NodeState Evaluate()
    {
        //Debug.Log($"Current Node Name : {GetMethodName()} ");
        return onUpdate?.Invoke() ?? BTNode.NodeState.Failure;
    }
}
