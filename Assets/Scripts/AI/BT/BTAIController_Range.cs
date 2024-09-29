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
        //blackboard.SetValue<AIStateType>("Wait", AIStateType.Wait);
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
        SelectorNode selector = new SelectorNode();

        // 대기 
        WaitNode waitNode = new WaitNode(waitDelay, waitDelayRandom);

        Decorator_Blackboard<AIStateType> waitDeco =
            new Decorator_Blackboard<AIStateType>("WaitDeco", waitNode, this.gameObject,
            blackboard, Decorator_Blackboard<AIStateType>.NotifyObserver.OnResultChange,
            Decorator_Blackboard<AIStateType>.ObserveAborts.Selft,
            "AIStateType", AIStateType.Wait);


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

        Decorator_Blackboard<AIStateType> patrolDeco =
         new Decorator_Blackboard<AIStateType>("PatrolDeco", patrolSequence, this.gameObject,
         blackboard, Decorator_Blackboard<AIStateType>.NotifyObserver.OnResultChange,
            Decorator_Blackboard<AIStateType>.ObserveAborts.Selft,
         "AIStateType", AIStateType.Patrol);

        // 공격 
        SequenceNode attackSequence = new SequenceNode();
        attackSequence.NodeName = "Attack";

        TaskNode_Equip equipNode = new TaskNode_Equip(this.gameObject, blackboard, enemy.weaponType);

        TaskNode_Targeting targetingNode = new TaskNode_Targeting(this.gameObject, blackboard);

        TaskNode_Action actionNode = new TaskNode_Action(this.gameObject, blackboard);

        WaitNode attackWaitNode = new WaitNode(attackDelay, attackDelayRandom);
        attackWaitNode.NodeName = "Attak_Wait";

        attackSequence.AddChild(equipNode);
        attackSequence.AddChild(targetingNode);
        attackSequence.AddChild(actionNode);
        attackSequence.AddChild(attackWaitNode);

        Decorator_Blackboard<AIStateType> attackDeco =
            new Decorator_Blackboard<AIStateType>("ActionDeco",
            attackSequence, this.gameObject, blackboard,
            Decorator_Blackboard<AIStateType>.NotifyObserver.OnResultChange,
            Decorator_Blackboard<AIStateType>.ObserveAborts.Selft,
            "AIStateType", AIStateType.Action);

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
