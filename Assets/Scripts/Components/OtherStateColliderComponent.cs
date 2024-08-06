using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherStateColliderComponent : MonoBehaviour
{
    protected new Collider collider;
    protected Vector3 originalCenter;
    protected float originalHeight;


    [SerializeField]
    private float airStateRatio = 1.5f;
    private float donwCenterY = 1.5f;

    protected CapsuleCollider capsuleCollider;
    private float originRadius; // 캡슐콜라이더 전용..

    protected new Rigidbody rigidbody;

    private void Awake()
    {
        collider = GetComponent<Collider>();
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (collider != null)
        {
            CapsuleCollider capsule = collider as CapsuleCollider;
            if (capsule != null)
            {
                capsuleCollider = capsule;
                originalCenter = capsuleCollider.center;
                originalHeight = capsuleCollider.height;
                originRadius = capsuleCollider.radius;
            }
        }
    }

    // 체공 상태일 때 콜라이더 범위를 늘려준다. 
    public void SetAirStateCollider(bool state)
    {
        if (capsuleCollider == null)
            return;

        Debug.Log($"SetAirStateCollider - {state}");

        if (state == true)
        {
            
            capsuleCollider.height = originalHeight * airStateRatio;
            capsuleCollider.center = 
                new Vector3(originalCenter.x, originalCenter.y - donwCenterY, originalCenter.z);
        }
        else
        {
            capsuleCollider.center = originalCenter;
            capsuleCollider.height = originalHeight;
        }

    }

    public void SetDownCollider(bool state)
    {
        if (capsuleCollider == null)
            return;

        if (state == true)
        {
            //capsuleCollider.center = new Vector3(originalCenter.x, originalCenter.y, originalCenter.z);
            capsuleCollider.center = new Vector3(originalCenter.x, 0, -0.5f);
            //capsuleCollider.height = originalHeight; // 높이를 절반으로 줄입니다.
            capsuleCollider.direction = 2; // z축을 기준으로 회전
            capsuleCollider.radius = 1;
            capsuleCollider.height = 3;
            if (rigidbody != null)
                rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionY;
        }
        else
        {
            // 콜라이더를 원래 상태로 되돌리는 로직
            capsuleCollider.center = originalCenter;
            capsuleCollider.height = originalHeight;
            capsuleCollider.radius = originRadius;
            capsuleCollider.direction = 1; // y축을 기준으로 회전
            if (rigidbody != null)
                rigidbody.constraints |= RigidbodyConstraints.FreezePositionX;
        }
    }
}
