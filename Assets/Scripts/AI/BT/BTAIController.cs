using System;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.AI;
using static BTNode;

public class BTAIController : MonoBehaviour
{
    /// <summary>
    /// 공격 관련 
    /// </summary>
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackDelay = 2.0f;
    [SerializeField] private float attackDelayRandom = 0.9f;

    /// <summary>
    /// 데미지 관련 
    /// </summary>
    [SerializeField] private float damageDelay = 1.0f;
    [SerializeField] private float damageDelayRandom = 0.5f;

    /// <summary>
    /// 대기 관련
    /// </summary>
    [SerializeField] private float waitDelay = 2.0f;
    [SerializeField] private float waitDelayRandom = 0.5f; // goalDelay +( - 랜덤 ~ +랜덤)

    [SerializeField] private string uiStateName = "EnemyAIState";
    public enum Type
    {
        Wait = 0, Patrol, Approach, Equip, Action, Damaged,
    }
    private Type type = Type.Wait;
    public Type MyType { get => type; }

    public event Action<Type, Type> OnAIStateTypeChanged;
    public bool WaitMode { get => type == Type.Wait; }
    public bool PatrolMode { get => type == Type.Patrol; }
    public bool ApproachMode { get => type == Type.Approach; }
    public bool EquipMode { get => type == Type.Equip; }
    public bool ActionMode { get => type == Type.Action; }
    public bool DamagedMode { get => type == Type.Damaged; }

    private bool percept = false;
    public bool Percepted { get => percept; set => percept = value; }

    private Enemy enemy;
    private StateComponent state;
    private PerceptionComponent perception;
    private NavMeshAgent navMeshAgent;
    private Animator animator;
    private PatrolComponent patrol;
    private WeaponComponent weapon;
    private LaunchComponent launch;



    private TextMeshProUGUI userInterface;
    private Canvas uiStateCanvas;

    private BTNode root; // BTNode의 루트 

  //  public event Action OnBeginEquipWeapon;
   // public event Action OnEndEquipWeapon;

    [SerializeField] private float currentCoolTime = 0.0f;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        Debug.Assert(enemy != null);
        perception = GetComponent<PerceptionComponent>();
        Debug.Assert(perception != null);
        navMeshAgent = GetComponent<NavMeshAgent>();
        Debug.Assert(navMeshAgent != null);
        animator = GetComponent<Animator>();
        patrol = GetComponent<PatrolComponent>();
        Debug.Assert(patrol != null);
        launch = GetComponent<LaunchComponent>();
        Debug.Assert(launch != null);

        state = enemy.GetComponent<StateComponent>();

        weapon = GetComponent<WeaponComponent>();
        //weapon.OnEndEquip += OnEndEquip;
        weapon.OnEndDoAction += OnEndDoAction;
    }


    private void Start()
    {
        uiStateCanvas = UIHelpers.CreateBillboardCanvas(uiStateName, transform, Camera.main);

        Transform t = uiStateCanvas.transform.FindChildByName("Txt_AIState");
        userInterface = t.GetComponent<TextMeshProUGUI>();
        userInterface.text = "";

        //SetWaitMode();

        ConstructBehaviourTree();
    }

    private void Update()
    {
        if (root == null)
            return;

        root.Evaluate();
    }

    private void LateUpdate()
    {
        LateUpdate_DrawUI();
        LateUpdate_SetSpeed();
    }
    private void LateUpdate_DrawUI()
    {
        //빌보드 => 항상 카메라를 바라보게 하는 UI
        if (uiStateCanvas == null)
            return;

        userInterface.text = type.ToString() + currentCoolTime.ToString("f2");
        uiStateCanvas.transform.rotation = Camera.main.transform.rotation;

    }

    private void LateUpdate_SetSpeed()
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
            {
                animator.SetFloat("SpeedY", navMeshAgent.velocity.magnitude);
            }
            break;
        }
    }


    #region Set_BTNode

    private BTNode.NodeState Func_DamagedWithEquipMode()
    {
        if (state.DamagedMode == false)
            return BTNode.NodeState.Success;

        // 장착 중일 때 처맞을 때 타이밍에 따라 비긴, 엔드 호출 
        if (EquipMode == true)
        {
            animator.Play("Arms", 1);

            if (weapon.IsEquippingMode() == false)
                weapon.Begin_Equip();

            weapon.End_Equip();
        }

        return BTNode.NodeState.Failure;
    }

    private void ConstructBehaviourTree()
    {
        //감지 
        //var perceptionNode = BTNodeFactory.CreateSelectorNode(new PerceptionNode(this, perception), 
        //    new InvertorNode(new ChangeStateNode(this)));

        // 순찰 
        var patrolNode = BTNodeFactory.CreateSequenceNode(
            new UnequipNode(Action_DoUnequip),
            new SetPatrolNode(this, patrol),
            new GetRandomLocationNode(patrol, GetRandomPosition),
            //new MoveToNode(Action_MoveTo),
            new WaitNode(this, 2.0f, true, () => patrol.Arrived = false)
            /*new WaitNode(DoWait)*/);


        var patrolAndPerception = new PerceptionDecorator(patrolNode, perception);

        // 장착
        var equipNode = BTNodeFactory.CreateSequenceNode(new EquipNode(Aciton_DoEquip));
        // 추적
        var chaseNode = BTNodeFactory.CreateSequenceNode(equipNode,
            new ActionNode(Condition_Approach),
            new ActionNode(Action_ChaseTo));
        // 공격
        var attackNode = BTNodeFactory.CreateSequenceNode(new ActionNode(Condition_CheckAttack),
            new ActionNode(Action_DoAttack),
            new WaitNode(this, 2.0f, true, () => { bAttacking = false; }));

        root = BTNodeFactory.CreateSelectorNode(patrolAndPerception, chaseNode, attackNode);
        Debug.Assert(root != null, "root가 생성되지 않았습니다.");
    }
    #endregion
    private void SetCoolTime(float delayTime, float randomTime)
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
    public bool CheckCoolTime()
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

            return false;
        }

        return true;
    }

    private bool CheckMode()
    {
        bool bCheck = false;
        // 장착할 때까지 지연시킬거임!
        bCheck |= (EquipMode == true);
        // 공격할 때도 처리 안함
        bCheck |= (ActionMode == true);
        bCheck |= (DamagedMode == true);
        bCheck |= (animator.GetBool("IsDownCondition") == true);

        return bCheck;
    }


    #region SetState

    public void SetNavMeshStop(bool bStop = false)
    {
        //if (launch.IsAir)
        //    return;

        if (navMeshAgent != null)
            navMeshAgent.isStopped = bStop;
    }

    public void SetWaitMode()
    {
        if (WaitMode == true)
            return;

        SetNavMeshStop(true);
        //navMeshAgent.isStopped = true;
        ChangeType(Type.Wait);
    }

    public void SetApproachMode()
    {
        if (ApproachMode == true)
            return;

        ChangeType(Type.Approach);

        SetNavMeshStop(false);
        //navMeshAgent.isStopped = false;
    }

    public void SetPatrolMode()
    {
        if (PatrolMode == true)
            return;

        ChangeType(Type.Patrol);

        SetNavMeshStop(false);
        //navMeshAgent.isStopped = false;
        //patrol.StartMove();
    }

    public void SetEquipMode()
    {
        if (enemy == null)
        {
            SetEquipMode(WeaponType.Unarmed);
            return;
        }

        SetEquipMode(enemy.weaponType);
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
        {
            transform.LookAt(player.transform, Vector3.up);
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, transform.eulerAngles.z);
        }

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
            Debug.Log("Unaremd로 가는 이유?");
            //animator.Play("Blend Tree", 0);

            if (animator.GetBool("IsAction") == true)
                weapon.End_DoAction();

            //if (coroutineEndDoAction != null)
            //{
            //    //Stop을 시킨다고 coroutine  객체가 null 되지않으므로 이 곳은 매번 온다
            //    StopCoroutine(coroutineEndDoAction);
            //    //Debug.Log($"Enemy_Good End? : {coroutineEndDoAction == null}");
            //}
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

    private void ChangeType(Type type)
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

    #region PositionAndMove

    protected BTNode.NodeState GetRandomPosition()
    {
        if (patrol == null)
            return BTNode.NodeState.Failure;

        if (patrol.GetPath() != null)
            return BTNode.NodeState.Success;

        patrol.SetPath();
        return BTNode.NodeState.Success;
    }


    protected BTNode.NodeState Action_MoveTo()
    {

        if (patrol == null)
            return BTNode.NodeState.Failure;
        patrol.MoveTo();
        if (patrol.Arrived)
            return NodeState.Success;

        return BTNode.NodeState.Running;
    }

    #endregion

    #region Wait

    private bool bInit = false;
    private float maxWaitTime;
    private void ResetTime(bool bRandom)
    {
        if (bInit)
            return;

        bInit = true;
        SetWaitMode();
        maxWaitTime += waitDelay;
        if (bRandom)
            maxWaitTime += UnityEngine.Random.Range(-waitDelayRandom, waitDelayRandom);
    }

    protected NodeState Acition_DoWait(bool bRandom = false, Action action = null)
    {
        ResetTime(bRandom);

        currentCoolTime += Time.deltaTime;
        if (currentCoolTime < waitDelay)
        {
            return NodeState.Running;
        }

        bInit = true;
        currentCoolTime = 0.0f;
        maxWaitTime = 0.0f;
        action?.Invoke();

        return NodeState.Success;
    }
    #endregion

    #region Unequip
    protected NodeState Action_DoUnequip()
    {
        if (weapon.IsEquippingMode() == false)
        {
            SetWaitMode();
            weapon.SetUnarmedMode();
            return NodeState.Success;
        }

        return NodeState.Running;
    }

    #endregion

    #region Equip

    protected NodeState Aciton_DoEquip()
    {
        if (weapon == null)
            return NodeState.Failure;

        if (weapon.IsEquippingMode())
        {
            SetWaitMode();
            return NodeState.Success;
        }
        else
        {
            SetEquipMode();
        }

        if (DamagedMode)
            weapon.End_Equip();


        return NodeState.Running;
    }

    #endregion

    #region Approach

    protected BTNode.NodeState Condition_Approach()
    {
        GameObject player = perception.GetPercievedPlayer();
        float temp = Vector3.Distance(transform.position, player.transform.position);
        if (temp < attackRange)
        {
            if (weapon.UnarmedMode == false)
            {
                return NodeState.Failure;
            }
        }


        SetApproachMode();

        return NodeState.Success;
    }

    protected NodeState Action_ChaseTo()
    {
        if (ApproachMode == false)
            return NodeState.Failure;

        GameObject player = perception.GetPercievedPlayer();

        if (player == null)
            return NodeState.Failure;

        navMeshAgent.SetDestination(player.transform.position);
        return NodeState.Running;
    }



    #endregion

    #region Attack 

    private bool bAttacking = false; 
    protected NodeState Condition_CheckAttack()
    {
      

        GameObject player = perception.GetPercievedPlayer();
        float temp = Vector3.Distance(transform.position, player.transform.position);
        if (temp < attackRange)
        {
            if (weapon.UnarmedMode == false)
            {
                return NodeState.Success;
            }
        }
        return NodeState.Failure;
    }

    protected NodeState Action_DoAttack()
    {
        if (weapon.UnarmedMode == false && bAttacking == false)
        {
            bAttacking = true; 
            SetActionMode();
            
        }

        return NodeState.Success;
    }
    private void OnEndDoAction()
    {
        SetCoolTime(attackDelay, attackDelayRandom);

        SetWaitMode();
        //bAttacking = false; 
        // 여긴 마무리만 처리해야하는 로직이므로 ㄷ별도의 코드 xx 
        // coroutineEndDoAction = StartCoroutine(Wait_EndDoAction_Random());
    }
    #endregion 
}
