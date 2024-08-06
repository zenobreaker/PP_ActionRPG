using UnityEngine;
using UnityEngine.AI;

public class PatrolNode : BTNode
{
    private BTAIController controller;
    private PatrolComponent patrol;
    private NavMeshAgent agent; 
    public PatrolNode(BTAIController controller, PatrolComponent patrol)
    {
        this.controller = controller;
        this.patrol = patrol;

        Debug.Assert(controller != null);
        agent = controller.GetComponent<NavMeshAgent>();
    }

    // ���� �´ٴ°� ������ �����ϰ� �����δٴ� �ǹ� 
    public override NodeState Evaluate()
    {
        bool bCheck = false;
        bCheck |= (controller == null);
        bCheck |= (agent == null);
        if (bCheck )
            return NodeState.Failure;

        if (controller.PatrolMode == false)
            return NodeState.Failure;

        // �����ߴٸ� ���� �Ұ��� ���� 
        if (patrol.Arrived) // �����߳�?
        {
            controller.SetWaitMode();
            return NodeState.Success;
        }

        patrol.StartMove();
        return NodeState.Running;

    }
}
