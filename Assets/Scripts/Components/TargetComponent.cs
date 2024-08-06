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
    [SerializeField] private float rotateSpeed = 1.0f; // Ÿ���� ������ ȸ���� �ӵ� 

    private GameObject targetObject;
    private float deltaRotation = 0.0f;
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

        // ���� ����� ��� 
        GameObject nearlyObject = GetNearlyFrontAngle(candinates);

        ChangeTarget(nearlyObject);

        // ����� ���� ȸ��
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

        Quaternion from = transform.localRotation;
        Quaternion to = Quaternion.LookRotation(direction.normalized, Vector3.up);

        if(Quaternion.Angle(from,to) < 2.0f)
        {
            deltaRotation = 0.0f;
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


        // �����̴� ���� �� �Է� ���� �ʵ��� 
        if (bMovingFocus == true)
            return;

        bMovingFocus = true;


        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, layerMask.value);

        // ���� , �׿�����
        Dictionary<float, GameObject> candinateTable = new Dictionary<float, GameObject>();
        foreach (Collider collider in colliders)
        {
            if (targetObject == collider.gameObject)
                continue;

            Vector3 vec1 = collider.transform.position;
            Vector3 vec2 = transform.position;
            Vector3 direction = vec1 - vec2;

            // ���ణ �Ÿ� ���� 
            Vector3 cross = Vector3.Cross(transform.forward, direction);
            float distance = Vector3.Dot(cross, Vector3.up); // <- y�� �����ϱ� ������ ������ �����ϸ� ���ణ�� �Ÿ��� ���´�

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



        deltaRotation = 0.0f;
        targetObject = null;

        // �ĺ��ڰ� ������ �׳� �� Ǯ��.. 
        //if (bLookForward == true)
        //    transform.localRotation = Quaternion.identity;
    }

}
