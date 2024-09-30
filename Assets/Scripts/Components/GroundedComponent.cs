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
    private bool privoudGround; // 이전 상태 저장값
    public bool IsGround { get => bGround; }

    private bool bCheck = true;
    private bool bDistanceCheck = false;
    private Vector3 checkOnPosition = Vector3.zero;

    [SerializeField] private float checkDistance = 0.5f;

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

        privoudGround = bGround;
        groundLayer = 1 << LayerMask.NameToLayer("Ground");
    }

    private void FixedUpdate()
    {
        //FixedUpdate_CheckOverDistance();
        FixedUpdate_CheckGrounded();
    }

    private void FixedUpdate_CheckGrounded()
    {
        //if (bCheck == false)
        //    return;
   
        Vector3 boxSize = new Vector3(transform.lossyScale.x, checkDistance, transform.lossyScale.z);
        Vector3 center = transform.position;
        center.y = -checkDistance * 0.5f;
        Collider[] colliders = Physics.OverlapBox(center, boxSize, Quaternion.identity, groundLayer);

        // 자신의 아래로 레이를 쏴서 다른 오브젝트가 있다면 
        GameObject candidate = null; 
        foreach (Collider collider in colliders)
        {
            // 땅레이어 인지 검사 혹은 다른 것도 사관 없음 
            int layer = collider.gameObject.layer;
            if ((groundLayer & (1 << layer)) == 0)
                continue;

            candidate = collider.gameObject;
            break; 
        }

        if (candidate == null)
            return;
        // 대상과의 거리 측정 
        float distance = Mathf.Abs(candidate.transform.position.y - transform.position.y);

        if (distance >= 0.004f)
            bGround = false;
        else
            bGround = true;

        ChangeGroundState(bGround);

        //bGround = true;
        //bDistanceCheck = false;
        //Change_RigidBodyToGround();

        //StopAllCoroutines();
        //bGround = false;
    }


    private void ChangeGroundState(bool newGroundState)
    {
        if (privoudGround == newGroundState)
            return;

        privoudGround = newGroundState;

        // 상태가 달라졌으니 관련 이벤트 콜
        // 이전 상태가 땅에 있지 않았고 다음에 오는 상태가 땅에 오는 거면 처리하기 
        if (privoudGround == false && newGroundState == true)
        {
            OnChangedGorund?.Invoke();
            OnCharacterGround?.Invoke();
        }
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

        if (y >= checkDistance)
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
        Vector3 boxSize = new Vector3(transform.lossyScale.x, -checkDistance, transform.lossyScale.z);
        Vector3 center = transform.position;
        center.y = -checkDistance * 0.5f;
        Gizmos.DrawWireCube(center, boxSize);

        Vector3 from = transform.position;
        Vector3 to = center;
        Gizmos.DrawLine(from, to);
    }

#endif

}
