using BT;
using BT.CustomBTNodes;
using BT.Nodes;
using BT.TaskNodes;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 제 자리에만 서서 공격만 하는 AI
/// </summary>
public class BTAIController_Tower : BTAIController
{
    [SerializeField] int maxWaitCondtionPattern = 0;
    HealthPointComponent health;

    protected override void Awake()
    {
        base.Awake();
        health = GetComponent<HealthPointComponent>();
        if (action != null)
            action.OnEndDoAction += OnEndDoAction;
    }

    protected override void Start()
    {
        base.Start();

        waitCondition = WaitCondition.None;
        blackboard.AddEnumComparisonStrategy<AIStateType>();

        if (btRunner)
        {
            btRunner.RunBehaviorTree(tickInterval);
        }

        navMeshAgent.avoidancePriority = Random.Range(1, 100);
    }

    protected override void Update()
    {
        base.Update();
        userInterface.text += "\n" + waitCondition.ToString();
        userInterface.text += "\n" + state.Type.ToString();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (CheckMode())
            return; 

        if (NoneCondition == false)
            return; 

        GameObject player = perception.GetPercievedPlayer();
        if (player == null)
        {
            SetWaitMode();
            //SetPatrolMode();

            return;
        }

        float distanceSquared = Vector3.Distance(player.transform.position, this.transform.position);
        distanceSquared = Mathf.Floor(distanceSquared * 10 ) / 10 ;
        if (distanceSquared <= attackRange)
        {
            // 배회 or 공격 
            //DeicidePattern();
            // 공격
            SetActionMode();

            return;
        }

        SetWaitMode();
        //SetApproachMode();
    }

    protected override void LateUpdate()
    {
        if (WaitMode && IdleCondition)
        {
            animator.SetFloat("SpeedY", 0.0f);
            return;
        }
        else
        {
            animator.SetFloat("SpeedX", navMeshAgent.velocity.z); 
            animator.SetFloat("SpeedY", navMeshAgent.velocity.magnitude);
        }
    }

    #region BehaviorTree 
    protected override void CreateBlackboardKey()
    {
        // 모든 데코레이터노드가 해당 키로 비교한다. 
        //blackboard.SetValue<AIStateType>("AIStateType", AIStateType.Wait);

        // 각각의 데코레이터노드에서 값을 비교하기 위한 값들
        //blackboard.SetValue<AIStateType>("Wait", AIStateType.Wait);
        //blackboard.SetValue<AIStateType>("Approach", AIStateType.Approach);
        //blackboard.SetValue<AIStateType>("Patrol", AIStateType.Patrol);
        //blackboard.SetValue<AIStateType>("Action", AIStateType.Action);
        //blackboard.SetValue<AIStateType>("Damaged", AIStateType.Damaged);
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
            Decorator_Blackboard<AIStateType>.ObserveAborts.Selft,
            "AIStateType", AIStateType.Damaged);

        DamagedSelector.AddChild(damagedDeco);

        // 서비스 
        SequenceNode sequence = new SequenceNode();

        // WaitMode
        SelectorNode waitSelector = new SelectorNode();
        {
            waitSelector.NodeName = "WaitSelector";

            // 대기 
            WaitNode waitNode = new WaitNode(waitDelay, waitDelayRandom);
            //Decorator_WaitCondition idleDeco = new Decorator_WaitCondition(waitNode, this.gameObject,
            //    WaitCondition.Idle);
            //idleDeco.NodeName = "Idle";

            waitSelector.AddChild(waitNode);
        }

        // 공격 
        SequenceNode attackSequence = new SequenceNode();
        attackSequence.NodeName = "Attack";

        TaskNode_Targeting targetNode = new TaskNode_Targeting(this.gameObject, blackboard);
        TaskNode_Equip equipNode = new TaskNode_Equip(this.gameObject, blackboard, enemy.weaponType);

        TaskNode_Action actionNode = new TaskNode_Action(this.gameObject, blackboard);

        WaitNode attackWaitNode = new WaitNode(attackDelay, attackDelayRandom);
        attackWaitNode.NodeName = "Attak_Wait";

        TaskNode_ActionEnd actionEnd = new TaskNode_ActionEnd(this.gameObject, blackboard);

        
        attackSequence.AddChild(targetNode);
        attackSequence.AddChild(equipNode);
        attackSequence.AddChild(actionNode);
        attackSequence.AddChild(attackWaitNode);
       // attackSequence.AddChild(actionEnd);

 
        sequence.AddChild(waitSelector);
        sequence.AddChild(attackSequence);

        DamagedSelector.AddChild(sequence);

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
        //bool bCheck = base.CheckMode();
        bool bCheck = false;
        bCheck |= DamagedMode;
        bCheck |= state.IdleMode == false;

        if (health != null)
            bCheck |= health.Dead;

        return bCheck;
    }

    public override void SetWaitMode(bool isDamaged = false)
    {
        base.SetWaitMode(isDamaged);

        //waitCondition = WaitCondition.Idle;
        if(isDamaged == false)
            DeicideWaitCondition();
    }

    public override void SetPatrolMode()
    {
        base.SetPatrolMode();

        NavMeshUpdateRotationSet();
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

            case WaitCondition.Backward:
            //SetWaitState_StrafeCondition();
            SetWaitState_BackwardCondition();
            break;

            case WaitCondition.Strafe:
            NavMeshUpdateRotationSet();
            SetWaitState_StrafeCondition();
            break;

            default:
                SetWaitState_IdleCondition();
            break;
        }

    }


    // 배회 경로 상에 무언가가 있는지 체크 
    private bool CheckSafetyProwl()
    {
        Vector3 position = transform.position;
        Vector3 behindPos = -transform.forward;
        Vector3 castStartPosition = position + behindPos * 0.5f; 
        
        RaycastHit[] hits = Physics.BoxCastAll(castStartPosition, transform.lossyScale, behindPos,
            transform.rotation, 1.0f);

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
            return false;

        return true; 
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
