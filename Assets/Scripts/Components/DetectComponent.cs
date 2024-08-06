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

    private GameObject target;  // �ϴ� �ϳ�
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

    // ha... ���⼭ �ϴ� ����..

    // �þ߰��� ���ϴ� �Լ� 
    private Vector3 BoundaryAngle(float angle)
    {
        float rad = (angle + transform.eulerAngles.y) * Mathf.Deg2Rad;
        return new Vector3(Mathf.Sin(rad), 0, Mathf.Cos(rad));
    }


    // ���� �þ߰� ���ο� ��� �Դ��� �˻� 
    private bool CheckInTheBoundaryWithEenmy(Transform tr)
    {
        if (tr == null)
            return false;

        // Ÿ���� ����
        Vector3 targetDir = (tr.position - transform.position).normalized;
        float dot = Vector3.Dot(transform.forward, targetDir);

        dot = Mathf.Clamp(dot, -1.0f, 1.0f);

        // ���� �� ��� 
        float theta = Mathf.Acos(dot) * Mathf.Rad2Deg; // ������� rad�̹Ƿ� deg�� ��ȯ

        float distance = Vector3.Distance(
          tr.transform.position, transform.position);

        if (theta <= detectAngle * 0.5f && distance <= detectDistance )
            return true;

        return false;
    }



    // �� ���� 
    private void Update_DetectTarget()
    {
        //var overlapColliders = Physics.OverlapSphere(transform.position,
        //  detectDistance, LayerMask.GetMask("Player"));

        // �ϴ� �̸��� Player�� ����� ã�´�. 
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

                // Ÿ�� ���� 
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
