using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetRandomLocationNode : ActionNode
{
    private PatrolComponent patrol;

    public GetRandomLocationNode(PatrolComponent patrol, Func<NodeState> onUpdate) : base(onUpdate)
    {
        this.patrol = patrol;
    }

    public GetRandomLocationNode(Func<NodeState> onUpdate) : base(onUpdate)
    {

    }

    //public override NodeState Evaluate()
    //{
    //    if (patrol == null)
    //        return NodeState.Failure;

        
    //    // 갈 수 있는 상태가 될 때까지 기다림
    //    if (patrol.GetPath() != null)
    //        return NodeState.Success;
        
    //    patrol.SetPath();

    //    base.Evaluate();
    //    return NodeState.Success; 
    //}

}