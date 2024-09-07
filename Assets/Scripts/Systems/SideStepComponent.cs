using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static UnityEditor.PlayerSettings;
using static UnityEngine.EventSystems.EventTrigger;

public class SideStepComponent : MonoBehaviour
{
    [SerializeField] private float angleIncrement = 10.0f; // ���� ������ 
    [SerializeField] float totalAngle = 90.0f; // ��ȣ �̵� �ִ밢 
    [SerializeField] float distance = 5.0f;
    private GameObject targetObj;

    private Enemy enemy;
    private NavMeshAgent navMeshAgent;
    private NavMeshPath path;

    //private bool bFirstStep = false;
    private int crossroads = 0;
    private int toIndex = 0;


    private Vector3 debugPos; 
    private List<Vector3> sidePositionList = new List<Vector3>();

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        Debug.Assert(enemy != null);
        navMeshAgent = GetComponent<NavMeshAgent>();
    }

    public void DoStep()
    {
        if (targetObj == null)
            return;

        bool b = DoStep_CanNextPos(sidePositionList[toIndex]);
        if (b == false)
        {
            navMeshAgent.isStopped = true;
            return;
        }
        navMeshAgent.isStopped = false;

        navMeshAgent.updateRotation = false;
        Vector3 direction = targetObj.transform.position - transform.position;
        transform.localRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        if (navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
            return;

        //Debug.Log("����!");
        // ������ ���������� ���� �������� ����? 
        DoNextPos(targetObj);
    }

    private void RemovePathList()
    {
        sidePositionList.Clear();
    }

    private void SetSideMovePostionList(GameObject target)
    {
        if (target == null)
            return;
 

        Vector3 forward = target.transform.forward;
        Vector3 center = target.transform.position;
        //float distance =5.0f/* Vector3.Distance(center, this.transform.position)*/;
        
        for (float angle = -totalAngle; angle <= totalAngle; angle += angleIncrement)
        {
            Vector3 direction = Quaternion.Euler(0, angle, 0) * forward;
            Vector3 point = center + direction * distance;

            sidePositionList.Add(point);
        }

    }


    private bool CanDoNextPos(Vector3 pos)
    {
        float distance = Vector3.Distance(pos, transform.localPosition);

        return distance > navMeshAgent.stoppingDistance;
    }

    private bool DoStep_CanNextPos(Vector3 pos)
    {
        Vector3 direction = pos - transform.localPosition;
        float posToDistance = direction.magnitude;

        Vector3 myPos = transform.localPosition + Vector3.up;
        Ray ray = new Ray(myPos, direction);
        Debug.DrawRay(myPos, direction, Color.green, 5.0f);

        bool bCheck = true;
        if (Physics.Raycast(ray, posToDistance))
        {
            bCheck &= false;
        }

        return bCheck; 
    }

    private void SetFirstNearIndex()
    {
        float minDis = float.MaxValue;
        for (int i = 0; i < sidePositionList.Count; i++)
        {
            var pos = sidePositionList[i];
            float dist = Vector3.Distance(transform.position, pos);
            if (dist == 0 || CanDoNextPos(pos) == false)
                continue;

            if (dist < minDis)
            {
                minDis = dist;
                if (toIndex != i)
                    toIndex = i;
            }
        }
    }

    private Vector3  GetFirstNearPos()
    {
        float minDis = float.MaxValue;
        for (int i = 0; i < sidePositionList.Count; i++)
        {
            var pos = sidePositionList[i];
            float dist = Vector3.Distance(transform.position, pos);
            if (dist == 0 || CanDoNextPos(pos) == false)
                continue;

            if (dist < minDis)
            {
                minDis = dist;
                if (toIndex != i)
                    toIndex = i;
            }
        }

        return sidePositionList[toIndex];
    }

    private Vector3 GetNextPostion()
    {
        Vector3 goalPosition  = transform.position;
        // �̹� �̵� ���̶�� ������������ 
        //float checkDistance = Vector3.Distance(transform.position, goalPosition);
        bool b = false;
        //b |= checkDistance <= 0.02f;
        b |= navMeshAgent.velocity.magnitude > 0.2f;
        b |= sidePositionList.Count <= 0;
        if (b)
            return goalPosition;

        // ���� index --; // ������ inde++; 
        if (crossroads == 0)
            toIndex--;
        else
            toIndex++;


        if (toIndex < 0)
        {
            crossroads = 1;
            toIndex = 0;
        }
        else if (toIndex >= sidePositionList.Count)
        {
            crossroads = 0;
            toIndex = sidePositionList.Count - 1;
        }


        Debug.Log($"추적 경로  {toIndex}");
        goalPosition = sidePositionList[toIndex];

        return goalPosition;
    }

    private void CheckEnableMove(Vector3 targetGoal)
    {
        if (sidePositionList.Count <= 0)
            return;

        // �� ���� �� �� �ִ��� �˻� 
        path = new NavMeshPath();
        bool pathValid = navMeshAgent.CalculatePath(targetGoal, path);

        if (pathValid)
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                navMeshAgent.SetPath(path);
                navMeshAgent.isStopped = false;
            
            }
        }
        else
        {
            Debug.Log("��� ��� ����.");
        }
    }

    public void SetStrafeTarget(GameObject target)
    {
        targetObj = target;

        // 0: Left , 1 : Right 
        crossroads = UnityEngine.Random.Range(0, 2);
        Debug.Log(crossroads == 0 ? "����" : "������");
        
        RemovePathList();
        SetSideMovePostionList(target);
        SetFirstNearIndex();
    }

    public void StopStep()
    {
        path = null;
    }
    

    public void DoNextPos(GameObject target)
    {
        if (target == null)
            return;


      //  SetSideMovePostionList(target);

        //GetFirstNearPos();

        Vector3 goal = GetNextPostion();
        debugPos = goal;
        CheckEnableMove(goal);
    }


    private float CalculatePathLength(NavMeshPath path)
    {
        float length = 0.0f;
        if (path.corners.Length < 2)
            return length;

        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return length;
    }
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying == false)
            return;

        Gizmos.color = Color.green;
        if (sidePositionList == null || sidePositionList.Count <= 0)
            return;
        int count = 1; 
        foreach (Vector3 pos in sidePositionList)
        {
            Gizmos.DrawWireSphere(pos, .25f);
            Handles.color = Color.white;
            Handles.Label(pos + Vector3.up, count.ToString());
            count++; 
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(debugPos, .25f);

    }
}
