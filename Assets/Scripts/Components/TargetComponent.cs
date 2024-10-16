using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetComponent : MonoBehaviour
{
    [SerializeField] private float radius = 1f;
    [SerializeField] private LayerMask layerMask;
    //[SerializeField] private float rotateSpeed = 1.0f; // 타게팅 대상까지 회전할 속도 

    private GameObject targetObject;
    //private float deltaRotation = 0.0f;
    private bool bMovingFocus;

    private void Awake()
    {
        PlayerInput input = GetComponent<PlayerInput>();
        Debug.Assert(input != null);
        InputActionMap actionMap = input.actions.FindActionMap("Player");
        Debug.Assert(actionMap != null);

    
    }

    public void Begin_Targeting(bool bRotation = false)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, layerMask.value);

        GameObject[] candinates = colliders.Select(colliders => colliders.gameObject).ToArray();

        // 가장 가까운 대상 
        GameObject nearlyObject = GetNearlyFrontAngle(candinates);

        ChangeTarget(nearlyObject);

        // 대상을 향해 회전
        if (bRotation == false)
            return;

        RotateToTarget();
    }

    private GameObject GetNearlyFrontAngle(GameObject[] candinates)
    {
        Vector3 position = transform.position;

        GameObject candinate = null;
        float minAngle = float.MinValue;
        float minDistance = float.MaxValue;

        foreach (GameObject obj in candinates)
        {
            Vector3 enemyPosition = obj.transform.position;
            Vector3 direction = enemyPosition - position;
            float distance = Vector3.Distance(enemyPosition, position); 

            float angle = Vector3.Dot(transform.forward, direction.normalized);

            if (angle < 1.0f - 0.5f)
                continue; 

            if(minDistance > distance)
            {
                minDistance = distance; 
                minAngle = angle;
                candinate = obj;
            }
        }

        return candinate;
    }

    private void ChangeTarget(GameObject target)
    {
        if (target == null)
        {
            EndTargeting(true);
            return;
        }

        EndTargeting();

        targetObject = target;
    }


    private void RotateToTarget()
    {
        if (targetObject == null)
        {
            return;
        }

        Vector3 position = transform.position;
        Vector3 targetPosition = targetObject.transform.position;
        Vector3 direction = targetPosition - position;
        direction.y = 0.0f;

        Quaternion from = transform.localRotation;
        Quaternion to = Quaternion.LookRotation(direction.normalized, Vector3.up);

        if(Quaternion.Angle(from,to) < 2.0f)
        {
            //deltaRotation = 0.0f;
            transform.localRotation = to;

            return; 
        }

        transform.localRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        //deltaRotation += rotateSpeed * Time.deltaTime;
        //transform.localRotation = Quaternion.RotateTowards(from, to, deltaRotation);
    }

    private void ChangeFocucs(bool bRight)
    {
        if (targetObject == null)
            return;


        // 움직이는 중일 땐 입력 받지 않도록 
        if (bMovingFocus == true)
            return;

        bMovingFocus = true;


        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, layerMask.value);

        // 외적 , 겜오브젝
        Dictionary<float, GameObject> candinateTable = new Dictionary<float, GameObject>();
        foreach (Collider collider in colliders)
        {
            if (targetObject == collider.gameObject)
                continue;

            Vector3 vec1 = collider.transform.position;
            Vector3 vec2 = transform.position;
            Vector3 direction = vec1 - vec2;

            // 평행간 거리 등장 
            Vector3 cross = Vector3.Cross(transform.forward, direction);
            float distance = Vector3.Dot(cross, Vector3.up); // <- y값 추출하기 외적에 내적을 추출하면 평행간에 거리가 나온다

            candinateTable.Add(distance, collider.gameObject);
        }

        float minmum = float.MaxValue;
        GameObject candinate = null;

        foreach (float distance in candinateTable.Keys)
        {
            if (Mathf.Abs(distance) >= minmum)
                continue;

            if (bRight && distance > 0.0f)
            {
                minmum = Mathf.Abs(distance);
                candinate = candinateTable[distance];
            }

            if (bRight == false && distance < 0.0f)
            {
                minmum = Mathf.Abs(distance);
                candinate = candinateTable[distance];
            }
        }

        ChangeTarget(candinate);
    }



    private void EndTargeting(bool bLookForward = false)
    {

        if (targetObject != null)
        {
            Transform particle = targetObject.transform.FindChildByName("Target(Clone)");

            if (particle != null)
                Destroy(particle.gameObject);
        }



        //deltaRotation = 0.0f;
        targetObject = null;

        // 후보자가 없으면 그냥 다 풀어.. 
        //if (bLookForward == true)
        //    transform.localRotation = Quaternion.identity;
    }

}
