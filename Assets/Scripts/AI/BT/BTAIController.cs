using AI.BT;
using AI.BT.CustomBTNodes;
using AI.BT.Nodes;
using AI.BT.TaskNodes;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static StateComponent;

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

    protected PerceptionComponent perception;

    Vector3 dest;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        blackboard = so_blackboard.Clone();

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

        if (attackRange <= distanceSquared)
        {
            // 공격
        }


        SetApproachMode();
    }

    private void CreateBlackboardKey()
    {
        blackboard.SetValue<AIStateType>("Wait", AIStateType.Wait);
        blackboard.SetValue<AIStateType>("Approach", AIStateType.Approach);
        blackboard.SetValue<AIStateType>("Patrol", AIStateType.Patrol);
    }

    private RootNode CreateBTTree()
    {
        SelectorNode selector = new SelectorNode();

        // 대기 
        WaitNode waitNode = new WaitNode();

        BlackboardConditionDecorator<AIStateType> waitDeco =
            new BlackboardConditionDecorator<AIStateType>("WaitDeco",waitNode, this.gameObject,
            blackboard, "Wait",
            CheckState);


        // 타겟으로 이동
        SequenceNode approachSequence = new SequenceNode();

        TaskNode_Speed approachSpeed = new TaskNode_Speed(this.gameObject, blackboard,
            SpeedType.Run);

        MoveToNode moveToNode = new MoveToNode(this.gameObject, blackboard);
        perception.OnValueChange += moveToNode.OnValueChange;

        approachSequence.AddChild(approachSpeed);
        approachSequence.AddChild(moveToNode);

        BlackboardConditionDecorator<AIStateType> moveDeco =
            new BlackboardConditionDecorator<AIStateType>("MoveDeco", approachSequence, this.gameObject,
            blackboard, "Approach",
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
        WaitNode patrolWait = new WaitNode(1.5f, 0.5f);
        
        patrolSubSequence.AddChild(patrolNode); 
        patrolSubSequence.AddChild(patrolWait);

        patrolSelector.AddChild(patrolSubSequence);
        patrolSelector.AddChild(patrolWait);

        patrolSequence.AddChild(patrolSpeed);
        patrolSequence.AddChild(patrolSelector);

        BlackboardConditionDecorator<AIStateType> patrolDeco =
         new BlackboardConditionDecorator<AIStateType>("PatrolDeco", patrolSequence, this.gameObject,
         blackboard, "Patrol",
         CheckState);


        selector.AddChild(waitDeco);
        selector.AddChild(patrolDeco);
        selector.AddChild(moveDeco);

        return new RootNode(this.gameObject, blackboard, selector);
    }

    private bool CheckState(AIStateType type)
    {
        if (this.type == type)
        {
            Debug.Log($"Current AI State : {type}");
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
        Debug.Log($"{this.gameObject.name} Target Loss!  - - 1");
        blackboard.SetValue<GameObject>("Target", null);
        //perception.OnValueChange?.Invoke();
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

        navMeshAgent.stoppingDistance = 2;
        ChangeType(AIStateType.Approach);
    }


    protected void ChangeType(AIStateType type)
    {
        AIStateType prevType = this.type;
        this.type = type;
        OnAIStateTypeChanged?.Invoke(prevType, type);
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
