using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 이 노드는 주요 컨트롤러의 상태를 조작하는 노드 
/// </summary>
public class ChangeStateNode : BTNode
{
    private BTAIController controller;

    public ChangeStateNode(BTAIController controller)
    {
        this.controller = controller;
    }

    public override NodeState Evaluate()
    {
        if(controller == null)
            return NodeState.Failure;

        if(controller.Percepted)
        {
            // 적이 감지 되었고 장착을 안한 상태면
            if (controller.EquipMode == false)
            {
                Debug.Log("장착 모드 ");
                controller.SetEquipMode();
            }
            // 장착한 상태면 추격 
            if (controller.ApproachMode == false)
            {
                Debug.Log("추격 모드");
                controller.SetApproachMode();
            }
        }
        else
        {
            Debug.Log("순찰 모드");
            controller.SetPatrolMode();
        }


        return NodeState.Success;
    }
}
