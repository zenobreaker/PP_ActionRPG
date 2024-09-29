using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 캡슐 형태로 점진적으로 커져 나가는 형태이다. 
/// </summary>
public class DragonBreath : MonoBehaviour
{
    public GameObject owner;
    public DoActionData actionData;

    public float initialLength = 1f;    // 초기 캡슐 길이
    public float maxLength = 10f;       // 최대 캡슐 길이
    public float growthRate = 2;        // 캡슐이 자라나는 속도 (초당 증가량)
    public float radius = 1f;           // 캡슐의 반지름
    public Vector3 direction = Vector3.forward;     // 진행 방향
    public float maxDistance = 10f;     // 감지할 최대 거리

    public float attackDelay = 0.5f;    // 피격 간격 
    private float currentDelay = 0;

    private float currentLength;        // 최대 캡슐 길이

    private List<GameObject> hitList = new List<GameObject>();
    private void Start()
    {
        currentLength = initialLength;
    }

    private void Update()
    {
        // 매 프레임마다 캡슐 길이를 증가시킴.
        if (currentLength < maxLength)
            currentLength += growthRate * Time.deltaTime;

        // 캡슐의 두 끝점을 계산한다. 
        Vector3 point1 = transform.position;
        Vector3 point2 = transform.position + transform.forward * currentLength; // 캡슐의 긑이 길어 진다.


        if (currentDelay <= 0)
        {
            // CapsuleCast 호출 (브레스 범위에 따른 감지)
            Collider[] colliders = Physics.OverlapCapsule(point1, point2, radius);

            foreach (Collider collider in colliders)
            {
                if (owner == collider.gameObject)
                    continue;
                if (hitList.Contains(collider.gameObject))
                    continue;
                hitList.Add(collider.gameObject);
            }

            currentDelay = attackDelay;
            foreach (GameObject go in hitList)
            {
                IDamagable damagable = go.GetComponent<IDamagable>();
                if (damagable != null)
                {
                    damagable.OnDamage(owner, null, Vector3.zero, actionData);
                }
            }
            hitList.Clear();
        }
        else
            currentDelay -= Time.deltaTime;
    }


#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        Vector3 point1 = transform.position;
        Vector3 point2 = transform.position + transform.forward * currentLength;

        Gizmos.color = Color.green;
        Gizmos.DrawLine(point1, point2);
    }
#endif
}
