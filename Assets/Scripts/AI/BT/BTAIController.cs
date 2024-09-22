using AI.BT;
using AI.BT.CustomBTNodes;
using AI.BT.Nodes;
using AI.BT.TaskNodes;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using static StateComponent;
using static UnityEngine.EventSystems.EventTrigger;

public class BTAIController : MonoBehaviour
{
    public enum AIStateType
    {
        Wait = 0, Patrol, Approach, Equip, Action, Damaged, Max,
    }

    private AIStateType type;
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
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackDelay = 2.0f;
    [SerializeField] private float attackDelayRandom = 0.5f;

    /// <summary>
    /// 데미지 관련 
    /// </summary>
    [SerializeField] private float damageDelay = 2.0f;
    [SerializeField] private float damageDelayRandom = 1.0f;

    /// <summary>
    /// 대기 관련
    /// </summary>
    [SerializeField] private float waitDelay = 1.0f;
    [SerializeField] private float waitDelayRandom = 0.5f; // goalDelay +( - 랜덤 ~ +랜덤)


    /// <summary>
    ///  이동 관련
    /// </summary>
    [SerializeField] private float moveSpeed = 1.0f;
    public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }

    [SerializeField] private string uiStateName = "EnemyAIState";


    /// <summary>
    /// 순찰 관련
    /// </summary>
    [SerializeField] private float radius;
    [SerializeField] private PatrolPoints patrolPoints;
    public PatrolPoints PatrolPoints { get => patrolPoints; }
    

    private Animator animator; 
    private NavMeshAgent navMeshAgent;
    public NavMeshAgent NavMeshAgent { get { return navMeshAgent; } }

    [SerializeField] bool bBT_DebugMode = false;
    [SerializeField] private SO_Blackboard so_blackboard;
    private SO_Blackboard blackboard;

    private BehaviorTreeRunner btRunner;


    private Enemy enemy; 
    protected PerceptionComponent perception;
    protected StateComponent state;
    protected WeaponComponent weapon;   

    Vector3 dest;

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

    private void Start()
    {
        perception.OnPerceptionUpdated += OnPerceptionUpdated;

        CreateBlackboardKey();

        btRunner = new BehaviorTreeRunner(CreateBTTree());
    }

    private void Update()
    {
        btRunner?.OperateNode(bBT_DebugMode);
    }

    private void LateUpdate()
    {
        if (WaitMode)
        {
            animator.SetFloat("SpeedY", 0.0f);
            return;
        }
        else
        {
            animator.SetFloat("SpeedY", navMeshAgent.velocity.magnitude);
        }
        
    }

    private void FixedUpdate()
    {
        GameObject player = perception.GetPercievedPlayer();
        if (player == null)
        {
            //SetWaitMode();
            SetPatrolMode();

            return;
        }

        float distanceSquared = (player.transform.position - this.transform.position).sqrMagnitude;

        if ( distanceSquared <= attackRange)
        {
            // 공격
            SetActionMode();

            return; 
        }


        SetApproachMode();
    }

    private void CreateBlackboardKey()
    {
        // 모든 데코레이터노드가 해당 키로 비교한다. 
        blackboard.SetValue<AIStateType>("AIStateType", AIStateType.Wait);

        // 각각의 데코레이터노드에서 값을 비교하기 위한 값들
        blackboard.SetValue<AIStateType>("Wait", AIStateType.Wait);
        blackboard.SetValue<AIStateType>("Approach", AIStateType.Approach);
        blackboard.SetValue<AIStateType>("Patrol", AIStateType.Patrol);
        blackboard.SetValue<AIStateType>("Action", AIStateType.Action);
        blackboard.SetValue<AIStateType>("Damaged", AIStateType.Damaged);
    }

    private RootNode CreateBTTree()
    {

        // hit
        SelectorNode DamagedSelector = new SelectorNode();
        DamagedSelector.NodeName = "DamagedSelector";

        SequenceNode DamagedSequence = new SequenceNode();
        DamagedSequence.NodeName = "DamagedSequence";

        TaskNode_Damaged damagedNode = new TaskNode_Damaged(this.gameObject, blackboard);
        WaitNode  damagedWaitNode = new WaitNode(damageDelay, damageDelayRandom);
        damagedWaitNode.NodeName = "DamagedWait";

        DamagedSequence.AddChild(damagedNode);
        DamagedSequence.AddChild(damagedWaitNode);

        BlackboardConditionDecorator<AIStateType> damagedDeco =
            new BlackboardConditionDecorator<AIStateType>("DamagedDeco", DamagedSequence,
            this.gameObject, blackboard, "AIStateType", "Damaged", CheckState);

        DamagedSelector.AddChild(damagedDeco);

        // 서비스 
        SelectorNode selector = new SelectorNode();

        // 대기 
        WaitNode waitNode = new WaitNode(waitDelay, waitDelayRandom);

        BlackboardConditionDecorator<AIStateType> waitDeco =
            new BlackboardConditionDecorator<AIStateType>("WaitDeco",waitNode, this.gameObject,
            blackboard, "AIStateType","Wait",
            CheckState);


        // 타겟으로 이동
        SequenceNode approachSequence = new SequenceNode();

        TaskNode_Speed approachSpeed = new TaskNode_Speed(this.gameObject, blackboard,
            SpeedType.Run);

        MoveToNode moveToNode = new MoveToNode(this.gameObject, blackboard);

        approachSequence.AddChild(approachSpeed);
        approachSequence.AddChild(moveToNode);

        BlackboardConditionDecorator<AIStateType> moveDeco =
            new BlackboardConditionDecorator<AIStateType>("MoveDeco", approachSequence, this.gameObject,
            blackboard, "AIStateType", "Approach",
            CheckState);

        // 순찰
        SequenceNode patrolSequence = new SequenceNode();
        TaskNode_Speed patrolSpeed = new TaskNode_Speed(this.gameObject, blackboard,
            SpeedType.Walk);


        SelectorNode patrolSelector = new SelectorNode();
        SequenceNode patrolSubSequence = new SequenceNode();

        TaskNode_Patrol patrolNode = new TaskNode_Patrol(this.gameObject, blackboard,
            radius);
        patrolNode.OnDestination += OnDestination;
        WaitNode patrolWait = new WaitNode(waitDelay, waitDelayRandom);
        
        patrolSubSequence.AddChild(patrolNode); 
        patrolSubSequence.AddChild(patrolWait);

        patrolSelector.AddChild(patrolSubSequence);
        patrolSelector.AddChild(patrolWait);

        patrolSequence.AddChild(patrolSpeed);
        patrolSequence.AddChild(patrolSelector);

        BlackboardConditionDecorator<AIStateType> patrolDeco =
         new BlackboardConditionDecorator<AIStateType>("PatrolDeco", patrolSequence, this.gameObject,
         blackboard, "AIStateType", "Patrol",
         CheckState);

        // 공격 
        SequenceNode attackSequence = new SequenceNode();
        attackSequence.NodeName = "Attack";

        TaskNode_Equip equipNode = new TaskNode_Equip(this.gameObject, blackboard, enemy.weaponType);

        TaskNode_Action actionNode = new TaskNode_Action(this.gameObject, blackboard);

        WaitNode attackWaitNode = new WaitNode(attackDelay, attackDelayRandom);
        attackWaitNode.NodeName = "Attak_Wait";

        attackSequence.AddChild(equipNode);
        attackSequence.AddChild(actionNode);
        attackSequence.AddChild(attackWaitNode);

        BlackboardConditionDecorator<AIStateType> attackDeco =
            new BlackboardConditionDecorator<AIStateType>("ActionDeco",
            attackSequence, this.gameObject, blackboard, "AIStateType", "Action",
            CheckState);

        selector.AddChild(waitDeco);
        selector.AddChild(patrolDeco);
        selector.AddChild(moveDeco);
        selector.AddChild(attackDeco);

        DamagedSelector.AddChild(selector);

        return new RootNode(this.gameObject, blackboard, DamagedSelector);
    }

    private bool CheckState(AIStateType type)
    {
        if (this.type == type)
        {
            //Debug.Log($"Current AI State : {type}");
            return true;
        }
        else
            return false; 
    }

    private void OnPerceptionUpdated(List<GameObject> gameObjects)
    {
        if(gameObjects.Count > 0)
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


    private void OnDestination(Vector3 destination)
    {
        dest = destination;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying == false)
            return;

        if (Selection.activeGameObject != gameObject)
            return;

        Vector3 form = transform.position + new Vector3(0, 0.1f, 0);
        Vector3 to = dest + new Vector3(0, 0.1f, 0);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(form, to);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(dest, 0.5f);


        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(form, 0.25f);

       
    }
#endif
}
