using UnityEditor;
using UnityEngine;

public class AIController_Patrol : AIController
{
    public enum WaitState
    {
        Idle = 0, BackStep, Strafe,
    }

    protected WaitState waitState;

    private bool IdleState { get => waitState == WaitState.Idle; }
    private bool StrafeState { get => waitState == WaitState.Strafe; }
    private bool BackStepState { get => waitState == WaitState.BackStep; }

    private void SetIdletate()
    {
        waitState = WaitState.Idle;
    }

    private void SetStrafeState()
    {
        waitState = WaitState.Strafe;
    }

    private void SetBackStep()
    {
        waitState = WaitState.BackStep;
    }

    protected PatrolComponent patrol;
    protected SideStepComponent sideStep;

    protected override void Awake()
    {
        base.Awake();

        patrol = GetComponent<PatrolComponent>();
        sideStep = GetComponent<SideStepComponent>();

        Debug.Assert(patrol != null);
    }


    protected override void Update()
    {
        base.Update();

        userInterface.text += "\n" + waitState.ToString();
    }

    protected override void LateUpdate()
    {
        bool bCheck = false;
        bCheck |= state.DeadMode;
        bCheck |= enemy.DownCondition;
        if (bCheck)
        {
            Debug.Log("is Dead");
            SetNavMeshStop(true);
            return;
        }

        base.LateUpdate();

        LateUpdate_StrafeWalk();
        LateUpdate_BackStep();
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

            if (patrol == null)
            {
                SetWaitMode();

                return;
            }

            SetPatrolMode();

            return;
        }

        if (weapon.UnarmedMode == true)
        {
            SetEquipMode(enemy.weaponType);
            return;
        }

        // 공격 조건 
        float temp = Vector3.Distance(transform.position, player.transform.position);
        if (temp < attackRange)
        {
            if (weapon.UnarmedMode == false)
            {
                SetActionMode();
                return;
            }
        }

        //SetWaitMode();
        SetApproachMode();
    }

    protected override void LateUpdate_SetSpeed()
    {
        base.LateUpdate_SetSpeed();

        if (WaitMode == false)
            return;
        switch (waitState)
        {
            case WaitState.Idle:
            {
                animator.SetFloat("SpeedY", 0.0f);
            }
            break;
            case WaitState.BackStep:
            {
                animator.SetFloat("SpeedY", navMeshAgent.velocity.magnitude * -1.0f);
            }
            break;
            case WaitState.Strafe:
            {
                animator.SetFloat("SpeedY", navMeshAgent.velocity.magnitude);
            }
            break;
        }
    }
    public void LateUpdate_StrafeWalk()
    {
        bool bCheck = true;
        bCheck &= WaitMode;
        bCheck &= StrafeState;
        bCheck &= !state.DeadMode;
        bCheck &= !enemy.DownCondition;
        if (bCheck == false)
            return;

        sideStep.DoStep();
    }

    public void LateUpdate_BackStep()
    {
        bool bCheck = true;
        bCheck &= WaitMode;
        bCheck &= BackStepState;
        bCheck &= !state.DeadMode;
        bCheck &= !enemy.DownCondition;
        if (bCheck == false)
            return;

        Vector3 position = transform.position;
        Vector3 behindPos = -transform.forward;
        // 박스 캐스트의 시작 위치를 뒤쪽으로 약간 이동
        Vector3 castStartPosition = position + behindPos * 0.5f; // 캐릭터 위치에서 뒤로 0.5f 만큼 이동
        // 뒷편에 검사를 해서 뒷걸음질 할 수 있는지 검사
        RaycastHit[] hits = Physics.BoxCastAll(castStartPosition, transform.lossyScale, behindPos,
            transform.rotation, 1.0f);


        Debug.DrawLine(position + Vector3.up, Vector3.up + position + behindPos * 1.0f, Color.red);
        int otherCount = 0;
        foreach (RaycastHit hit in hits)
        {
            if (hit.transform.gameObject == this.gameObject)
                continue;
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
                continue;

            //Debug.Log($"{hit.transform.name}");
            otherCount++;
        }

        if (otherCount != 0)
            return;

        navMeshAgent.updateRotation = false;
        Vector3 targetPos = CalcBehindPosition();
        navMeshAgent.SetDestination(targetPos);
    }

    private Vector3 CalcBehindPosition()
    {
        // 현재 위치와 뒤쪽 방향 계산
        Vector3 characterPosition = transform.position;
        Vector3 backwardDirection = -transform.forward * (navMeshAgent.stoppingDistance + 1.0f);
        Vector3 behindPosition = characterPosition + backwardDirection;

        return behindPosition;
    }


    public void SetPatrolMode()
    {
        if (PatrolMode == true)
            return;

        ChangeType(Type.Patrol);

        SetNavMeshStop(false);
        //navMeshAgent.isStopped = false;
        patrol.StartMove();
    }

    public override void SetWaitMode()
    {
        // 0 : Idle 1 : Strafe 2 : back step
        int typeRand = UnityEngine.Random.Range(0, 3);
        ChangeType(Type.Wait);

        ChangeWaitState((WaitState)typeRand);

        if (typeRand != 0)
            return;

        base.SetWaitMode();
    }

    private void ChangeWaitState(WaitState newState)
    {
        WaitState prevState = waitState;
        waitState = newState;

        sideStep.StopStep();

        if (waitState == WaitState.Idle)
        {
            SetNavMeshStop(true);

            return;
        }

        SetNavMeshStop(false);

        if (waitState == WaitState.BackStep)
        {
            return;
        }

        if (waitState == WaitState.Strafe)
        {
            GameObject player = perception.GetPercievedPlayer();
            if (player == null)
                return;

            SetCoolTime(5.0f, 0.5f);
            sideStep.SetStrafeTarget(player);
            return;
        }
    }


#if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;
        if (Selection.activeGameObject != gameObject)
            return;
        if (waitState == WaitState.BackStep)
        {
            Gizmos.color = Color.green;
            Vector3 boxSize = transform.lossyScale;
            // 캐릭터의 뒤쪽 위치 계산
            Vector3 behindPosition = transform.position - transform.forward * navMeshAgent.stoppingDistance;
            Gizmos.DrawWireCube(behindPosition, boxSize);

            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(CalcBehindPosition(), 0.5f);
        }
    }

#endif
}
