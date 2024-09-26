using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// ���� ����ִ��� Ȯ�����ִ� ������Ʈ 
/// </summary>
public class GroundedComponent : MonoBehaviour
{
    private ConditionComponent condition;
    private StateComponent state;
    private AirborneComponent airborne;
    private new Rigidbody rigidbody;

    [SerializeField] private bool bGround = true;
    public bool IsGround { get => bGround; }

    private bool bCheck = true;
    private bool bDistanceCheck = false;
    private Vector3 checkOnPosition = Vector3.zero;

    [SerializeField] private float distance = 0.1f;

    private float originMass;
    private float originDrag;
    private RigidbodyConstraints originConstraints;

    private int groundLayer;

    public event Action OnChangedGorund;
    public event Action OnCharacterGround;


    private void Awake()
    {

        condition = GetComponent<ConditionComponent>();

        state = GetComponent<StateComponent>();
        Debug.Assert(state != null);

        airborne = GetComponent<AirborneComponent>();
        Debug.Assert(airborne != null);
        if(airborne != null)
            airborne.OnAirborneChange += OnAirborneChange;


        rigidbody = GetComponent<Rigidbody>();
        Debug.Assert(rigidbody != null);

        originMass = rigidbody.mass;
        originDrag = rigidbody.drag;
        originConstraints = rigidbody.constraints;

        groundLayer = 1 << LayerMask.NameToLayer("Ground");
    }

    private void FixedUpdate()
    {
        //FixedUpdate_CheckOverDistance();
        //FixedUpdate_CheckGrounded();
    }

    private void FixedUpdate_CheckGrounded()
    {
        if (bCheck == false)
            return;
        Vector3 boxSize = new Vector3(transform.lossyScale.x, distance, transform.lossyScale.z);
        Collider[] colliders = Physics.OverlapBox(transform.position, boxSize, Quaternion.identity, groundLayer);

        foreach (Collider candidate in colliders)
        {
            int layer = candidate.gameObject.layer;
            if ((groundLayer & (1 << layer)) == 0)
                continue;

            bGround = true;
            bDistanceCheck = false;
            Change_RigidBodyToGround();
            return;
        }

        StopAllCoroutines();
        bGround = false;
    }

    private void OnAirborneChange()
    {
        if (condition != null && condition.AirborneCondition == false)
            return;

        bDistanceCheck = true;
        checkOnPosition = transform.position;
    }

    private void FixedUpdate_CheckOverDistance()
    {
        if (bDistanceCheck == false)
            return;

        float y = transform.position.y - checkOnPosition.y;

        if (y >= distance)
        {
            bCheck = true;
            bGround = false; 
        }
    }

 
    private void Change_RigidBodyToGround()
    {
        //rigidbody.isKinematic = true;
        bCheck = false;

        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;

        rigidbody.mass = originMass;
        rigidbody.drag = originDrag;


        rigidbody.constraints = originConstraints;
        transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        OnChangedGorund?.Invoke();
        OnCharacterGround?.Invoke();
    }

    public void OnAirial()
    {
        rigidbody.useGravity = true;
        rigidbody.isKinematic = false;
        bDistanceCheck = true;
        checkOnPosition = transform.position;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (Selection.activeGameObject != this.gameObject)
            return;

        Gizmos.color = Color.green;
        Vector3 boxSize = new Vector3(transform.lossyScale.x, distance, transform.lossyScale.z);
        Gizmos.DrawWireCube(transform.position , boxSize);
    }

#endif

}
