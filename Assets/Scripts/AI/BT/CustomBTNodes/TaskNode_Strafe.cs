using AI.BT.Helpers;
using AI.BT.Nodes;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace AI.BT.CustomBTNodes
{
    /// <summary>
    /// 정찰 지점이 있다면 해당 지점으로 이동, 없다면 일정 구간 범위에서 랜덤한 곳으로 이동
    /// </summary>
    public class TaskNode_Strafe : TaskNode
    {

        private BTAIController controller;
        private PerceptionComponent perception;
        private NavMeshAgent agent;
        private NavMeshPath navMeshPath;
        private Vector3 goalPosition;
        private float radius;

        private int loopBreakMaxCount = 10;     // 루프를 강제로 탈출시킬 최대 수치 
        private int loopCount;

        private bool hasFirst = true;
        private bool bRight = true; // 왼쪽으로 갈지 오른쪽으로 갈지 정하기 
        private float maxAngle = 100.0f; // 특정 방향으로 진행 최대 각
        private float currentAngle = 0.0f;
        private float angleStep = 10.0f; // 각도 변화폭

        private Vector3 targetPos = Vector3.zero;
        private Vector3 centerPos = Vector3.zero;

        public Action<Vector3> OnDestination;
        private Coroutine strafCoroutine;

        public TaskNode_Strafe(GameObject ownerObject, SO_Blackboard blackboard, float radius)
            : base(ownerObject, blackboard)
        {
            nodeName = "Strafe";

            controller = ownerObject.GetComponent<BTAIController>();
            perception = ownerObject.GetComponent<PerceptionComponent>();
            agent = owner.GetComponent<NavMeshAgent>(); 

            this.radius = radius;

            onBegin = OnBegin;
            onUpdate = OnUpdate;
            onEnd = OnEnd;
            onAbort = OnAbort;
        }
        protected override NodeState OnBegin()
        {
            if (agent == null || blackboard == null)
            {
                return NodeState.Failure;
            }

            if (controller == null || perception == null)
            {
                return NodeState.Failure;
            }
            
            GameObject player = perception.GetPercievedPlayer();
            if (player == null)
                return NodeState.Failure;

            bRunning = true; 
            agent.updateRotation = false;
            navMeshPath = null;

            targetPos = player.transform.position - owner.transform.position;
            targetPos.y = 0;
            owner.transform.localRotation = Quaternion.LookRotation(targetPos.normalized, Vector3.up);
            if (hasFirst == true)
            {
                centerPos = player.transform.position;
            }
            DecideDirection();
            loopCount = 0;


            strafCoroutine = CoroutineHelper.Instance.StartHelperCoroutine(CreateNavMeshPathRoutine());
            // 경로에 따른 처리 
            if (navMeshPath != null)
            {
                OnDestination?.Invoke(goalPosition);
                ChangeActionState(ActionState.Update);
                agent.SetPath(navMeshPath);

                return NodeState.Running;
            }

            hasFirst = true;
            return NodeState.Failure;
        }


        protected override NodeState OnUpdate()
        {
            if (agent == null || CheckPath() == false)
            {
                return NodeState.Failure;
            }

            GameObject player = perception.GetPercievedPlayer();
            if (player == null)
                return NodeState.Failure;

            targetPos = player.transform.position - owner.transform.position;
            targetPos.y = 0;    
            owner.transform.localRotation = Quaternion.LookRotation(targetPos.normalized, Vector3.up);
            
            // 도착하면 값 다시 세팅하도록 
            if (CalcArrive() == true)
                ChangeActionState(ActionState.Begin);
            // 정해진 각도가 될 때까지 움직이기 
            if (Mathf.Abs(currentAngle) < maxAngle)
            {
                return NodeState.Running;
            }

            
            return base.OnUpdate();
        }

        protected override NodeState OnEnd()
        {
            ResetAgent();

            hasFirst = true;
            currentAngle = 0;
            return base.OnEnd();
        }


        private void DecideDirection()
        {
            if (hasFirst == false)
                return;

            hasFirst = false;
            currentAngle = 0;

            int num = UnityEngine.Random.Range(0, 2);
            if (num == 0)
            {
                bRight = true;

                return;
            }

            bRight = false;

        }

        private IEnumerator CreateNavMeshPathRoutine()
        {
            NavMeshPath path = null;

            Vector3 prevGoalPos = goalPosition;
            // 현재 캐릭터의 위치에서 시작 각도 계산
            Vector3 currentPosition = owner.transform.position; // 현재 캐릭터 위치
            Vector3 directionToCenter = (currentPosition - centerPos).normalized;
            currentAngle = Mathf.Atan2(directionToCenter.z, directionToCenter.x) * Mathf.Rad2Deg; // 시작 각도 계산
            while (true)
            {
                if (loopCount >= loopBreakMaxCount)
                {
                    Debug.Log("Not find Goal Poistion");
                    break;
                }

                loopCount++;
                
                int smalLoop = 0;
                while (true)
                {
                    smalLoop++;
                    if(smalLoop >= loopBreakMaxCount)
                    {
                        Debug.Log("Not find Goal small scope ");
                        break;
                    }

                    float dir = bRight ? 1 : -1;
                    currentAngle += angleStep * dir;

                    // 각도를 라디안으로 변환 (원호 이동을 위한 좌표 계산)
                    float radian = currentAngle * Mathf.Deg2Rad;

                    // 새로운 위치 계산 (극좌표 -> 직교좌표로 변환)
                    float x =  Mathf.Cos(radian) * radius;
                    float z =  Mathf.Sin(radian) * radius;

                    // 중심점에서 떨어진 위치로 계산 (B의 목표 위치)
                    Vector3 offset = new Vector3(x, 0, z);
                    goalPosition = centerPos + offset;

                    break;
                }

                // NavMesh 상에서 해당 좌표로 이동할 수 있는지 확인합니다.
                path = new NavMeshPath();
                if (agent.CalculatePath(goalPosition, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    navMeshPath = path;
                    loopCount = 0;
                    // 유효한 경로가 있으면 루프를 종료하고 목표 좌표를 반환합니다.
                    yield break;
                }

                yield return null;
            }
        }


        protected override NodeState OnAbort()
        {
            if(bRunning == false)
            {
                return NodeState.Failure;
            }
            Debug.Log($"Starfe Abort / {currActionState}");
            ChangeActionState(ActionState.Begin);
            ResetAgent();
            //TODO: 버그가 많으니 Abort 관련 처리를 수행하고 처리한다.
            //hasFirst = true;
            //currentAngle = 0;
            CoroutineHelper.Instance.StopHelperCoroutine(strafCoroutine);
            return base.OnAbort();
        }

        private void ResetAgent()
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.updateRotation = true;
            //agent.isStopped = true;
        }
        private bool CalcArrive()
        {
            float distanceSquared = (goalPosition - agent.transform.position).magnitude;

            if (distanceSquared <= agent.stoppingDistance
                || agent.remainingDistance <= agent.stoppingDistance + 2.5f)
            {
                // Debug.Log("도착");
                return true;
            }

            return false;
        }

        private bool CheckPath()
        {
            NavMeshPath path = new NavMeshPath();
            return agent.CalculatePath(goalPosition, path);
        }

    }

}
