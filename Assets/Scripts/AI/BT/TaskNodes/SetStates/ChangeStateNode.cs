using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �� ���� �ֿ� ��Ʈ�ѷ��� ���¸� �����ϴ� ��� 
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
            // ���� ���� �Ǿ��� ������ ���� ���¸�
            if (controller.EquipMode == false)
            {
                Debug.Log("���� ��� ");
                controller.SetEquipMode();
            }
            // ������ ���¸� �߰� 
            if (controller.ApproachMode == false)
            {
                Debug.Log("�߰� ���");
                controller.SetApproachMode();
            }
        }
        else
        {
            Debug.Log("���� ���");
            controller.SetPatrolMode();
        }


        return NodeState.Success;
    }
}
