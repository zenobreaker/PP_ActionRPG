using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


/// <summary>
///  NavMeshAgent를 이용하여  움직이는 컴포넌트 
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class MovementComponent : MonoBehaviour
{
    [SerializeField] private float radius = 10.0f; // 순찰 반경 
   // [SerializeField] private float goalDelay = 2.0f; // 도달 시 대기 시간 
   // [SerializeField] private float goalDelayRandom = 0.5f; // goalDelay +( - 랜덤 ~ +랜덤)
    [SerializeField] //private PatrolPoints patrolPoints;
    //public bool HasPatrolPoints { get => patrolPoints != null; }

    private Vector3 initPosition; // 시작지점
    private Vector3 goalPosition; // 목표지점


    private NavMeshPath navMeshPath;
    private NavMeshAgent navMeshAgent;
    private Coroutine coroutienPathRoutine;

    private bool bArrived = false;

    private void Awake()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();   

    }

    private void Update()
    {
        if (navMeshPath == null)
            return;

        if (bArrived == true)
            return;


        float distance = Vector3.Distance(transform.position, goalPosition);

        if (distance >= navMeshAgent.stoppingDistance)
            return;

        bArrived = true;

        //TODO: Wait Mode 때 돌릴까
        //float waitTime = goalDelay + Random.Range(-goalDelayRandom, goalDelayRandom);

        //IEnumerator waitRoutine = WaitDelay(waitTime);

        //StartCoroutine(waitRoutine);
    }

    public void StartMove()
    {
        if (navMeshPath != null)
            return;

        StartCoroutine(CreateNavMeshPathRoutine());
    }

    private IEnumerator CreateNavMeshPathRoutine()
    {
        navMeshPath = null;

        //if (HasPatrolPoints)
        //{
        //    goalPosition = patrolPoints.GetMoveToPosition();

        //    path = new NavMeshPath();

        //    bool bCheck = navMeshAgent.CalculatePath(goalPosition, path);
        //    Debug.Assert(bCheck);

        //    patrolPoints.UpdateNextIndex();

        //    return path;
        //}


        Vector3 prevGoalPosition = goalPosition;

        // 갈 수 있는 위치가 나올 때까지 돌린다.
        while (true)
        {
            while (true)
            {
                float x = Random.Range(-radius * 0.5f, radius * 0.5f);
                float z = Random.Range(-radius * 0.5f, radius * 0.5f);

                goalPosition = new Vector3(x, 0, z) + initPosition;

                if (Vector3.Distance(goalPosition, prevGoalPosition) > radius * 0.25f)
                    break;

                yield return null;
            }

        
            // 갈 수 있는 경로가 나오면 루틴 종료 
            if (navMeshAgent.CalculatePath(goalPosition, navMeshPath) == true)
            {
                navMeshAgent.SetPath(navMeshPath);
                yield break;
            }


            yield return null;  // 매프레임 마다 
        }
    }




}
