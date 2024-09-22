using AI.BT.CustomBTNodes;
using AI.BT.Nodes;
using AI.BT.TaskNodes;
using UnityEditor;
using UnityEngine;

public class BTAIController_Range : BTAIController
{

    protected override void Start()
    {
        base.Start();
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        GameObject player = perception.GetPercievedPlayer();
        if (player == null)
        {
            //SetWaitMode();
            SetPatrolMode();

            return;
        }

        float distanceSquared = Vector3.Distance(player.transform.position , this.transform.position);

        if (distanceSquared <= attackRange)
        {
            // 공격
            SetActionMode();

            return;
        }


        // 도망 or 대기 
        SetWaitMode();
        //SetApproachMode();
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


    protected override void CreateBlackboardKey()
    {
        // 모든 데코레이터노드가 해당 키로 비교한다. 
        blackboard.SetValue<AIStateType>("AIStateType", AIStateType.Wait);

        // 각각의 데코레이터노드에서 값을 비교하기 위한 값들
        blackboard.SetValue<AIStateType>("Wait", AIStateType.Wait);
        blackboard.SetValue<AIStateType>("Patrol", AIStateType.Patrol);
        blackboard.SetValue<AIStateType>("Action", AIStateType.Action);
        blackboard.SetValue<AIStateType>("Damaged", AIStateType.Damaged);
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
