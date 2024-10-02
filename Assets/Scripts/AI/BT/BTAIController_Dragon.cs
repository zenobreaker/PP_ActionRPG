using AI.BT;
using AI.BT.CustomBTNodes;
using AI.BT.Nodes;
using AI.BT.TaskNodes;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class BTAIController_Dragon : BTAIController
{
    private enum DragonState
    {
        Idle, Move, Fall = 3, Fly = 6, Dead,
    }

    public class DragonPattern
    {
        int patternID;
        float maxCoolDown;
        public float coolDown;

        public bool Usable { get => coolDown <= 0.0f; }

        public DragonPattern(int patternID, float maxCoolDown)
        {
            this.patternID = patternID;
            this.maxCoolDown = maxCoolDown;
            coolDown = -maxCoolDown;
        }

        public void SetCoolDown()
        {
            coolDown = maxCoolDown;
        }
    }


    [SerializeField] int maxWaitCondtionPattern = 0;
    //[SerializeField] int maxAttackPattern = 5;
    private Dictionary<int, DragonPattern> dragonPatternTable = new Dictionary<int, DragonPattern>();
    private int currentAttackPattern = 0;
    public int GetCurrPattern { get => currentAttackPattern; }

    private HealthPointComponent health;
    private DragonState cur_DragonState;

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

        Start_SetDragonPattern();

        //navMeshAgent.updateRotation = false;

        cur_DragonState = DragonState.Idle;
        waitCondition = WaitCondition.None;
        if (blackboard == null)
            return;
        
        blackboard.AddEnumComparisonStrategy<AIStateType>();
    }

    private void Start_SetDragonPattern()
    {
        dragonPatternTable.Add(1, new DragonPattern(1, 3));
        dragonPatternTable.Add(2, new DragonPattern(2, 4));
        dragonPatternTable.Add(3, new DragonPattern(3, 10));
        dragonPatternTable.Add(4, new DragonPattern(4, 20));
        dragonPatternTable.Add(5, new DragonPattern(5, 30));
    }

    protected void OnEnable()
    {
        cur_DragonState = DragonState.Idle;

        
        //btRunner.RunBehaviorTree(tickInterval);
        //StartCoroutine(CoolDownCoroutine());

        if (bCanMove)
            OnEnableAI();
    }

    protected void OnDisable()
    {
        btRunner.StopBehaviorTree();

        StopAllCoroutines();
    }

    protected override void Update()
    {
        if (bCanMove == false)
            return; 
        base.Update();
        userInterface.text += "\n" + waitCondition.ToString();
        userInterface.text += "\n" + state.Type.ToString();
        userInterface.text += "\n" + "Decide Pattern => "+  currentAttackPattern; 
        userInterface.text += "\n" + dragonPatternTable[1].coolDown.ToString("F1");
        userInterface.text += "\n" + dragonPatternTable[2].coolDown.ToString("F1");
        userInterface.text += "\n" + dragonPatternTable[3].coolDown.ToString("F1");
        userInterface.text += "\n" + dragonPatternTable[4].coolDown.ToString("F1");
        userInterface.text += "\n" + dragonPatternTable[5].coolDown.ToString("F1");
        userInterface.text += "\n" + "\n" + "\n";

        if (health.Dead)
        {
            btRunner.StopBehaviorTree();
        }

        // 결정된 패턴이 없으면 패턴을 구한다.
        if(currentAttackPattern == 0)
        {
            DeceideActionPattern();

            ChangeAttackRange();
        }
    }

    protected override void FixedUpdate()
    {
        if (bCanMove == false)
            return;

        base.FixedUpdate();

        if (CheckMode())
            return;

        if (NoneCondition == false)
            return;

        GameObject player = perception.GetPercievedPlayer();
        // 결정된 패턴이 없으면 결정될 때까지 Wait
        if (player == null || currentAttackPattern == 0)
        {
            SetWaitMode();
            //SetPatrolMode();

            return;
        }

        float distanceSquared = Vector3.Distance(player.transform.position, this.transform.position);
        distanceSquared = Mathf.Floor(distanceSquared * 10) / 10;
        if (distanceSquared <= attackRange && currentAttackPattern != 0)
        {
            // 공격
            SetActionMode();

            return;
        }

        SetApproachMode();
    }

    protected override void LateUpdate()
    {
        if (bCanMove == false)
            return;

        LateUpdate_DeathMotion();

        if (health.Dead)
            return;

        if (WaitMode && IdleCondition)
        {
            animator.SetFloat("Vertical", 0.0f);
            return;
        }
        else
        {
            LateUpdate_AnimalMove();
        }
    }

    public override void OnEnableAI()
    {
        base.OnEnableAI();


        btRunner = new BehaviorTreeRunner(this.gameObject, blackboard, CreateBTTree());
        btRunner.RunBehaviorTree(tickInterval);
        navMeshAgent.enabled = true; 

        StartCoroutine(CoolDownCoroutine());
    }

    private void LateUpdate_AnimalMove()
    {
        // 자신의 전방 벡터에서 도착 지점과의 외적
        Vector3 foward = this.gameObject.transform.forward;
        Vector3 cross = Vector3.Cross(foward, dest);
        // 이 값을 y값으로 추출하면 두 좌표의 평행간의 거리 
        float distance = Vector3.Dot(cross, Vector3.up);

        // 이 거리 값이 0보다 작으면 왼쪽 
        // 이후 소수점이 어느 정도 이하면 그냥 0처리 
        //distance = Mathf.Floor(distance * 10) / 10;

        //animator.SetFloat("Horizontal", distance);
        animator.SetInteger("State", 1);
        float deltaSpeed = navMeshAgent.velocity.magnitude / navMeshAgent.speed;
        animator.SetFloat("Vertical", deltaSpeed);
    }


    private void LateUpdate_DeathMotion()
    {
        if (health.Dead == false)
            return;

        if (cur_DragonState == DragonState.Fly)
            animator.SetInteger("LastState", 3);

        animator.SetInteger("State", 7);
    }


    #region BehaviorTree 
    protected override void CreateBlackboardKey()
    {
        // 모든 데코레이터노드가 해당 키로 비교한다. 
        blackboard.SetValue<AIStateType>("AIStateType", AIStateType.Wait);

        // 각각의 데코레이터노드에서 값을 비교하기 위한 값들
        blackboard.SetValue<int>("DragonPattern", 0);
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
            this.gameObject, blackboard,
             Decorator_Blackboard<AIStateType>.NotifyObserver.OnResultChange,
            Decorator_Blackboard<AIStateType>.ObserveAborts.Selft,
            "AIStateType", AIStateType.Damaged);

        DamagedSelector.AddChild(damagedDeco);

        // 서비스 
        SelectorNode selector = new SelectorNode();

        // WaitMode
        SelectorNode waitSelector = new SelectorNode();
        {
            waitSelector.NodeName = "WaitSelector";

            // 대기 
            WaitNode waitNode = new WaitNode(waitDelay, waitDelayRandom);
            Decorator_WaitCondition idleDeco = new Decorator_WaitCondition(waitNode, this.gameObject,
                WaitCondition.Idle);
            idleDeco.NodeName = "Idle";


            // 옆 걸음
            WaitNode sfWaitNode = new WaitNode(waitDelay, waitDelayRandom);
            sfWaitNode.NodeName = "StrafeWait";
            TaskNode_Speed sfSpeed = new TaskNode_Speed(this.gameObject, blackboard, SpeedType.Walk);
            SelectorNode strafeSelector = new SelectorNode();

            TaskNode_Strafe strafeNode = new TaskNode_Strafe(gameObject, blackboard, 10.0f);
            strafeNode.OnDestination += OnDestination;
            TaskNode_Backward backward = new TaskNode_Backward(gameObject, blackboard, 3.5f);
            backward.OnDestination += OnDestination;

            strafeSelector.AddChild(strafeNode);
            strafeSelector.AddChild(backward);

            SequenceNode strafeSequence = new SequenceNode();
            strafeSequence.AddChild(sfSpeed);
            strafeSequence.AddChild(strafeSelector);


            //ParallelNode strafeParallel = new ParallelNode(ParallelNode.FinishCondition.All);
            //{
            //    strafeParallel.NodeName = "StarfeParallel";

            //    strafeParallel.AddChild(sfWaitNode);
            //    strafeParallel.AddChild(sequenceNode);
            //}
            Decorator_WaitCondition strafeDeco = new Decorator_WaitCondition(
                strafeSequence, this.gameObject, WaitCondition.Strafe);
            strafeDeco.NodeName = "Strafe";


            waitSelector.AddChild(idleDeco);
            waitSelector.AddChild(strafeDeco);
        }

        Decorator_Blackboard<AIStateType> waitDeco =
            new Decorator_Blackboard<AIStateType>("WaitDeco", waitSelector, this.gameObject,
            blackboard, Decorator_Blackboard<AIStateType>.NotifyObserver.OnResultChange,
            Decorator_Blackboard<AIStateType>.ObserveAborts.Selft, 
            "AIStateType", AIStateType.Wait);


        // 타겟으로 이동
        SequenceNode approachSequence = new SequenceNode();

        TaskNode_Speed approachSpeed = new TaskNode_Speed(this.gameObject, blackboard,
            SpeedType.Run);

        MoveToNode moveToNode = new MoveToNode(this.gameObject, blackboard, true);
        moveToNode.OnDestination += OnDestination;

        approachSequence.AddChild(approachSpeed);
        approachSequence.AddChild(moveToNode);

        Decorator_Blackboard<AIStateType> moveDeco =
            new Decorator_Blackboard<AIStateType>("MoveDeco", approachSequence, this.gameObject,
            blackboard, Decorator_Blackboard<AIStateType>.NotifyObserver.OnResultChange,
            Decorator_Blackboard<AIStateType>.ObserveAborts.Selft, 
            "AIStateType", AIStateType.Approach);


        // 공격 
        Decorator_Blackboard<AIStateType> attackDeco =
            new Decorator_Blackboard<AIStateType>("ActionDeco",
            CreatePatternActionBTTree(), this.gameObject, blackboard,
            Decorator_Blackboard<AIStateType>.NotifyObserver.OnResultChange,
            Decorator_Blackboard<AIStateType>.ObserveAborts.Selft,
            "AIStateType",
            AIStateType.Action);

        selector.AddChild(waitDeco);
        selector.AddChild(moveDeco);
        selector.AddChild(attackDeco);

        DamagedSelector.AddChild(selector);

        return new RootNode(this.gameObject, blackboard, DamagedSelector);
    }

    protected BTNode CreatePatternActionBTTree()
    {

        SelectorNode patternSelector = new SelectorNode();
        patternSelector.NodeName = "PatternSelect";

        // 물기 
        SequenceNode biteSequence = new SequenceNode();
        {
            biteSequence.NodeName = "biteSequence";
            TaskNode_SetupPattern bitePattern = new TaskNode_SetupPattern(this.gameObject, 1);
            //Decorator_CoolDown biteCoolDown = new Decorator_CoolDown(bitePattern, this.gameObject, 4.0f);
           // biteCoolDown.NodeName = "Bite";
            TaskNode_Action biteAction = new TaskNode_Action(this.gameObject, blackboard);
            biteAction.NodeName = "Bite";
            WaitNode bitWait = new WaitNode(attackDelay, attackDelayRandom);

            biteSequence.AddChild(bitePattern);
            biteSequence.AddChild(biteAction);
            biteSequence.AddChild(bitWait);

        }
        Decorator_Composit<int> biteDeco =
            new Decorator_Composit<int>("BiteNode", biteSequence, this.gameObject,
            blackboard,"DragonPattern", 1);


        // 날개 치기
        SequenceNode wingSequence = new SequenceNode();
        {
            wingSequence.NodeName = "WingSequence";
            TaskNode_SetupPattern wingPattern = new TaskNode_SetupPattern(this.gameObject, 2);
            //Decorator_CoolDown wingCoolDown = new Decorator_CoolDown(wingPattern, this.gameObject, 5.0f);
            //wingCoolDown.NodeName = "Wing";
            TaskNode_Action wingAction = new TaskNode_Action(this.gameObject, blackboard);
            wingAction.NodeName = "Wing";
            WaitNode wingWait = new WaitNode(attackDelay, attackDelayRandom);

            wingSequence.AddChild(wingPattern);
            wingSequence.AddChild(wingAction);
            wingSequence.AddChild(wingWait);
        }
        Decorator_Composit<int> wingDeco =
            new Decorator_Composit<int>("WingNode", wingSequence, this.gameObject,
            blackboard, "DragonPattern", 2);


        // 화염탄 발사
        SequenceNode fireballSequence = new SequenceNode();
        {

            TaskNode_SetupPattern fireballPattern = new TaskNode_SetupPattern(this.gameObject, 3);
            //Decorator_CoolDown fireballCoolDown = new Decorator_CoolDown(fireballPattern, this.gameObject, 6.5f);
            //fireballCoolDown.NodeName = "Fireball";

            TaskNode_Targeting target = new TaskNode_Targeting(this.gameObject, blackboard);
            target.NodeName = "Fireball_Target";
            TaskNode_Action fireballAction = new TaskNode_Action(this.gameObject, blackboard);
            fireballAction.NodeName = "Fireball";
            WaitNode fireballWait = new WaitNode(attackDelay, attackDelayRandom);


            fireballSequence.AddChild(fireballPattern);
            fireballSequence.AddChild(target);
            fireballSequence.AddChild(fireballAction);
            fireballSequence.AddChild(fireballWait);
        }
        Decorator_Composit<int> fireballDeco =
            new Decorator_Composit<int>("fireballNode", fireballSequence, this.gameObject,
            blackboard, "DragonPattern", 3);




        // 화염 방사
        SequenceNode firebreathSequence = new SequenceNode();
        {

            TaskNode_SetupPattern firebreathPattern = new TaskNode_SetupPattern(this.gameObject, 4);
            //Decorator_CoolDown firebreathCoolDown = new Decorator_CoolDown(firebreathPattern, this.gameObject, 6.5f);
            //firebreathCoolDown.NodeName = "Firebreath";

            TaskNode_Targeting target = new TaskNode_Targeting(this.gameObject, blackboard);
            target.NodeName = "Firebreath_Target";
            TaskNode_Action firebreathAction = new TaskNode_Action(this.gameObject, blackboard);
            firebreathAction.NodeName = "Firebreath";
            WaitNode firebreathWait = new WaitNode(attackDelay, attackDelayRandom);


            firebreathSequence.AddChild(firebreathPattern);
            firebreathSequence.AddChild(target);
            firebreathSequence.AddChild(firebreathAction);
            firebreathSequence.AddChild(firebreathWait);
        }
        Decorator_Composit<int> firebreathDeco =
            new Decorator_Composit<int>("firebreathNode", firebreathSequence, this.gameObject,
            blackboard, "DragonPattern", 4);



        // 날아오르고 화염 공격
        SequenceNode flySequence = new SequenceNode();
        {

            TaskNode_SetupPattern flyPattern = new TaskNode_SetupPattern(this.gameObject, 5);
            //Decorator_CoolDown flyCoolDown = new Decorator_CoolDown(flyPattern, this.gameObject, 10.0f);
           // flyCoolDown.NodeName = "Fly";

            TaskNode_Action flyAction = new TaskNode_Action(this.gameObject, blackboard);
            flyAction.NodeName = "Fly";
            WaitNode flyWait = new WaitNode(attackDelay, attackDelayRandom);

            flySequence.AddChild(flyPattern);
            flySequence.AddChild(flyAction);
            flySequence.AddChild(flyWait);

        }
        Decorator_Composit<int> flyDeco =
            new Decorator_Composit<int>("flyNode", flySequence, this.gameObject,
            blackboard, "DragonPattern", 5);

        TaskNode_ActionEnd attkendNode = new TaskNode_ActionEnd(this.gameObject, blackboard);

        patternSelector.AddChild(biteDeco);
        patternSelector.AddChild(wingDeco);
        patternSelector.AddChild(fireballDeco);
        patternSelector.AddChild(firebreathDeco);
        patternSelector.AddChild(flyDeco);
        //patternSelector.AddChild(attkendNode);

        return patternSelector;
    }

    #endregion

    protected override bool CheckMode()
    {
        bool bCheck = base.CheckMode();
        bCheck |= state.IdleMode == false;

        if (health != null)
            bCheck |= health.Dead;

        return bCheck;
    }
    public override void SetWaitMode(bool isDamaged = false)
    {
        if (WaitMode == true)
            return;

        base.SetWaitMode(isDamaged);

        //waitCondition = WaitCondition.Idle;
        DeicideWaitCondition();

        //DeceideActionPattern();
    }

    public override void SetPatrolMode()
    {
        // 드래곤은 움직이게 되면 스테이트가 변경된다. 
        //animator.SetInteger("State", );
        cur_DragonState = DragonState.Move;
        base.SetPatrolMode();
        NavMeshUpdateRotationSet();
        navMeshAgent.stoppingDistance = 2.5f;
    }

    public override void SetDamagedMode()
    {
        if (DamagedMode == true)
            return;
        StartCoroutine(RecoveryDamage());
        //ChangeType(AIStateType.Damaged);
    }

    private IEnumerator RecoveryDamage()
    {
        yield return new WaitForSeconds(0.5f);
        End_Damage();
    }

    public override void End_Damage()
    {
        //base.End_Damage();
        //state.SetIdleMode();
        //SetWaitMode(true);
    }

    protected override void ChangeType(AIStateType type)
    {
        base.ChangeType(type);

        animator.SetInteger("State", (int)cur_DragonState);
    }

    private void DeicideWaitCondition()
    {
        if (NoneCondition == false)
            return;

        int maxConditionValue = maxWaitCondtionPattern;
        GameObject player = perception.GetPercievedPlayer();
        if (player == null)
            maxConditionValue = (int)WaitCondition.Idle;

        int num = Random.Range(1, maxConditionValue);


        WaitCondition codition = (WaitCondition)num;
        //Debug.Log($"Decide Wait Condition  {codition}");

        switch (codition)
        {
            case WaitCondition.Idle:
            cur_DragonState = DragonState.Idle;
            SetWaitState_IdleCondition();
            break;

            case WaitCondition.Strafe:
            cur_DragonState = DragonState.Move;
            NavMeshUpdateRotationSet();
            SetWaitState_StrafeCondition();
            break;

            default:
            SetWaitState_IdleCondition();
            break;
        }

    }

    private Coroutine patternDecideCoroutine;
    private void DeceideActionPattern()
    {
        if (health == null)
        {
            return;
        }
     
        // 날아 오르고 화염 
        if (health.GetCurrentHPByPercent <= 0.3f && dragonPatternTable[5].Usable)
        {
            currentAttackPattern = 5;
            blackboard.SetValue("DragonPattern", currentAttackPattern);
            return;
        }


        if(patternDecideCoroutine == null)
            patternDecideCoroutine = StartCoroutine(DecidePatternCoroutine());

    }

    private void ChangeAttackRange()
    {
        if (currentAttackPattern == 1 || currentAttackPattern == 0)
        {
            attackRange = 5.5f;
        }
        else if (currentAttackPattern == 2)
        {
            attackRange = 5.1f;
        }
        else if (currentAttackPattern == 3)
        {
            attackRange = 9.0f;
        }
        else if (currentAttackPattern == 4 || currentAttackPattern == 5)
        {
            attackRange = 12.0f;
        }
    }

    private IEnumerator CoolDownCoroutine()
    {
        while (true)
        {
            foreach (KeyValuePair<int, DragonPattern> keyValuePair in dragonPatternTable)
            {
                if (keyValuePair.Value.coolDown > 0)
                    keyValuePair.Value.coolDown -= Time.deltaTime;
            }

            yield return null;
        }
    }

    private IEnumerator DecidePatternCoroutine()
    {
        int maxLoopCount = 5;
        int loopCount = 0;

        //1. 물기 2. 날개 치기 3. 화염 발사 4. 날아오르고 화염쏘기 
        int maxActionPaternValue = 5;

        while (true)
        {
            if (loopCount >= maxLoopCount)
            {
                currentAttackPattern = 0;
                //Debug.Log($"Dragon is No {currentAttackPattern} Decided pattern");
                blackboard.SetValue("DragonPattern", currentAttackPattern);
                patternDecideCoroutine = null;
                SetWaitMode(false);
                yield break;
            }

            loopCount++;

            int num = Random.Range(1, maxActionPaternValue);

            if (dragonPatternTable[num].Usable)
            {
                currentAttackPattern = num;
                //Debug.Log($"{currentAttackPattern} Decided pattern");
                blackboard.SetValue("DragonPattern", currentAttackPattern);
                patternDecideCoroutine = null;
                yield break;
            }

            yield return null;
        }
    }


    protected override void OnBeginDoAction()
    {
        base.OnBeginDoAction();

        dragonPatternTable[currentAttackPattern].SetCoolDown();
        if (currentAttackPattern == 5)
        {
            cur_DragonState = DragonState.Fly;
        }
    }


    protected override void OnEndDoAction()
    {
        // 공격을 끝냈으니 패턴을 초기화
        currentAttackPattern = 0; 

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

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);

    }
#endif
}
