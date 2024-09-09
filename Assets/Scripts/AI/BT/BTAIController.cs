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

    [SerializeField] private string uiStateName = "EnemyAIState";

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }



}
