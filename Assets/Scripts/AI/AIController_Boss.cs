using System;
using System.Collections;
using System.Data.SqlTypes;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class AIController_Boss : AIController
{
    public enum WaitState
    {
        Idle = 0, BackStep, Strafe,
    }

    private bool IdleState { get => waitState == WaitState.Idle; }
    private bool StrafeState { get => waitState == WaitState.Strafe; }
    private bool BackStepState { get => waitState == WaitState.BackStep; }

    protected WaitState waitState;

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

    [Flags]
    enum BossPattern 
    {
        None = 0, 
        Pattern1 = 1,
        Pattern2 = 2,
        Pattern3 = 3,
        Max,
    }

    private BossPattern bossPattern = BossPattern.None;
    private Vector3 wanderingPosition = Vector3.zero;
    public bool WanderingMode { get => type == Type.Wandering; }

    
    protected BossActionComponent bossAction;
    protected SideStepComponent sideStep;

    private bool bCanMove = true; 
    public bool CanMove { get => bCanMove; set => bCanMove = value; }

    private float bossOriginSpeed = 1.0f;

    protected override void Awake()
    {
        base.Awake();   

        //patrol = GetComponent<PatrolComponent>();
        bossAction = GetComponent<BossActionComponent>();
        sideStep = GetComponent<SideStepComponent>();

        weapon.OnEndDoAction += OnBossEndAction;

        bossOriginSpeed = navMeshAgent.speed;
    }

    private void OnEnable()
    {
      //  SetEquipMode(enemy.weaponType);
    }

    protected override void Update()
    {
        base.Update();

        uiStateCanvas.gameObject.SetActive(bDrawDebug);
        userInterface.text +="\n";
        //TODO: 상태 적어보기 
        userInterface.text += bossPattern.ToString() +"\n";
        userInterface.text += $"pattern1 "+fireballMaxCoolTime.ToString("f2") + "can : " +  bCanfireball.ToString() +"\n";
        userInterface.text += $"pattern2 " + comboMaxCoolTime.ToString("f2") + "can : " +bCanCombo+"\n";
        userInterface.text += waitState.ToString();
        //userInterface.text += $"pattern2 " + comboMaxCoolTime.ToString("f2") + "can : " + bCanCombo + "\n";
        //userInterface.text += $"pattern3 " + fireballCoolTime.ToString("f2") + "\n";
    }

    protected override void FixedUpdate()
    {
        if (bCanMove == false)
            return; 

        if (CheckCoolTime())
            return;

        if (CheckMode())
            return;

        if (WanderingMode)
            return;


        GameObject player = perception.GetPercievedPlayer();

        if (player == null)
            SetWaitMode();
        else
        {
            if (weapon.UnarmedMode == true)
            {
                SetEquipMode(enemy.weaponType);
                return;
            }

            // 공격 조건 확인
            SetBossAttackMode(player);

            if (bossPattern == BossPattern.Pattern2 || bossPattern == BossPattern.Pattern3)
            {
                float temp = Vector3.Distance(transform.position, player.transform.position);
                if (temp > attackRange)
                {
                    SetApproachMode();
                    return;
                }
            }
            
            // 공격
            DoDecidedPattern();
        }
        
    }

    protected override void LateUpdate()
    {
        if (state.DeadMode)
            return;

        if (condition.DownCondition)
        {
            Debug.Log("is Down");
            return;
        }


        if (bCanMove == false)
            return;

        base.LateUpdate();
        
        LateUpdate_StrafeWalk();
        LateUpdate_BackStep();
        //LateUpdate_Wandering();
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

    private void LateUpdate_Wandering()
    {
        //if (launch.IsAir)
        //    return;
        if (WanderingMode == false)
            return;
        
        if (CheckArrivedWandering())
        {
            OnWanderingArrived();
            return; 
        }

        GameObject player = perception.GetPercievedPlayer();

        if(player != null)
            transform.LookAt(player.transform, Vector3.up);

        navMeshAgent.SetDestination(wanderingPosition);
    }
    public void LateUpdate_StrafeWalk()
    {
        bool bCheck = true;
        bCheck &= WaitMode;
        bCheck &= StrafeState;
        if (bCheck == false)
            return;

        sideStep.DoStep();
    }

    public void LateUpdate_BackStep()
    {
        bool bCheck = true;
        bCheck &= WaitMode;
        bCheck &= BackStepState;
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


    //public void SetPatrolMode()
    //{
    //    if (PatrolMode == true)
    //        return;

    //    ChangeType(Type.Patrol);

    //    SetNavMeshStop(false);
    //    //navMeshAgent.isStopped = false;
    //    patrol.StartMove();
    //}

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

        GameObject player = perception.GetPercievedPlayer();
        if (player == null)
            return;
        
        navMeshAgent.speed = bossOriginSpeed * 0.5f;

        if (waitState == WaitState.BackStep)
        {
            return;
        }

        if (waitState == WaitState.Strafe)
        {

            SetCoolTime(5.0f, 0.5f);
            sideStep.SetStrafeTarget(player);
            return;
        }
    }
    public override void SetApproachMode()
    {
        base.SetApproachMode();
        navMeshAgent.speed = bossOriginSpeed;
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

    public void SetWanderingMode()
    {
        if (WanderingMode == true)
            return;

        ChangeType(Type.Wandering);
        GameObject player = perception.GetPercievedPlayer();
        if(player != null)
            transform.LookAt(player.transform.position, Vector3.up);

        SetNavMeshStop(false);
    }
 

    // 공격 패턴 정리 
    private void SetBossAttackMode(GameObject player)
    {
        if (ActionMode)
            return;
        if (WanderingMode)
            return;
        if (bStart)
            return;

        bool bDecide = true;
        var randPattern = (BossPattern)Random.Range(1, (int)BossPattern.Max);

        if (randPattern == BossPattern.Pattern1)
        {
            bDecide &= CanShootFireball();
        }

        if (randPattern == BossPattern.Pattern2)
        {
            bDecide &= CanComboAttack();
        }

        if (bDecide)
        {
            ChangeBossPattern(randPattern);
            CheckPatternKeepTime();
        }

    }

    private void DoDecidedPattern()
    {
        if (bossPattern == BossPattern.None)
            return;
        


        SetNavMeshStop(true);
        ChangeType(Type.Action);

        if (bossPattern == BossPattern.Pattern1)
        {
            ShootFireball();
        }
        if (bossPattern == BossPattern.Pattern2)
        {
            DoComboAttack();
        }
        if (bossPattern == BossPattern.Pattern3)
        {
            DoNormalAttack();
        }


        bossPattern = BossPattern.None;
    }

    #region Pattern1 
    // 패턴이 결정이 되면 해당 패턴을 일정 시간 이내에 발동시켜야 한다. 
    // 그러기 위해선 근접 기술에 경우 접근하던가 원거리 기술에 경우 조건을 결정 

    private bool bCanfireball = true;
    private float fireballMaxCoolTime = 5.0f;
    private Coroutine firballCoroutine = null;
    
    [SerializeField] private float usedDistance = 5.0f; 
    private bool CanShootFireball()
    {
        GameObject player = perception.GetPercievedPlayer();
        
        bool bCheck = false;
        bCheck |= bCanfireball == false;
        bCheck |= player == null;
        
        if (bCheck)
            return false;
        
        float distance = Vector3.Distance(this.transform.position, player.transform.position);
        if( distance <= usedDistance)
            return false;

        return true; 
    }

    private void ShootFireball()
    {
        firballCoroutine = StartCoroutine(CoolDown_Fireball());

        GameObject player = perception.GetPercievedPlayer();
        if (player != null)
        {
            //Vector3 direction = player.transform.localPosition - transform.localPosition;
            //transform.localRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
            transform.LookAt(player.transform, Vector3.up);
            //transform.localRotation = Quaternion.LookRotation(direction);

            //TODO: 파이어볼 발사 
            bossAction.DoPattern(2);
        }
    }


    private IEnumerator CoolDown_Fireball()
    {
        bCanfireball = false;

        float time = 0.0f;
        time += fireballMaxCoolTime;
        time += Random.Range(-attackDelayRandom, -attackDelayRandom);
    
        yield return new WaitForSeconds(time);
        bCanfireball = true;
        
    }

    #endregion

    #region Pattern2
    private bool bCanCombo = true;
    private float comboMaxCoolTime = 5.0f;
    private bool CanComboAttack()
    {
        return bCanCombo;
    }

    private void DoComboAttack()
    {
        StartCoroutine(CoolDown_ComboAttack());
        //TODO: 콤보 
        // 이 공격 동안 슈퍼아머 상태 - 공격이 끝나면 슈퍼아머 변수 해제 
        animator.SetBool("IsAction", true);
        animator.SetInteger("Pattern",1);
    }

    IEnumerator CoolDown_ComboAttack()
    {
        bCanCombo = false;

        float time = 0.0f;
        time += comboMaxCoolTime;
        time += Random.Range(-attackDelayRandom, -attackDelayRandom);

        yield return new WaitForSeconds(time);
        bCanCombo = true;
    }


    #endregion

    #region Wandering
    private bool bCanWander = true;
    private float wanderMaxCoolTime = 10.0f;
    private void DoWandering()
    {
        if (bCanWander == false)
        {
            OnEndDoAction();
            return;
        }
        StartCoroutine(CoolDown_Wandering());

        wanderingPosition = GetWanderingPoisition();
        //Debug.Log($"Wandering : {wanderingPosition}");
        SetWanderingMode();
    }
    IEnumerator CoolDown_Wandering()
    {
        bCanWander = false;

        float time = 0.0f;
        time += wanderMaxCoolTime;
        time += Random.Range(-attackDelayRandom, -attackDelayRandom);

        yield return new WaitForSeconds(time);
        bCanWander = true;
    }


    private Vector3 GetWanderingPoisition()
    {
        float wanderRadius = 5f; 
        Vector3 position = Vector3.zero;
        
        NavMeshPath path = new NavMeshPath();
        for (int i = 0; i < 5; i++)
        {
            position = Random.insideUnitSphere * wanderRadius;

            if (navMeshAgent.CalculatePath(position, path))
                return position;
        }
        
        return position;
    }

    private bool CheckArrivedWandering()
    {
        return MathHelpers.IsNearlyEqual(transform.position.magnitude, wanderingPosition.magnitude, 1.5f);
    }

    private void OnWanderingArrived()
    {
        OnEndDoAction();
    }
    #endregion

    private bool CanNormalAttack()
    {
        bool bCheck = true; 
        GameObject player = perception.GetPercievedPlayer();
        float temp = Vector3.Distance(transform.position, player.transform.position);
        
        bCheck &= player != null;
        bCheck &= (temp <= attackRange);

        return bCheck;
    }
    private void DoNormalAttack()
    {

        GameObject player = perception.GetPercievedPlayer();

        if (player != null)
            transform.LookAt(player.transform);
        animator.SetInteger("Pattern", 0);
        weapon.DoAction();
    }

    // 반격 어떻게 넣지 
    private bool bStart;
    private Coroutine coroutineCheckUsedPattern;
    private void CheckPatternKeepTime()
    {
        if(bStart == false)
        {
            if(coroutineCheckUsedPattern != null)
                StopCoroutine(coroutineCheckUsedPattern);
            coroutineCheckUsedPattern = StartCoroutine(CheckUsedPattern());
        }

    }

    private IEnumerator CheckUsedPattern()
    {
        bStart = true;
        Debug.Log("타이머 시작");
        yield return new WaitForSeconds(4.0f);
        Debug.Log("타이머 끝");
        // 일정 시간 뒤에 검사해보니 아직도 패턴을 갖고 있다면 
        if (bossPattern != BossPattern.None)
        {
            Debug.Log("행동 종료");
            // 그냥 행동을 종료 
            OnBossEndAction();
        }
    }

    private void OnBossEndAction()
    {
        animator.SetInteger("Pattern", 0);
        ChangeBossPattern(BossPattern.None);
        bStart = false;

        SetWaitMode();
    }

    private void ChangeBossPattern(BossPattern pattern)
    {
        BossPattern prevPattern = bossPattern;
        bossPattern = pattern;

        Debug.Log($"이전 패턴 {prevPattern} / 결정 {bossPattern}");
    }


#if UNITY_EDITOR

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying == false)
            return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, usedDistance);
    }
#endif

}
