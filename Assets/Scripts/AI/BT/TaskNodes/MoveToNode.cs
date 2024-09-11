using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// MoveToNode 는 NavMeshAgent 컴포넌트의 의존도가 높다.
/// </summary>
public class MoveToNode : ActionNode
{

    // 이 클래스는 타 컴포넌트의 의존도가 필수불가결이다.


    private NavMeshAgent agent;
    private Vector3 target;
    public void SetDestination(Vector3 destination) => this.target = destination;


    public MoveToNode(GameObject ownerObject)
        : base(ownerObject)
    {
        agent = ownerObject.GetComponent<NavMeshAgent>();
        if(agent == null)
            ownerObject.AddComponent<NavMeshAgent>();

        onBegin = OnBegin;
        onUpdate = OnUpdate;
        onEnd = OnEnd;
    }

    // 도착 지점을 언제 어디서 어떻게 세팅할 것인지가 문제다. 
    // 해결방안 1. OnBegin함수를 생성자에서 전달받아서 처리해본다. => MoveTo 전용 OnBegin 등등 필요할지도?

    protected override NodeState OnBegin()
    {
        if (agent == null)
            return NodeState.Failure;
        
        agent.SetDestination(target);

        return base.OnBegin();
    }

    protected override NodeState OnUpdate()
    {
        if (agent == null || CheckPath())
            return NodeState.Failure;

        if (CalcArrive())
        {
            agent.velocity = Vector3.zero;
            agent.ResetPath();
        }
        else
            return NodeState.Running;

        return base.OnUpdate();
    }

    protected override NodeState OnEnd()
    {
        return base.OnEnd();
    }

 

    private bool CalcArrive()
    {
        float distanceSquared = (target - agent.transform.position).sqrMagnitude;

        if(distanceSquared <= agent.stoppingDistance) 
        {
            return true; 
        }

        return false; 
    }

    private bool CheckPath()
    {
        NavMeshPath path = new NavMeshPath();
        return agent.CalculatePath(target, path);
    }
}
