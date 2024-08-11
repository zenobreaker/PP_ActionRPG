using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


/// <summary>
///  NavMeshAgent�� �̿��Ͽ�  �����̴� ������Ʈ 
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class MovementComponent : MonoBehaviour
{
    [SerializeField] private float radius = 10.0f; // ���� �ݰ� 
   // [SerializeField] private float goalDelay = 2.0f; // ���� �� ��� �ð� 
   // [SerializeField] private float goalDelayRandom = 0.5f; // goalDelay +( - ���� ~ +����)
    [SerializeField] //private PatrolPoints patrolPoints;
    //public bool HasPatrolPoints { get => patrolPoints != null; }

    private Vector3 initPosition; // ��������
    private Vector3 goalPosition; // ��ǥ����


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

        //TODO: Wait Mode �� ������
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

        // �� �� �ִ� ��ġ�� ���� ������ ������.
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

        
            // �� �� �ִ� ��ΰ� ������ ��ƾ ���� 
            if (navMeshAgent.CalculatePath(goalPosition, navMeshPath) == true)
            {
                navMeshAgent.SetPath(navMeshPath);
                yield break;
            }


            yield return null;  // �������� ���� 
        }
    }




}
