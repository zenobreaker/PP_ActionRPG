using AI.BT;
using AI.BT.Nodes;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(MovementComponent))]
[RequireComponent(typeof(PerceptionComponent))]
public abstract class BTAIController : MonoBehaviour
{
    public enum AIStateType
    {
        Wait = 0, Patrol, Approach, Equip, Action, Damaged, Max,
    }

    protected AIStateType type;
    public event Action<AIStateType, AIStateType> OnAIStateTypeChanged;
    public bool WaitMode { get => type == AIStateType.Wait; }
    public bool PatrolMode { get => type == AIStateType.Patrol; }
    public bool ApproachMode { get => type == AIStateType.Approach; }
    public bool EquipMode { get => type == AIStateType.Equip; }
    public bool ActionMode { get => type == AIStateType.Action; }
    public bool DamagedMode { get => type == AIStateType.Damaged; }

    /// <summary>
    /// 공격 관련 
    /// </summary>
    [Header("About Attack")]
    [SerializeField] protected float attackRange = 1.5f;
    [SerializeField] protected float attackDelay = 2.0f;
    [SerializeField] protected float attackDelayRandom = 0.5f;

    /// <summary>
    /// 데미지 관련 
    /// </summary>
    [Header("About Damage")]
    [SerializeField] protected float damageDelay = 2.0f;
    [SerializeField] protected float damageDelayRandom = 1.0f;

    /// <summary>
    /// 대기 관련
    /// </summary>
    [Header("About Wait")]
    [SerializeField] protected float waitDelay = 1.0f;
    [SerializeField] protected float waitDelayRandom = 0.5f; // goalDelay +( - 랜덤 ~ +랜덤)


    /// <summary>
    ///  이동 관련
    /// </summary>
    [Header("About Move")]
    [SerializeField] protected float moveSpeed = 1.0f;
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }


    /// <summary>
    /// 순찰 관련
    /// </summary>
    [Header("About Patrol")]
    [SerializeField] protected float radius;
    [SerializeField] protected PatrolPoints patrolPoints;
    public PatrolPoints PatrolPoints { get => patrolPoints; }


    [Header("UI")]
    [SerializeField] protected string uiStateName = "EnemyAIState";


    protected Animator animator;
    protected NavMeshAgent navMeshAgent;
    private float navOriginSpeed;
    private float navOriginAngularSpeed;

    public NavMeshAgent NavMeshAgent { get { return navMeshAgent; } }

    [SerializeField] bool bBT_DebugMode = false;
    [SerializeField] protected SO_Blackboard so_blackboard;
    protected SO_Blackboard blackboard;

    protected BehaviorTreeRunner btRunner;


    protected Enemy enemy;
    protected PerceptionComponent perception;
    protected StateComponent state;
    protected WeaponComponent weapon;

    protected Vector3 dest;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        blackboard = so_blackboard.Clone();

        enemy = GetComponent<Enemy>();
        state = GetComponent<StateComponent>();
        weapon = GetComponent<WeaponComponent>();
        perception = GetComponent<PerceptionComponent>();
        Debug.Assert(perception != null);
    }

    protected virtual void Start()
    {
        perception.OnPerceptionUpdated += OnPerceptionUpdated;

        CreateBlackboardKey();

        btRunner = new BehaviorTreeRunner(CreateBTTree());
    }

    protected virtual void Update()
    {
        btRunner?.OperateNode(bBT_DebugMode);
    }

    protected abstract void LateUpdate();
    

    protected virtual void FixedUpdate()
    {
      
    }

    protected abstract void CreateBlackboardKey();


    protected abstract RootNode CreateBTTree();
   



    private void OnPerceptionUpdated(List<GameObject> gameObjects)
    {
        if (gameObjects.Count > 0)
        {
            blackboard.SetValue<GameObject>("Target", gameObjects[0]);

            return;
        }
        //Debug.Log($"{this.gameObject.name} Target Loss!  - - 1");
        blackboard.SetValue<GameObject>("Target", null);
    }

    public virtual void SetWaitMode()
    {
        if (WaitMode == true)
            return;

        ChangeType(AIStateType.Wait);
    }

    public virtual void SetPatrolMode()
    {
        if (PatrolMode == true)
            return;

        navMeshAgent.stoppingDistance = 0;
        ChangeType(AIStateType.Patrol);
    }

    public virtual void SetApproachMode()
    {
        if (ApproachMode == true)
            return;

        navMeshAgent.stoppingDistance = attackRange;
        ChangeType(AIStateType.Approach);
    }

    public virtual void SetActionMode()
    {
        if (ActionMode == true)
            return;

        ChangeType(AIStateType.Action);
    }

    public virtual void SetDamagedMode()
    {
        if (DamagedMode == true)
            return;

        ChangeType(AIStateType.Damaged);
    }

    protected void ChangeType(AIStateType type)
    {
        Debug.Log($"new type : {type}");
        AIStateType prevType = this.type;
        this.type = type;
        blackboard.SetValue("AIStateType", type);
        OnAIStateTypeChanged?.Invoke(prevType, type);
    }


    public void StopMovement()
    {
        navMeshAgent.isStopped = true;
    }
    public void StartMovement()
    {
        navMeshAgent.isStopped = false;
    }

    public void SetSpeed(float speed)
    {
        navMeshAgent.speed = speed;
    }

    public void Slow_NavMeshSpeed(float slowFactor)
    {
        navOriginSpeed = navMeshAgent.speed;
        navOriginAngularSpeed = navMeshAgent.angularSpeed;

        navMeshAgent.speed = navMeshAgent.speed * slowFactor;
        navMeshAgent.angularSpeed = navMeshAgent.angularSpeed * slowFactor;
    }

    public void Reset_NavMeshSpeed()
    {
        navMeshAgent.speed = navOriginSpeed;
        navMeshAgent.angularSpeed = navOriginAngularSpeed;
    }

    public void End_Damage()
    {
        // coroutineEndDamage = StartCoroutine(Wait_End_Damage());
        //SetCoolTime(damageDelay, damageDelayRandom);

        SetWaitMode();
    }
}
