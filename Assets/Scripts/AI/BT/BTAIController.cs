using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using static BTNode;

public class BTAIController : MonoBehaviour
{
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

    [SerializeField] bool bBT_DebugMode = false; 

    private NavMeshAgent navMeshAgent;
    public NavMeshAgent NavMeshAgent { get { return navMeshAgent; } }

    private BehaviorTreeRunner btRunner;

    private void Start()
    {
        btRunner = new BehaviorTreeRunner(CreateBTTree());
    }

    private void Update()
    {
        btRunner?.OperateNode(bBT_DebugMode);
    }


    private BTNode CreateBTTree()
    {
        return new SelectorNode
            (
                new List<BTNode>()
                {                 
                    new WaitNode()
                }
            ); 
    }

    
}
