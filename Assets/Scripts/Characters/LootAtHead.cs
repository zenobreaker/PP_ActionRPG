using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LootAtHead : MonoBehaviour
{
    private PerceptionComponent perception; 
    private Animator animator;

    [SerializeField] private float lookAtWeight = 1.0f;
    public Transform target;

    private GameObject targetObj; 
    private void Start()
    {
        animator = GetComponent<Animator>();
        perception = GetComponent<PerceptionComponent>();
    }

    private void Update()
    {
        GameObject player = perception.GetPercievedPlayer();
        if (player == null)
            return; 


    }

    private void OnAnimatorIK(int layerIndex)
    {
        Debug.Log(" call ");
        if (layerIndex != 3)
        {
            return; 
        }
        if (animator)
        {
            // IK를 활성화하고 머리 부분만 회전시키도록 설정
            animator.SetLookAtWeight(lookAtWeight);  // Head only
            animator.SetLookAtPosition(target.position);  // 목표 위치 설정
        }
    }



}
