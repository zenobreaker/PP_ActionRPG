using AI.BT;
using AI.BT.CustomBTNodes;
using AI.BT.Nodes;
using AI.BT.TaskNodes;
using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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


    private Animator animator; 
    private NavMeshAgent navMeshAgent;
    public NavMeshAgent NavMeshAgent { get { return navMeshAgent; } }

    [SerializeField] bool bBT_DebugMode = false;
    [SerializeField] private SO_Blackboard so_blackboard;
    private SO_Blackboard blackboard;

    private BehaviorTreeRunner btRunner;

    protected PerceptionComponent perception;

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
            SetWaitMode();

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
        MoveToNode moveToNode = new MoveToNode(this.gameObject, blackboard);
        perception.OnValueChange += moveToNode.OnValueChange;

        BlackboardConditionDecorator<AIStateType> moveDeco =
            new BlackboardConditionDecorator<AIStateType>("MoveDeco",moveToNode, this.gameObject,
            blackboard, "Approach",
            CheckState);


        selector.AddChild(waitDeco);
        selector.AddChild(moveDeco);

        return new RootNode(this.gameObject, blackboard, selector);
    }

    private bool CheckState(AIStateType type)
    {
        if (this.type == type)
            return true;
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
        Debug.Log("Target Loss!  - - 1");
        blackboard.SetValue<GameObject>("Target", null);
        perception.OnValueChange?.Invoke();
    }

    public virtual void SetWaitMode()
    {
        if (WaitMode == true)
            return;

        ChangeType(AIStateType.Wait);
    }

    public virtual void SetApproachMode()
    {
        if (ApproachMode == true)
            return;

        ChangeType(AIStateType.Approach);
    }


    protected void ChangeType(AIStateType type)
    {
        AIStateType prevType = this.type;
        this.type = type;
        OnAIStateTypeChanged?.Invoke(prevType, type);
    }


}
