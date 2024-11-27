using BT;
using BT.CustomBTNodes;
using BT.Nodes;
using BT.TaskNodes;
using UnityEditor;
using UnityEngine;

public class BTAIController_Melee : BTAIController
{
    /// <summary>
    /// 0 = action 1 = backstep 2 = straife
    /// </summary>
    [SerializeField] int maxWaitCondtionPattern = 0;

    HealthPointComponent health;

    protected override void Awake()
    {
        base.Awake();
        health = GetComponent<HealthPointComponent>();
        if (action != null)
        {
            action.OnBeginDoAction += OnBeginDoAction;
            action.OnEndDoAction += OnEndDoAction;
        }
    }

    protected override void Start()
    {
        base.Start();

        waitCondition = WaitCondition.None;
        blackboard.AddEnumComparisonStrategy<AIStateType>();
        blackboard.AddEnumComparisonStrategy<WaitCondition>();

        if (btRunner != null)
        {
            btRunner.RunBehaviorTree(0.01f);
        }
        navMeshAgent.avoidancePriority = Random.Range(1, 100);
    }

    protected override void Update()
    {
        base.Update();
        userInterface.text += "\n" + waitCondition.ToString();

        if (health.Dead)
        {
            btRunner.StopBehaviorTree();
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (CheckMode())
            return;

        if (NoneCondition == false)
        {
            return;
        }

        GameObject player = perception.GetPercievedPlayer();
        if (player == null)
        {
            //SetWaitMode();
            SetPatrolMode();

            return;
        }

        float distanceSquared = Vector3.Distance(player.transform.position, this.transform.position);
        distanceSquared = Mathf.Floor(distanceSquared * 10) / 10;
        if (distanceSquared <= attackRange)
        {
            // 배회 or 공격 
            //DeicidePattern();
            // 공격
            SetActionMode();

            return;
        }


        SetApproachMode();
    }

    protected override void LateUpdate()
    {
        if (WaitMode && IdleCondition)
        {
            animator.SetFloat("SpeedX", 0.0f);
            animator.SetFloat("SpeedY", 0.0f);
            return;
        }
        else
        {
            animator.SetFloat("SpeedX", navMeshAgent.velocity.z);
            float deltaSpeed = navMeshAgent.velocity.magnitude / navMeshAgent.speed * 2.0f;
            animator.SetFloat("SpeedY", deltaSpeed);
        }
    }

    #region BehaviorTree 
    protected override void CreateBlackboardKey()
    {
        // 모든 데코레이터노드가 해당 키로 비교한다. 
        blackboard.SetValue<AIStateType>("AIStateType", AIStateType.Wait);

        blackboard.SetValue<WaitCondition>("WaitCondition", WaitCondition.None);
    }

    private BTNode CreateWaitCondition()
    {
        SequenceNode sequenceNode = new SequenceNode();


        // WaitMode
        SelectorNode waitSelector = new SelectorNode();
        {
            waitSelector.NodeName = "WaitSelector";


            WaitNode waitNode1 = new WaitNode(waitDelay, waitDelayRandom);

            // 대기 
            WaitNode waitNode = new WaitNode(waitDelay, waitDelayRandom);
            Decorator_Composit<WaitCondition> idleDeco =
                new Decorator_Composit<WaitCondition>("Idle", waitNode, this.gameObject,
                blackboard, "WaitCondition", WaitCondition.Idle);

            Decorator_WaitCondition idleEnd = new Decorator_WaitCondition(idleDeco,
            this.gameObject, WaitCondition.Idle);

            // 뒷걸음 
            WaitNode bwWaitNode = new WaitNode(waitDelay, waitDelayRandom);
            TaskNode_Speed bwSpeed = new TaskNode_Speed(this.gameObject, blackboard, SpeedType.Walk);
            TaskNode_Backward backward = new TaskNode_Backward(gameObject, blackboard, 1.5f);
            bwWaitNode.NodeName = "Backward";
            SequenceNode backsequenceNode = new SequenceNode();
            backsequenceNode.AddChild(bwSpeed);
            backsequenceNode.AddChild(backward);

            Decorator_Composit<WaitCondition> backwardDeco =
                new Decorator_Composit<WaitCondition>("Backward",
                backsequenceNode, this.gameObject, blackboard,
                "WaitCondition", WaitCondition.Backward);


            // 옆 걸음

            WaitNode sfWaitNode = new WaitNode(waitDelay, waitDelayRandom);
            sfWaitNode.NodeName = "StrafeWait";
            TaskNode_Speed sfSpeed = new TaskNode_Speed(this.gameObject, blackboard, SpeedType.Walk);

            SelectorNode strafeSelector = new SelectorNode();

            TaskNode_Strafe strafeNode = new TaskNode_Strafe(gameObject, blackboard, 5.0f, waitDelay, waitDelayRandom);
            strafeNode.OnDestination += OnDestination;
            TaskNode_Backward sfbackward = new TaskNode_Backward(gameObject, blackboard, 3.5f);
            backward.OnDestination += OnDestination;
            WaitNode sfWaitNode2 = new WaitNode(waitDelay, waitDelayRandom);

            strafeSelector.AddChild(strafeNode);
            strafeSelector.AddChild(sfbackward);
            strafeSelector.AddChild(sfWaitNode2);

            SequenceNode sfsequenceNode = new SequenceNode();
            sfsequenceNode.AddChild(sfSpeed);
            sfsequenceNode.AddChild(strafeSelector);

            Decorator_Composit<WaitCondition> strafeDeco =
              new Decorator_Composit<WaitCondition>("Strafe",
              sfsequenceNode, this.gameObject, blackboard,
              "WaitCondition", WaitCondition.Strafe);


            waitSelector.AddChild(idleDeco);
            waitSelector.AddChild(backwardDeco);
            waitSelector.AddChild(strafeDeco);
        }

        TaskNode_WaitEnd waitEnd = new TaskNode_WaitEnd(this.gameObject, blackboard);

        sequenceNode.AddChild(waitSelector);
        sequenceNode.AddChild(waitEnd);

        return sequenceNode;
    }


    protected override RootNode CreateBTTree()
    {

        // hit
        SelectorNode DamagedSelector = new SelectorNode();
        DamagedSelector.NodeName = "DamagedSelector";

        SequenceNode DamagedSequence = new SequenceNode();
        DamagedSequence.NodeName = "DamagedSequence";

        TaskNode_Damaged damagedNode = new TaskNode_Damaged(this.gameObject, blackboard);
        WaitNode damagedWaitNode = new WaitNode(damageDelay, damageDelayRandom);
        damagedWaitNode.NodeName = "DamagedWait";

        DamagedSequence.AddChild(damagedNode);
        DamagedSequence.AddChild(damagedWaitNode);

        Decorator_Blackboard<AIStateType> damagedDeco =
            new Decorator_Blackboard<AIStateType>("DamagedDeco", DamagedSequence,
            this.gameObject, blackboard, Decorator_Blackboard<AIStateType>.NotifyObserver.OnResultChange,
            Decorator_Blackboard<AIStateType>.ObserveAborts.None,
            "AIStateType", AIStateType.Damaged);

        DamagedSelector.AddChild(damagedDeco);

        // 서비스 
        SelectorNode selector = new SelectorNode();

        // WaitMode
        Decorator_Blackboard<AIStateType> waitDeco =
            new Decorator_Blackboard<AIStateType>("WaitDeco", CreateWaitCondition(), this.gameObject,
            blackboard, Decorator_Blackboard<AIStateType>.NotifyObserver.OnResultChange,
            Decorator_Blackboard<AIStateType>.ObserveAborts.Selft,
            "AIStateType", AIStateType.Wait);



        // 타겟으로 이동
        SequenceNode approachSequence = new SequenceNode();

        TaskNode_Speed approachSpeed = new TaskNode_Speed(this.gameObject, blackboard,
            SpeedType.Run);

        MoveToNode moveToNode = new MoveToNode(this.gameObject, blackboard, true);

        approachSequence.AddChild(approachSpeed);
        approachSequence.AddChild(moveToNode);

        Decorator_Blackboard<AIStateType> moveDeco =
            new Decorator_Blackboard<AIStateType>("MoveDeco", approachSequence, this.gameObject,
            blackboard, Decorator_Blackboard<AIStateType>.NotifyObserver.OnResultChange,
            Decorator_Blackboard<AIStateType>.ObserveAborts.Selft,
            "AIStateType", AIStateType.Approach);

        // 순찰
        SequenceNode patrolSequence = new SequenceNode();
        TaskNode_Speed patrolSpeed = new TaskNode_Speed(this.gameObject, blackboard,
            SpeedType.Walk);


        SelectorNode patrolSelector = new SelectorNode();
        SequenceNode patrolSubSequence = new SequenceNode();

        TaskNode_Unequip unequipNode = new TaskNode_Unequip(this.gameObject, blackboard);

        TaskNode_Patrol patrolNode = new TaskNode_Patrol(this.gameObject, blackboard,
            radius);
        patrolNode.OnDestination += OnDestination;
        WaitNode patrolWait = new WaitNode(waitDelay, waitDelayRandom);

        patrolSubSequence.AddChild(unequipNode);
        patrolSubSequence.AddChild(patrolNode);
        patrolSubSequence.AddChild(patrolWait);

        patrolSelector.AddChild(patrolSubSequence);
        patrolSelector.AddChild(patrolWait);

        patrolSequence.AddChild(patrolSpeed);
        patrolSequence.AddChild(patrolSelector);

        Decorator_Blackboard<AIStateType> patrolDeco =
         new Decorator_Blackboard<AIStateType>("PatrolDeco", patrolSequence, this.gameObject,
         blackboard, Decorator_Blackboard<AIStateType>.NotifyObserver.OnResultChange,
            Decorator_Blackboard<AIStateType>.ObserveAborts.Selft,
            "AIStateType", AIStateType.Patrol);

        // 공격 
        SequenceNode attackSequence = new SequenceNode();
        attackSequence.NodeName = "Attack";

        TaskNode_Equip equipNode = new TaskNode_Equip(this.gameObject, blackboard, enemy.weaponType);

        TaskNode_Action actionNode = new TaskNode_Action(this.gameObject, blackboard);

        WaitNode attackWaitNode = new WaitNode(attackDelay, attackDelayRandom);
        attackWaitNode.NodeName = "Attak_Wait";

        TaskNode_ActionEnd actionEnd = new TaskNode_ActionEnd(this.gameObject, blackboard);

        attackSequence.AddChild(equipNode);
        attackSequence.AddChild(actionNode);
        attackSequence.AddChild(CreateWaitCondition());
        // attackSequence.AddChild(actionEnd);

        Decorator_Blackboard<AIStateType> attackDeco =
            new Decorator_Blackboard<AIStateType>("ActionDeco",
            attackSequence, this.gameObject, blackboard, 
            Decorator_Blackboard<AIStateType>.NotifyObserver.OnResultChange,
            Decorator_Blackboard<AIStateType>.ObserveAborts.Selft,
            "AIStateType",
            AIStateType.Action);

        selector.AddChild(waitDeco);
        selector.AddChild(patrolDeco);
        selector.AddChild(moveDeco);
        selector.AddChild(attackDeco);

        DamagedSelector.AddChild(selector);

        return new RootNode(this.gameObject, blackboard, DamagedSelector);
    }
    #endregion
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

    protected override bool CheckMode()
    {
        bool bCheck = base.CheckMode();

        if (health != null)
            bCheck |= health.Dead;

        return bCheck;
    }

    public override void SetWaitMode(bool isDamaged = false)
    {
        base.SetWaitMode(isDamaged);

        //waitCondition = WaitCondition.Idle;
        if (isDamaged == false)
            DeicideWaitCondition();
    }

    public override void SetPatrolMode()
    {
        base.SetPatrolMode();

        NavMeshUpdateRotationSet();
    }

    public override void SetApproachMode()
    {
        if(CheckTargetArround() == false)
        {
            SetWaitMode();

            return; 
        }

        base.SetApproachMode();
    }

    protected override void ChangeWaitCondition(WaitCondition condition)
    {
        base.ChangeWaitCondition(condition);

        blackboard.SetValue("WaitCondition", condition);
    }

    private void DeicideWaitCondition()
    {
        int maxConditionValue = maxWaitCondtionPattern;
        GameObject player = perception.GetPercievedPlayer();
        if (player == null)
            maxConditionValue = (int)WaitCondition.Idle;

        int num = Random.Range(1, maxConditionValue);

        WaitCondition codition = (WaitCondition)num;
        Debug.Log($"Decide Wait Condtion  {codition}");

        switch (codition)
        {
            case WaitCondition.Idle:
            SetWaitState_IdleCondition();
            break;

            case WaitCondition.Strafe:
            NavMeshUpdateRotationSet();
            SetWaitState_StrafeCondition();
            break;

            case WaitCondition.Backward:
            SetWaitState_BackwardCondition();
            break;


            default:
            SetWaitState_IdleCondition();
            break;
        }

    }


    // 대상의 일정 범위 내에 무언가가 있고 그 상태를 체크 후에 그에대 갈지 말지를 결정한다.
    private bool CheckTargetArround()
    {

        GameObject player = perception.GetPercievedPlayer();
        if (player == null)
            return false;

        Vector3 targetPosition = player.transform.position;

        // 1. 대상과 나 사이에 무언가가 있는지 검사
        Vector3 position = transform.position;
        Vector3 direction = targetPosition - position;
        direction.Normalize();
        direction.y += 1;
        // 해당 방향으로 레이 발사 
        RaycastHit[] candidates = Physics.RaycastAll(position, direction);
        foreach (RaycastHit c in candidates)
        {
            if (c.transform.gameObject == player)
                continue;
            if(c.transform.gameObject == this.gameObject)
                continue;
            if (c.transform.gameObject.layer == LayerMask.GetMask("Ground"))
                continue;

            // 충돌된 다른 무언가가 있다면 해당 경로로는 추격을 금지
            Debug.Log("대상으로 향하는 방향에 장애물 있음");
            return false; 
        }

        // 2. 대상 주변 검사 
        Collider[] colliders = Physics.OverlapSphere(targetPosition, attackRange);

        foreach (Collider collider in colliders)
        {
            // 대상 주변에 내 공격 범위 만큼 측정했을 때 State 가진 무언가가 존재한다면?
            if (collider.gameObject.TryGetComponent<Enemy>(out Enemy enemy))
            {
                // 그 적의 행위 계산 
                if (enemy.TryGetComponent<StateComponent>(out StateComponent state))
                {
                    // 공격하고 있다면 난 안할래 
                    if (state.ActionMode)
                    {
                        Debug.Log("대상으로 주변에 무언가가 이미 공격 중");
                        return false;
                    }
                }
            }
        }

        Debug.Log("대상으로 추격이 온전히 가능 ");
        return true;
    }


    protected override void OnBeginDoAction()
    {
        base.OnBeginDoAction();

        DeicideWaitCondition();
    }

    protected override void OnEndDoAction()
    {
        base.OnEndDoAction();

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
