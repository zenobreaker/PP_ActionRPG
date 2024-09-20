using AI.BT.Nodes;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace AI.BT.CustomBTNodes
{
    /// <summary>
    /// 정찰 지점이 있다면 해당 지점으로 이동, 없다면 일정 구간 범위에서 랜덤한 곳으로 이동
    /// </summary>
    public class TaskNode_Patrol : TaskNode
    {

        private NavMeshAgent agent;
        private NavMeshPath navMeshPath;
        private Vector3 initPosition;
        private Vector3 goalPosition;
        private float radius;

        public Action<Vector3> OnDestination;

        public TaskNode_Patrol(GameObject ownerObject, SO_Blackboard blackboard, float radius)
            : base(ownerObject, blackboard)
        {
            nodeName = "Patrol";

            agent = ownerObject.GetComponent<NavMeshAgent>();
            this.radius = radius;

            Debug.Log("이색히 시작 상태 " + currActionState);

            onBegin = OnBegin;
            onUpdate = OnUpdate;
            onEnd = OnEnd;
        }
        protected override NodeState OnBegin()
        {
            if (agent == null || blackboard == null)
            {
                ChangeActionState(ActionState.End);
                return NodeState.Failure;
            }

            // 특정 지점 위치를 반환
            BTAIController aicont =  owner.GetComponent<BTAIController>();
            if (aicont == null)
            {
                ChangeActionState(ActionState.End);
                return NodeState.Failure;
            }

            initPosition = goalPosition = agent.transform.position;

            PatrolPoints patrolPoints =   aicont.PatrolPoints;
            if(patrolPoints == null )
            {
                // 지정한 지점이 없다면 인위적을 선택하여 처리 
                bool bResult = CreateNaveMeshPathRoutine();
                if (bResult)
                {
                    OnDestination?.Invoke(goalPosition);
                    ChangeActionState (ActionState.Update);
                    return NodeState.Running;
                }
                else
                    return NodeState.Failure;
            }
            else
            {
                goalPosition = patrolPoints.GetMoveToPosition();
                agent.SetDestination(goalPosition);
            }

            return base.OnBegin();
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
                agent.SetDestination(goalPosition);
                return NodeState.Running;
            }


            return base.OnUpdate();
        }

        protected override NodeState OnEnd()
        {
            ResetAgent();

            return base.OnEnd();
        }


        private bool CreateNaveMeshPathRoutine()
        {
            NavMeshPath path = null;

            Vector3 prevGoalPosition = goalPosition;
            while (true)
            {
                while (true)
                {
                    float x = UnityEngine.Random.Range(-radius * 0.5f, radius * 0.5f);
                    float z = UnityEngine.Random.Range(-radius * 0.5f, radius * 0.5f);

                    goalPosition = new Vector3(x, 0, z) + initPosition;

                    if (Vector3.Distance(goalPosition, prevGoalPosition) > radius * 0.25f)
                        break;

                    return false;
                }

                path = new NavMeshPath();


                if (agent.CalculatePath(goalPosition, path) == true)
                {
                    navMeshPath = path;
                    agent.SetPath(navMeshPath);

                    return true; 
                }

                return false;
            }
        }



        private void ResetAgent()
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
            //agent.isStopped = true;
        }
        private bool CalcArrive()
        {
            float distanceSquared = (goalPosition - agent.transform.position).sqrMagnitude;

            if (distanceSquared <= agent.stoppingDistance 
                || agent.remainingDistance <= agent.stoppingDistance)
            {
                Debug.Log("도착");
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
