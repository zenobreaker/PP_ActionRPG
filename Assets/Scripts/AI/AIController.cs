using System;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(PerceptionComponent))]
[RequireComponent(typeof(NavMeshAgent))]
public abstract class AIController : MonoBehaviour
{

    [SerializeField] protected float attackRange = 1.5f;
    [SerializeField] private float attackDelay = 2.0f;
    [SerializeField] protected float attackDelayRandom = 0.9f;


    [SerializeField] private float damageDelay = 1.0f;
    [SerializeField] private float damageDelayRandom = 0.5f;


    [SerializeField] private string uiStateName = "EnemyAIState";

    public enum Type
    {
        Wait = 0, Patrol, Approach, Equip, Action, Damaged, Wandering,
    }
    protected Type type = Type.Wait;
    public Type MyType { get => type; }

    public event Action<Type, Type> OnAIStateTypeChanged;
    public bool WaitMode { get => type == Type.Wait; }
    public bool PatrolMode { get => type == Type.Patrol; }
    public bool ApproachMode { get => type == Type.Approach; }
    public bool EquipMode { get => type == Type.Equip; }
    public bool ActionMode { get => type == Type.Action; }
    public bool DamagedMode { get => type == Type.Damaged; }


    protected Enemy enemy;
    protected PerceptionComponent perception;
    protected NavMeshAgent navMeshAgent;
    protected Animator animator;
    protected WeaponComponent weapon;
    protected LaunchComponent launch;
    protected StateComponent state;


    protected TextMeshProUGUI userInterface;
    protected Canvas uiStateCanvas;

    private float navOriginSpeed;
    private float navOriginAngularSpeed;

    [SerializeField]
    private float currentCoolTime = 0.0f;

    [SerializeField] protected bool bDrawDebug = false;

    protected virtual void Awake()
    {
        enemy = GetComponent<Enemy>();
        Debug.Assert(enemy != null);
        perception = GetComponent<PerceptionComponent>();
        Debug.Assert(perception != null);
        navMeshAgent = GetComponent<NavMeshAgent>();
        Debug.Assert(navMeshAgent != null);
        navOriginSpeed = navMeshAgent.speed;
        navOriginAngularSpeed = navMeshAgent.angularSpeed;

        animator = GetComponent<Animator>();

        state = GetComponent<StateComponent>();

        launch = GetComponent<LaunchComponent>();
        Debug.Assert(launch != null);

        weapon = GetComponent<WeaponComponent>();
        weapon.OnEndEquip += OnEndEquip;
        weapon.OnEndDoAction += OnEndDoAction;
    }

    private void Start()
    {
        uiStateCanvas = UIHelpers.CreateBillboardCanvas(uiStateName, transform, Camera.main);

        Transform t = uiStateCanvas.transform.FindChildByName("Txt_AIState");
        userInterface = t.GetComponent<TextMeshProUGUI>();
        userInterface.text = "";

        SetWaitMode();
    }


    protected virtual void Update()
    {
        //빌보드 => 항상 카메라를 바라보게 하는 UI
        if (uiStateCanvas == null)
            return;

        userInterface.gameObject.SetActive(bDrawDebug);

        userInterface.text = type.ToString() + currentCoolTime.ToString("f2");
        uiStateCanvas.transform.rotation = Camera.main.transform.rotation;

    }
    protected bool CheckCoolTime()
    {
        if (WaitMode == false)
            return false;

        if (currentCoolTime <= 0.0f)
            return false;


        currentCoolTime -= Time.fixedDeltaTime;

        bool bCheckCoolTimeZero = false;
        bCheckCoolTimeZero |= currentCoolTime <= 0.0f;
        bCheckCoolTimeZero |= perception.GetPercievedPlayer() == null;

        if (bCheckCoolTimeZero)
        {
            currentCoolTime = 0.0f;
            navMeshAgent.updateRotation = true;
            return false;
        }

        return true;
    }

    protected bool CheckMode()
    {
        bool bCheck = false;
        // 장착할 때까지 지연시킬거임!
        bCheck |= (EquipMode == true);
        // 공격할 때도 처리 안함
        bCheck |= (ActionMode == true);
        bCheck |= (DamagedMode == true);
        bCheck |= (animator.GetBool("IsDownCondition") == true);
        bCheck |= state.DeadMode;
        bCheck |= enemy.DownCondition;

        if (state.DeadMode)
            SetNavMeshStop(true);

        return bCheck;
    }


    protected abstract void FixedUpdate();


    #region Late_Update
    // 게으른처리?? => 디퍼드 익스큐팅 => 씬그래프가 이렇게 처리함
    protected virtual void LateUpdate()
    {
        LateUpdate_SetSpeed();
        LateUpdate_Approach();
    }

    protected virtual void LateUpdate_SetSpeed()
    {
        switch (type)
        {
            case Type.Wait:
            case Type.Action:
            case Type.Damaged:
            {
                animator.SetFloat("SpeedY", 0.0f);
            }
            break;

            case Type.Patrol:
            case Type.Approach:
            case Type.Wandering:
            {
                animator.SetFloat("SpeedY", navMeshAgent.velocity.magnitude);
            }
            break;
        }
    }

    private void LateUpdate_Approach()
    {
        //if (launch.IsAir)
          //  return;
        if (ApproachMode == false)
            return;

        GameObject player = perception.GetPercievedPlayer();

        if (player == null)
            return;

        //적과 자신 사이에 다른 무언가가 있는가?
        Vector3 targetPos = player.gameObject.transform.localPosition;
        Vector3 direction = targetPos - transform.localPosition;
        float distance = direction.magnitude;
        Vector3 myPos = transform.localPosition + Vector3.up;

        Ray ray = new Ray(myPos, direction);
        Debug.DrawRay(myPos, direction, Color.blue, 5.0f);
        if (Physics.Raycast(ray, out RaycastHit hit, distance))
        {
            if (hit.collider.gameObject.tag.Equals("Player") == false)
            {
                Debug.Log("너와 나 사이에 뭔가가 있구나?");
                SetWaitMode();
                return;
            }
        }

        navMeshAgent.updateRotation = true;
        navMeshAgent?.SetDestination(player.transform.position);
    }

    #endregion


    #region SetState

    public void SetNavMeshStop(bool bStop = false)
    {
        //if (launch.IsAir)
        //    return;

        if (navMeshAgent != null)
        {
            //navMeshAgent.updateRotation = true;
            navMeshAgent.isStopped = bStop;
        }
    }

    public virtual void SetWaitMode()
    {
        if (WaitMode == true)
            return;

        SetNavMeshStop(true);
        ChangeType(Type.Wait);
    }

    public virtual void SetApproachMode()
    {
        if (ApproachMode == true)
            return;

        SetNavMeshStop(false);
        ChangeType(Type.Approach);
    }



    public void SetEquipMode(WeaponType type)
    {
        if (EquipMode == true)
            return;

        ChangeType(Type.Equip);
        //navMeshAgent.isStopped = true;

        switch (type)
        {
            case WeaponType.Sword:
            weapon.SetSwordMode();
            break;
            case WeaponType.Fist:
            weapon.SetFistMode();
            break;
            case WeaponType.Hammer:
            weapon.SetHammerMode();
            break;
            case WeaponType.FireBall:
            weapon.SetFireBallMode();
            break;
            case WeaponType.Dual:
            weapon.SetDualMode();
            break;
            default:
            Debug.Assert(false, $"{name} is not have a weapon type!");
            break;
        }
    }

    public void SetActionMode()
    {
        if (ActionMode == true)
            return;

        // 움직이지 말지는 doaction 데이터로 처리해보기
        SetNavMeshStop(true);
        //navMeshAgent.isStopped = true;
        ChangeType(Type.Action);

        GameObject player = perception.GetPercievedPlayer();

        if (player != null)
            transform.LookAt(player.transform);

        weapon.DoAction();
    }

    public void SetDamagedMode()
    {

        // 이게 필요할까? 
        // 2024 07 10 
        //if (DamagedMode == true)
        //{
        //if (coroutineEndDamage != null)
        //    StopCoroutine(coroutineEndDamage);

        //}

        // 장착 중일 때 처맞을 때 타이밍에 따라 비긴, 엔드 호출 
        if (EquipMode == true)
        {
            animator.Play("Arms", 1);

            if (weapon.IsEquippingMode() == false)
                weapon.Begin_Equip();

            weapon.End_Equip();
        }


        if (ActionMode == true)
        {
            animator.Play($"{weapon.Type}.Blend Tree", 0);

            if (animator.GetBool("IsAction") == true)
                weapon.End_DoAction();
        }

        bool bCanIsCoolTime = false;
        bCanIsCoolTime |= EquipMode;
        bCanIsCoolTime |= ApproachMode;
        bCanIsCoolTime |= perception.GetPercievedPlayer() == null;

        if (bCanIsCoolTime == true)
            currentCoolTime = -1.0f;


        if (DamagedMode == true)
            return;

        SetNavMeshStop(true);
        //navMeshAgent.isStopped = true;
        ChangeType(Type.Damaged);
    }

    protected void ChangeType(Type type)
    {
        // 이전과 현재가 같으면 막을 필요가 있을까? 있으면 O
        Type prevType = this.type;
        this.type = type;
        //Debug.Log($"state Enemy {prevType} , {type}");
        OnAIStateTypeChanged?.Invoke(prevType, type);
    }
    public void End_Damage()
    {
        // coroutineEndDamage = StartCoroutine(Wait_End_Damage());
        SetCoolTime(damageDelay, damageDelayRandom);

        SetWaitMode();
    }
    #endregion

    private void OnEndEquip()
    {
        SetWaitMode();
    }


    protected virtual void OnEndDoAction()
    {
        SetCoolTime(attackDelay, attackDelayRandom);

        SetWaitMode();
        // 여긴 마무리만 처리해야하는 로직이므로 ㄷ별도의 코드 xx 
        // coroutineEndDoAction = StartCoroutine(Wait_EndDoAction_Random());
    }

    protected virtual void SetCoolTime(float delayTime, float randomTime)
    {
        if (currentCoolTime < 0.0f)
        {
            currentCoolTime = 0.0f;

            return;
        }

        float time = 0.0f;
        time += delayTime;
        time += UnityEngine.Random.Range(-randomTime, randomTime);

        currentCoolTime = time;
    }


    public void Slow_NavMeshSpeed(float slowFactor)
    {
        navMeshAgent.speed = navMeshAgent.speed * slowFactor;
        navMeshAgent.angularSpeed = navMeshAgent.angularSpeed * slowFactor;
    }

    public void Reset_NavMeshSpeed()
    {
        navMeshAgent.speed = navOriginSpeed;
        navMeshAgent.angularSpeed = navOriginAngularSpeed;
    }
}
