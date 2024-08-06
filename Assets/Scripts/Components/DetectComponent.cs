using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.GraphicsBuffer;

public class DetectComponent : MonoBehaviour
{

    [SerializeField]
    private float detectDistance = 5.0f;
    [SerializeField]
    private float detectAngle = 90.0f;
    [SerializeField]
    private float attackDistnace = 1.5f;

    private GameObject target;  // 일단 하나
    public GameObject Target { get { return target; } }

    bool bStop = false; 

    public void Detect()
    {
        bStop = false;
    }

    public void Stop()
    {
        bStop = true; 
    }

    public bool GetAttackRange()
    {
        return attackDistnace >= Vector3.Distance(target.transform.position, transform.position);
    }

    private void Update()
    {
        if (bStop)
            return;


        Update_DetectTarget();
    }

    // ha... 여기서 일단 구현..

    // 시야각을 구하는 함수 
    private Vector3 BoundaryAngle(float angle)
    {
        float rad = (angle + transform.eulerAngles.y) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
    }


    // 적이 시야각 내부에 들어 왔는지 검사 
    private bool CheckInTheBoundaryWithEenmy(Transform tr)
    {
        if (tr == null)
            return false;

        // 타겟의 방향
        Vector3 targetDir = (tr.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, targetDir);

        dot = Mathf.Clamp(dot, -1.0f, 1.0f);

        // 내적 각 계산 
        float theta = Mathf.Acos(dot) * Mathf.Rad2Deg; // 계산결과는 rad이므로 deg로 변환

        float distance = Vector3.Distance(
          tr.transform.position, transform.position);

        if (theta <= detectAngle * 0.5f && distance <= detectDistance )
            return true;

        return false;
    }



    // 적 감지 
    private void Update_DetectTarget()
    {
        //var overlapColliders = Physics.OverlapSphere(transform.position,
        //  detectDistance, LayerMask.GetMask("Player"));

        // 일단 이름이 Player인 대상을 찾는다. 
        var overlapColliders = Physics.OverlapSphere(transform.position,
          detectDistance);

        target = null;
        foreach (Collider collider in overlapColliders)
        {
            if (CheckInTheBoundaryWithEenmy(collider.transform) == true)
            {
                if (collider.gameObject.name.Equals("Player") == false)
                    continue;

                Debug.Log($"Find Enemy!");

                // 타겟 설정 
                target = collider.gameObject;
                return;
            }
        }
        Debug.Log($"Not Find Enemy!");
    }


    private void OnDrawGizmos()
    {
        if (Selection.activeGameObject != gameObject)
            return;


        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectDistance);

        Gizmos.color = Color.red;
        Vector3 left = BoundaryAngle(detectAngle * -1 * 0.5f);
        Vector3 right = BoundaryAngle(detectAngle * 0.5f);

        Gizmos.DrawRay(transform.position, left * detectDistance);
        Gizmos.DrawRay(transform.position, right * detectDistance);


        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, attackDistnace);
    }

}
