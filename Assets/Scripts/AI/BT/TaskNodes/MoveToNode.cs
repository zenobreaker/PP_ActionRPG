using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 움직이게 하는 노드 PatrolComponent를 베이스로 함
/// </summary>
public class MoveToNode : BTNode, IActionNode
{
    private MovementComponent movement;
    
    public MoveToNode(MovementComponent movement) 
    {
        this.movement = movement;
    }

    public override NodeState Evaluate()
    {
        return Execute();
    }

    public NodeState Execute()
    {
        return NodeState.Failure;
    }
}
