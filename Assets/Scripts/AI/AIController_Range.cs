using UnityEngine;
using UnityEngine.AI;

public abstract class AIController_Range : AIController
{


    [SerializeField]
    private float avoidRange = 5.0f;

    [SerializeField]
    private Vector2 backDistance = new Vector2(3, 10);

    [SerializeField]
    private float backRange = 3.0f;

    protected override void Awake()
    {
        base.Awake();
    }


    protected override void FixedUpdate()
    {
        if (CheckCoolTime())
            return;

        if (CheckMode())
            return;

        GameObject player = perception.GetPercievedPlayer();

        if (player == null)
        {
            if (weapon.UnarmedMode == false)
                weapon.SetUnarmedMode();

            SetWaitMode();

            return;
        }

        // 1. 감지 영역 - 파이어볼
        float temp = Vector3.Distance(transform.position, player.transform.position);
        if (temp >= avoidRange)
        {
            if (weapon.FireBallMode == false)
            {
                SetEquipMode(WeaponType.FireBall);

                return;
            }

            if (weapon.FireBallMode)
                SetActionMode();

            return;
        }


        // 2. 도망
        if (weapon.WarpMode == false)
        {
            Debug.Log("전환에 문제가 있나?");
            SetEquipMode(WeaponType.Warp);

            return;
        }

        if (weapon.WarpMode)
            DoActionWarp(player.transform);

    }

    private void DoActionWarp(Transform avoidTransform)
    {
        Vector3 position = GetAvoidPosition(avoidTransform);

        transform.LookAt(position);
        weapon.SetWarpPosition(position);

        SetActionMode();
    }

    private Vector3 GetAvoidPosition(Transform avoidTransform)
    {
        Vector3 range = new Vector3();
        range.x = Random.Range(-backRange, backRange);
        range.z = Random.Range(-backRange, backRange);

        float distance = 0.0f;
        Vector3 direction = Vector3.zero;
        Vector3 position = Vector3.zero;

        NavMeshPath path = new NavMeshPath();
        for (int i = 0; i < 5; i++)
        {
            distance = Random.Range(backDistance.x, backDistance.y);
            direction = avoidTransform.position - transform.position;

            position = avoidTransform.position + (direction.normalized * distance);
            position += range;

            if (navMeshAgent.CalculatePath(position, path))
                return position;
        }

        distance = Random.Range(backDistance.x, backDistance.y);
        direction = transform.position - avoidTransform.position;

        position = avoidTransform.position + (direction.normalized * distance);
        position += range;
        //TODO: 이곳도 트라이해야한다.
        return position;
    }



#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying == false)
            return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, avoidRange);
    }
#endif

}
