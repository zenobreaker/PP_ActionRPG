using BT.Helpers;
using BT.Nodes;
using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

namespace BT.CustomBTNodes
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

        private bool hasFirst = true;
        private bool bRight = true; // 왼쪽으로 갈지 오른쪽으로 갈지 정하기 
        
        private float currentAngle = 0.0f;
        private float angleStep = 10.0f; // 각도 변화폭

        private float moveTime;
        private float moveTimeDelay;
        private float lastTime;
        private float currentMoveTime;

        private Vector3 targetPos = Vector3.zero;
        private Vector3 centerPos = Vector3.zero;

        public Action<Vector3> OnDestination;
        private Coroutine strafCoroutine;

        private bool bSuceess = false;

        public TaskNode_Strafe(GameObject ownerObject, SO_Blackboard blackboard, float radius,
            float moveTime = 1.0f, float moveTimeDelay= 0.0f)
            : base(ownerObject, blackboard)
        {
            nodeName = "Strafe";

            controller = ownerObject.GetComponent<BTAIController>();
            perception = ownerObject.GetComponent<PerceptionComponent>();
            agent = owner.GetComponent<NavMeshAgent>();

            this.radius = radius;
            this.moveTime = moveTime;
            this.moveTimeDelay = moveTimeDelay;

            onBegin = OnBegin;
            onUpdate = OnUpdate;
            onEnd = OnEnd;
            onAbort = OnAbort;
        }

        private bool AgentCheck()
        {
            bool bCheck = true;
            bCheck &= agent != null;
            bCheck &= agent.enabled;

            return bCheck;
        }


        protected override NodeState OnBegin()
        {
            if (AgentCheck() == false || blackboard == null)
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


            navMeshPath = null;

            if (hasFirst == true)
            {
                centerPos = player.transform.position;
                DecideDirection();
            }

            if (SuccessedGoalPosition() == false)
                return NodeState.Failure;

            agent.updateRotation = false;

            // 경로에 따른 처리 
            if (navMeshPath != null)
            {
                OnDestination?.Invoke(goalPosition);
                agent.SetPath(navMeshPath);


                currentMoveTime = Random.Range(moveTime + (-1.0f * moveTimeDelay),
               moveTime + (+1.0f * moveTimeDelay));
                lastTime = Time.time;

                return NodeState.Running;
            }

            hasFirst = true;
            return NodeState.Failure;
        }


        protected override NodeState OnUpdate()
        {
            if (AgentCheck() == false || CheckPath() == false)
            {
                return NodeState.Failure;
            }

            GameObject player = perception.GetPercievedPlayer();
            if (player == null)
                return NodeState.Failure;

            bool bCheck = perception.CheckPositionOther(owner, goalPosition, true);
            if (bCheck)
            {
                return NodeState.Failure;
            }

            // 대상자 바라보기 
            targetPos = player.transform.position - owner.transform.position;
            targetPos.y = 0;
            owner.transform.localRotation = Quaternion.LookRotation(targetPos.normalized, Vector3.up);

            // 도착하면 값 다시 세팅하도록 
            if (CalcArrive() == true)
            {
                if (SuccessedGoalPosition() == false)
                    return NodeState.Failure;
                else if (navMeshPath != null)
                {
                    OnDestination?.Invoke(goalPosition);
                    agent.SetPath(navMeshPath);
                }
            }

            // 정해진 시간 동안 움직인다.
            if (Time.time - lastTime < currentMoveTime)
            {
                return NodeState.Running;
            }

            return NodeState.Success;
        }

        protected override NodeState OnEnd()
        {
            ResetAgent();

            strafCoroutine = null;
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

        private bool SuccessedGoalPosition()
        {
            bSuceess = true;
            strafCoroutine = null;
            strafCoroutine ??= CoroutineHelper.Instance.StartHelperCoroutine(CreateNavMeshPathRoutine());
            //Debug.Log($"Strafe Search Result : {bSuceess}");
            return bSuceess;
        }

        private IEnumerator CreateNavMeshPathRoutine()
        {
            NavMeshPath path = null;
            navMeshPath = null;
            int loopCount = 0;

            Vector3 prevGoalPos = goalPosition;
            // 현재 캐릭터의 위치에서 시작 각도 계산
            Vector3 currentPosition = owner.transform.position; // 현재 캐릭터 위치
            Vector3 directionToCenter = (currentPosition - centerPos).normalized;
            currentAngle = Mathf.Atan2(directionToCenter.z, directionToCenter.x) * Mathf.Rad2Deg; // 시작 각도 계산
            while (true)
            {
                if (loopCount >= loopBreakMaxCount)
                {
                    //Debug.Log("Not find Goal Poistion");
                    bSuceess = false;
                    yield break;
                }

                loopCount++;

                int smalLoop = 0;
                while (true)
                {
                    smalLoop++;
                    if (smalLoop >= loopBreakMaxCount)
                    {
                        //Debug.Log("Not find Goal small scope ");
                        bSuceess = false;
                        break;
                    }

                    float dir = bRight ? 1 : -1;
                    currentAngle += angleStep * dir;
                    // 각도를 라디안으로 변환 (원호 이동을 위한 좌표 계산)
                    float radian = currentAngle * Mathf.Deg2Rad;

                    // 새로운 위치 계산 (극좌표 -> 직교좌표로 변환)
                    float x = Mathf.Cos(radian) * radius;
                    float z = Mathf.Sin(radian) * radius;

                    // 중심점에서 떨어진 위치로 계산 (B의 목표 위치)
                    Vector3 offset = new Vector3(x, 0, z);
                    goalPosition = centerPos + offset;
                    if (CanNextStep(goalPosition) == false)
                        continue;

                    break;
                }

                // NavMesh 상에서 해당 좌표로 이동할 수 있는지 확인합니다.
                path = new NavMeshPath();
                if (AgentCheck() &&
                    agent.CalculatePath(goalPosition, path)
                    && path.status == NavMeshPathStatus.PathComplete)
                {
                    navMeshPath = path;
                    // 유효한 경로가 있으면 루프를 종료하고 목표 좌표를 반환합니다.
                    yield break;
                }

                yield return null;
            }
        }


        protected override NodeState OnAbort()
        {
            if (currActionState == ActionState.Begin || currActionState == ActionState.Update)
            {
                //Debug.Log($"Backward Abort / {currActionState}");
                CoroutineHelper.Instance.StopHelperCoroutine(strafCoroutine);

                //TODO: Abort 중단되었을 때 상태를 돌리는게 좋은걸까?
                //ChangeActionState(ActionState.Begin);
                ResetAgent();

                hasFirst = true;
                currentAngle = 0;
            }

            return base.OnAbort();
        }

        private void ResetAgent()
        {
            if (AgentCheck() == false)
                return;

            strafCoroutine = null;
            if(agent != null && agent.isOnNavMesh && agent.isActiveAndEnabled)
                agent.ResetPath();
            agent.velocity = Vector3.zero;
            agent.updateRotation = true;
            //agent.isStopped = true;
        }

        private bool CanNextStep(Vector3 pos)
        {
            if (perception == null)
            {
                return true;
            }
            bool debug = false;
#if UNITY_EDITOR
            debug = true;
#endif
            bool bCheck = perception.CheckPositionOther(owner, pos, debug);

            //Debug.Log($"{owner.name} Strafe avoid ? : {bCheck == false}");

            return bCheck == false;
        }

        private bool CalcArrive()
        {
            float distance = Vector3.Distance(goalPosition, agent.transform.position);

            if (distance <= agent.stoppingDistance
                || agent.remainingDistance <= agent.stoppingDistance + 2.5f)
            {
                // Debug.Log("도착");
                return true;
            }

            return false;
        }

        private bool CheckPath()
        {
            if (AgentCheck() == false)
                return false;

            NavMeshPath path = new NavMeshPath();
            return agent.CalculatePath(goalPosition, path);
        }

    }

}
