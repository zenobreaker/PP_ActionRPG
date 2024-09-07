using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OtherStateColliderComponent : MonoBehaviour
{
    protected new Collider collider;
    protected Vector3 originalCenter;
    protected float originalHeight;

    [SerializeField] private float airStateRatio = 1.5f;
    private float donwCenterY = 1.5f;

    protected CapsuleCollider capsuleCollider;
    protected new Rigidbody rigidbody;
    
    private float originRadius; // ĸ���ݶ��̴� ����..
    private bool bOriginTrigger;

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
                bOriginTrigger = capsuleCollider.isTrigger;
            }
        }
    }

    // ü�� ������ �� �ݶ��̴� ������ �÷��ش�. 
    public void SetAirStateCollider(bool state)
    {
        if (capsuleCollider == null)
            return;

        Debug.Log($"SetAirStateCollider - {state}");

        if (state == true)
        {
            capsuleCollider.isTrigger = true;
            capsuleCollider.height = originalHeight * airStateRatio;
            capsuleCollider.center = 
                new Vector3(originalCenter.x, originalCenter.y - donwCenterY, originalCenter.z);
        }
        else
        {
            capsuleCollider.isTrigger = bOriginTrigger;
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
            //capsuleCollider.height = originalHeight; // ���̸� �������� ���Դϴ�.
            capsuleCollider.direction = 2; // z���� �������� ȸ��
            capsuleCollider.radius = 1;
            capsuleCollider.height = 3;
            if (rigidbody != null)
                rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionY;
        }
        else
        {
            // �ݶ��̴��� ���� ���·� �ǵ����� ����
            capsuleCollider.center = originalCenter;
            capsuleCollider.height = originalHeight;
            capsuleCollider.radius = originRadius;
            capsuleCollider.direction = 1; // y���� �������� ȸ��
            if (rigidbody != null)
                rigidbody.constraints |= RigidbodyConstraints.FreezePositionX;
        }
    }
}
