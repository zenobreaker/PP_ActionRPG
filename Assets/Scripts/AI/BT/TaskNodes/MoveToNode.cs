using AI.BT.Nodes;
using UnityEngine;
using UnityEngine.AI;


namespace AI.BT.TaskNodes
{
    /// <summary>
    /// MoveToNode 는 NavMeshAgent 컴포넌트의 의존도가 높다.
    /// </summary>
    public class MoveToNode : TaskNode
    {


        private NavMeshAgent agent;
        private Vector3 target;
        public Vector3 Target { set => target = value; }

        public MoveToNode(GameObject ownerObject, SO_Blackboard blackboard)
            : base(ownerObject, blackboard)
        {
            nodeName = "MoveTo";

            agent = ownerObject.GetComponent<NavMeshAgent>();
            if (agent == null)
                agent = ownerObject.AddComponent<NavMeshAgent>();

            onBegin = OnBegin;
            onUpdate = OnUpdate;
            onEnd = OnEnd;
            onAbort = OnAbort;
        }

        // 도착 지점을 언제 어디서 어떻게 세팅할 것인지가 문제다. 
        // 해결방안 1. OnBegin함수를 생성자에서 전달받아서 처리해본다. => MoveTo 전용 OnBegin 등등 필요할지도?

        protected override NodeState OnBegin()
        {
            if (agent == null)
                return NodeState.Failure;
            
            if (blackboard != null)
            {
                //Debug.Log($"Move Begin / {currActionState}");
                GameObject targetObject = blackboard.GetValue<GameObject>("Target");
                if (targetObject == null)
                {
                   // ChangeActionState(ActionState.End);
                    ResetAgent();
                    //Debug.Log("Target Loss!");
                    return NodeState.Failure;
                }

                target = targetObject.transform.position;   
            }

            agent.SetDestination(target);
            //Debug.Log("Move Begin");
            return base.OnBegin();
        }

        protected override NodeState OnUpdate()
        {
            if (agent == null || CheckPath() == false)
                return NodeState.Failure;

            //Debug.Log($"Move Update / {currActionState}");
            if (CalcArrive() == false)
            {
                // 다시 대상 위치를 탐색하기 위해 Begin으로 
                ChangeActionState(ActionState.Begin);
                return NodeState.Running;
            }

            //ResetAgent();
            //Debug.Log("Move Update");
            return base.OnUpdate();
        }

        protected override NodeState OnEnd()
        {
            ResetAgent();

            Debug.Log($"Move End / {currActionState}");
            Debug.Log("Move End");
            return base.OnEnd();
        }

        private void ResetAgent()
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            //agent.isStopped = true;
        }

        protected override NodeState OnAbort()
        {
            ChangeActionState(ActionState.Begin);
            ResetAgent();

            return base.OnAbort();
        }

        private bool CalcArrive()
        {
            float distanceSquared = (target - agent.transform.position).sqrMagnitude;

            if (distanceSquared <= agent.stoppingDistance)
            {
                Debug.Log("도착");
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
}
