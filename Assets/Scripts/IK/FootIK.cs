using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootIK : MonoBehaviour
{
    [SerializeField, Range(0, 1)] private float distance = 0;
    [SerializeField, Range(0, 1)] private float weight = 1;

    [SerializeField] private LayerMask layerMask;

    private CapsuleCollider capsule;
    private Animator animator;

    private void Awake()
    {
        capsule = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
    }

    private void OnAnimatorIK(int layerIndex)
    {

        Test_IK();
        //Vector3 position = transform.position;
        //position.y += capsule.center.y;

        ////레이를 쏜다
        //Ray ray = new Ray(position, Vector3.down * 2.0f);
        //Debug.DrawRay(ray.origin, ray.direction * (distance + 1), Color.green);
        //RaycastHit hit;
        //if (Physics.Raycast(ray, out hit, capsule.center.y, layerMask))
        //{
        //    float distance = hit.distance - capsule.center.y;
        //    position = transform.position;
        //    position.y -= distance;
        //    transform.position = position;
        //    //Debug.Log($"Foot IK {position}");

        //    // 얼마만큼에 가중치 => 0이면 원점 1이면 조정한 위치 떨어질땐 0에 가깝게 붙을땐 1로 
        //    SetFootIK(AvatarIKGoal.LeftFoot, distance);
        //    SetFootIK(AvatarIKGoal.RightFoot, distance);
        //}

    }


    private void Test_IK()
    {
        Vector3 position = transform.position;
        position.y += capsule.center.y / 2;

        Ray ray = new Ray(position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, capsule.center.y / 2 + 0.5f, layerMask) == true)
        {
            position = transform.position;

            float currentBaseY = transform.position.y;
            float detectedBaseY = hit.point.y;

            float gap = currentBaseY - detectedBaseY;

            position.y -= gap;
            transform.position = position;
        }

        SetFootIK(AvatarIKGoal.LeftFoot, distance);
        SetFootIK(AvatarIKGoal.RightFoot, distance);
    }


    private void SetFootIK(AvatarIKGoal goal, float adjsut)
    {
        animator.SetIKPositionWeight(goal, weight);
        animator.SetIKRotationWeight(goal, weight);


        Ray ray = new Ray(animator.GetIKPosition(goal) + Vector3.up, Vector3.down);
        Debug.DrawRay(ray.origin, ray.direction * (distance + 1), Color.green);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance + 1, layerMask))  // 나중엔 여길 레이어 마스크로 처리한다. 
        {
            Vector3 foot = hit.point;
            foot.y += distance - adjsut;

            animator.SetIKPosition(goal, foot);
            animator.SetIKRotation(goal, Quaternion.LookRotation(transform.forward, hit.normal));

        }
    }


    private void SetHandIK(AvatarIKGoal goal, float adjsut)
    {
        animator.SetIKPositionWeight(goal, weight);
        animator.SetIKRotationWeight(goal, weight);


        Ray ray = new Ray(animator.GetIKPosition(goal) + Vector3.up, Vector3.down);
        Debug.DrawRay(ray.origin, ray.direction * (distance + 1), Color.green);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance + 1, layerMask))  // 나중엔 여길 레이어 마스크로 처리한다. 
        {
            Vector3 foot = hit.point;
            foot.y += distance - adjsut;

            animator.SetIKPosition(goal, foot);
            animator.SetIKRotation(goal, Quaternion.LookRotation(transform.forward, hit.normal));

        }
    }
}
