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
    public class TaskNode_Patrol : TaskNode
    {

        private BTAIController controller; 
        private NavMeshAgent agent;
        private NavMeshPath navMeshPath;
        private PatrolPoints patrolPoints; 
        private Vector3 initPosition;
        private Vector3 goalPosition;
        private float radius;

        private bool hasPatrolPoints;

        private int loopBreakMaxCount = 10;     // 루프를 강제로 탈출시킬 최대 수치 
        private int loopCount;

        public Action<Vector3> OnDestination;
        private Coroutine patrolCoroutine;

        public TaskNode_Patrol(GameObject ownerObject, SO_Blackboard blackboard, float radius)
            : base(ownerObject, blackboard)
        {
            nodeName = "Patrol";

            controller = ownerObject.GetComponent<BTAIController>();
            agent = ownerObject.GetComponent<NavMeshAgent>();
            this.radius = radius;

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
            initPosition = goalPosition = agent.transform.position;

            patrolPoints = controller.PatrolPoints;
            hasPatrolPoints = patrolPoints != null;

            patrolCoroutine = CoroutineHelper.Instance.StartHelperCoroutine(CreateNavMeshPathRoutine());
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
                return NodeState.Failure;
            }

            if (CalcArrive() == false)
            {
                return NodeState.Running;
            }

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
            navMeshPath = null; 

            if (agent == null || agent.enabled == false )
            {
                yield break; 
            }

            if(hasPatrolPoints)
            {
                goalPosition = patrolPoints.GetMoveToPosition();

                path = new NavMeshPath();
                bool bCheck = agent.CalculatePath(goalPosition, path);
                if (bCheck == false)
                {
                    Debug.LogError("Area Calc Erorr");
                    yield break;
                }

                navMeshPath = path;

                patrolPoints.UpdateNextIndex();

                yield break;
            }


            Vector3 prevGoalPosition = goalPosition;
            // 지정한 지점이 없다면 인위적을 선택하여 처리 
            while (true)
            {
                if (loopCount >= loopBreakMaxCount)
                {
                    Debug.Log("Not find Goal Poistion");
                    yield break;
                }

                loopCount++; 
                int semiLoopCount = 0;
                while (true)
                {
                    if (semiLoopCount >= loopBreakMaxCount)
                    {
                        Debug.Log("Not find Goal Poistion2");
                        break;
                    }
                    semiLoopCount++;

                    float x = UnityEngine.Random.Range(-radius * 0.5f, radius * 0.5f);
                    float z = UnityEngine.Random.Range(-radius * 0.5f, radius * 0.5f);

                    goalPosition = new Vector3(x, 0, z) + initPosition;
                    if (Vector3.Distance(goalPosition, prevGoalPosition) > radius * 0.25f)
                        break;
                }

                path = new NavMeshPath();

                if (AgentCheck() && agent.CalculatePath(goalPosition, path) == true)
                {
                    navMeshPath = path;
                    loopCount = 0;
                    yield break;
                }
            }
        }


        protected override NodeState OnAbort()
        {
            //Debug.Log($"Patrol Abort / {currActionState}");
            ChangeActionState(ActionState.Begin);
            ResetAgent();

            CoroutineHelper.Instance.StopHelperCoroutine(patrolCoroutine);
            return base.OnAbort();
        }

        private void ResetAgent()
        {
            if (AgentCheck() == false)
                return;

            agent.ResetPath();
            agent.velocity = Vector3.zero;
            //agent.isStopped = true;
        }
        private bool CalcArrive()
        {
            float distanceSquared = (goalPosition - agent.transform.position).magnitude;

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
            if (AgentCheck() == false)
                return false;


            NavMeshPath path = new NavMeshPath();
            return agent.CalculatePath(goalPosition, path);
        }

    }

}
