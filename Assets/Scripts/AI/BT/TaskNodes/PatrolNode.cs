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

    // 여기 온다는건 이전에 세팅하고 움직인다는 의미 
    public override NodeState Evaluate()
    {
        bool bCheck = false;
        bCheck |= (controller == null);
        bCheck |= (agent == null);
        if (bCheck )
            return NodeState.Failure;

        if (controller.PatrolMode == false)
            return NodeState.Failure;

        // 도착했다면 진행 불가로 판정 
        if (patrol.Arrived) // 도착했냐?
        {
            controller.SetWaitMode();
            return NodeState.Success;
        }

        patrol.StartMove();
        return NodeState.Running;

    }
}
