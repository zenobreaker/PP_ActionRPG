using AI.BT.CustomBTNodes;
using AI.BT.Nodes;
using AI.BT.TaskNodes;
using UnityEditor;
using UnityEngine;

public class BTAIController_Melee : BTAIController
{
    /// <summary>
    /// 0 = action 1 = backstep 2 = straife
    /// </summary>
    [SerializeField] int maxDecidePattern = 0;

    protected override void Start()
    {
        base.Start();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if(ProwlMode)
        {
            // 해당 모드 중에 이동 방향에 다른 무언가가 있는지 검사하기 
            bool bResult = CheckSafetyProwl();
            if(bResult)
            {
                return; 
            }
            else
            {
                // 한프레임 뒤에 
                SetWaitMode();
                
                return;
            }
        }

        GameObject player = perception.GetPercievedPlayer();
        if (player == null && ProwlMode == false)
        {
            //SetWaitMode();
            SetPatrolMode();

            return;
        }

        float distanceSquared = Vector3.Distance(player.transform.position, this.transform.position);

        if (distanceSquared <= attackRange)
        {
            // 배회 or 공격 
            DeicidePattern();
            // 공격
            //SetActionMode();

            return;
        }


        SetApproachMode();
    }

    protected override void LateUpdate()
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

    #region BehaviorTree 
    protected override void CreateBlackboardKey()
    {
        // 모든 데코레이터노드가 해당 키로 비교한다. 
        blackboard.SetValue<AIStateType>("AIStateType", AIStateType.Wait);

        // 각각의 데코레이터노드에서 값을 비교하기 위한 값들
        blackboard.SetValue<AIStateType>("Wait", AIStateType.Wait);
        blackboard.SetValue<AIStateType>("Approach", AIStateType.Approach);
        blackboard.SetValue<AIStateType>("Patrol", AIStateType.Patrol);
        blackboard.SetValue<AIStateType>("Action", AIStateType.Action);
        blackboard.SetValue<AIStateType>("Damaged", AIStateType.Damaged);
        blackboard.SetValue<AIStateType>("Prowl", AIStateType.Prowl);
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

        BlackboardConditionDecorator<AIStateType> damagedDeco =
            new BlackboardConditionDecorator<AIStateType>("DamagedDeco", DamagedSequence,
            this.gameObject, blackboard, "AIStateType", "Damaged", CheckState);

        DamagedSelector.AddChild(damagedDeco);

        // 서비스 
        SelectorNode selector = new SelectorNode();

        // 대기 
        WaitNode waitNode = new WaitNode(waitDelay, waitDelayRandom);

        BlackboardConditionDecorator<AIStateType> waitDeco =
            new BlackboardConditionDecorator<AIStateType>("WaitDeco", waitNode, this.gameObject,
            blackboard, "AIStateType", "Wait",
            CheckState);

        // 배회 
        SequenceNode bwSequenceNode = new SequenceNode();
        TaskNode_Backward backward = new TaskNode_Backward(gameObject, blackboard, 5.0f);
        WaitNode bwWaitNode = new WaitNode(2.0f, 0.5f);
        bwWaitNode.NodeName = "Prowl";

        bwSequenceNode.AddChild(backward);
        bwSequenceNode.AddChild(bwWaitNode);

        BlackboardConditionDecorator<AIStateType> prowlDeco =
            new BlackboardConditionDecorator<AIStateType>("BackwardDeco", bwSequenceNode,
            this.gameObject, blackboard, "AIStateType", "Prowl", CheckState);
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
        selector.AddChild(prowlDeco);
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


    private void DeicidePattern()
    {
        int num = Random.Range(0, maxDecidePattern);


        Debug.Log($"Decide Pattern {num}");

        if (num == 0)
            SetActionMode();
        else if (num == 1)
            SetProwlMode();
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
