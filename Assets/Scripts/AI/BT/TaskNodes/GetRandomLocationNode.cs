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

        
    //    // �� �� �ִ� ���°� �� ������ ��ٸ�
    //    if (patrol.GetPath() != null)
    //        return NodeState.Success;
        
    //    patrol.SetPath();

    //    base.Evaluate();
    //    return NodeState.Success; 
    //}

}