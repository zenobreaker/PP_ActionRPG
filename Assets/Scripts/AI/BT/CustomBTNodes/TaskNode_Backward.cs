using AI.BT.Helpers;
using AI.BT.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;

namespace AI.BT.CustomBTNodes
{
    /// <summary>
    /// 정찰 지점이 있다면 해당 지점으로 이동, 없다면 일정 구간 범위에서 랜덤한 곳으로 이동
    /// </summary>
    public class TaskNode_Backward : TaskNode
    {

        private BTAIController controller; 
        private NavMeshAgent agent;
        private NavMeshPath navMeshPath;
        private Vector3 initPosition;
        private Vector3 goalPosition;
        private float radius;

        private int loopBreakMaxCount = 10;     // 루프를 강제로 탈출시킬 최대 수치 
        private int loopCount;

        public Action<Vector3> OnDestination;
        private Coroutine backwardCoroutine;

        public TaskNode_Backward(GameObject ownerObject, SO_Blackboard blackboard, float radius)
            : base(ownerObject, blackboard)
        {
            nodeName = "Backward";

            controller = ownerObject.GetComponent<BTAIController>();
            agent = ownerObject.GetComponent<NavMeshAgent>();
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
                ChangeActionState(ActionState.End);
                return NodeState.Failure;
            }

            // 특정 지점 위치를 반환
            if (controller == null)
            {
                ChangeActionState(ActionState.End);
                return NodeState.Failure;
            }

            loopCount = 0; 
            navMeshPath = null; 
            initPosition = goalPosition = agent.transform.position;
            agent.updateRotation = false; 

            backwardCoroutine = CoroutineHelper.Instance.StartHelperCoroutine(CreateNavMeshPathRoutine());
            // 경로에 따른 처리 
            if (navMeshPath != null)
            {
                OnDestination?.Invoke(goalPosition);
                ChangeActionState(ActionState.Update);
                agent.SetPath(navMeshPath);

                return NodeState.Running;
            }
            else
            {
                return NodeState.Failure;
            }
        }


        protected override NodeState OnUpdate()
        {
            if (agent == null || CheckPath() == false)
            {
                ChangeActionState(ActionState.End);
                ResetAgent();

                return NodeState.Failure;
            }

            if (CalcArrive() == false)
            {
                //ChangeActionState(ActionState.Begin);
                //agent.SetDestination(goalPosition);
                return NodeState.Running;
            }

            agent.updateRotation = true;
            return base.OnUpdate();
        }

        protected override NodeState OnEnd()
        {
            ResetAgent();

            return base.OnEnd();
        }

        private IEnumerator CreateNavMeshPathRoutine()
        {
            NavMeshPath path = null;


            Vector3 prevGoalPosition = goalPosition;
            // 지정한 지점이 없다면 인위적을 선택하여 처리 
            //TODO: 비동기나 코루틴으로 빼야할 듯한 로직 
            //TODO: 코루틴 러너나 헬퍼로 넘겨준다.
            while (true)
            {
                if (loopCount >= loopBreakMaxCount)
                {
                    Debug.Log("Not find Goal Poistion");
                    break; 
                }

                loopCount++;
                while (true)
                {
                    // 캐릭터의 현재 방향을 얻습니다.
                    Vector3 forwardDirection = owner.transform.forward;

                    // 후방 방향은 현재 방향의 반대 방향입니다.
                    Vector3 backwardDirection = -forwardDirection;

                    // 후방으로 일직선 상의 좌표를 얻기 위해 z 축 값을 설정합니다.
                    float z = UnityEngine.Random.Range(1.0f, radius);

                    // 후방 방향으로 일정 거리만큼 이동한 좌표를 계산합니다.
                    goalPosition = initPosition + backwardDirection * z;

                    // 이전 목표 지점과 너무 가까운지 확인합니다.
                    if (Vector3.Distance(goalPosition, prevGoalPosition) > radius * 0.25f)
                    {
                        break;
                    }
                }

                // NavMesh 상에서 해당 좌표로 이동할 수 있는지 확인합니다.
                path = new NavMeshPath();
                if (agent.CalculatePath(goalPosition, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    initPosition = goalPosition;
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
            //Debug.Log($"Backward Abort / {currActionState}");
            ChangeActionState(ActionState.Begin);
            ResetAgent();

            CoroutineHelper.Instance.StopHelperCoroutine(backwardCoroutine);
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
            float distanceSquared = (goalPosition - agent.transform.position).sqrMagnitude;

            if (distanceSquared <= agent.stoppingDistance 
                || agent.remainingDistance <= agent.stoppingDistance)
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
