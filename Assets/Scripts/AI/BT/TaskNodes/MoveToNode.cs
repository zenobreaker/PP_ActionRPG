using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// �����̰� �ϴ� ��� PatrolComponent�� ���̽��� ��
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
