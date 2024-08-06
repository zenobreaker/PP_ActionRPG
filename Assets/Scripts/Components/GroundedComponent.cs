using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// 땅에 닿아있는지 확인해주는 컴포넌트 
/// </summary>
public class GroundedComponent : MonoBehaviour
{
    private new Rigidbody rigidbody;

    [SerializeField]
    private bool bGround = true;
    public bool IsGround { get => bGround; }

    private bool bCheck = true;
    public bool IsCheck { get => bCheck; }
    public void SetGroundCheck(bool value)
    {
        bCheck = value;
    }

    [SerializeField]  private float distance = 0.1f;
    private LaunchComponent launchComponent;

    private float originMass;
    private float originDrag;
    private RigidbodyConstraints originConstraints;

    private int groundLayer;

    public event Action OnChangedGorund;
    public event Action OnCharacterGround;


    private Coroutine coroutineStartDelayCheck;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        Debug.Assert(rigidbody != null);

        originMass = rigidbody.mass;
        originDrag = rigidbody.drag;
        originConstraints = rigidbody.constraints;

        groundLayer = 1 << LayerMask.NameToLayer("Ground");
        launchComponent = GetComponent<LaunchComponent>();  
        Debug.Assert(launchComponent != null);
        launchComponent.OnChangeAirState += OnStartCheckGroundByDelay;
    }

    private void FixedUpdate()
    {
        FixedUpdate_CheckGrounded();
    }

    private void FixedUpdate_CheckGrounded()
    {
        if (bCheck == false)
            return;
        Vector3 boxSize = new Vector3(transform.lossyScale.x, distance, transform.lossyScale.z);
        Collider[] colliders = Physics.OverlapBox(transform.position, boxSize , Quaternion.identity, groundLayer);
        
        foreach (Collider candidate in colliders)
        {
            int layer = candidate.gameObject.layer;
            if ((groundLayer & (1 << layer)) == 0)
                continue;

            Debug.Log($"땅 체크 있는 듯 {candidate.gameObject.name}");
            bGround = true;
            Change_RigidBodyToGround(candidate.gameObject);
            return;
        }

        StopAllCoroutines();
        bGround = false;
    }

    private void Change_RigidBodySettinToAir()
    {
        rigidbody.isKinematic = false;
        rigidbody.useGravity = true;
    }

    private void Change_RigidBodyToGround(GameObject groundObject)
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



    #region Event
    private void OnStartCheckGroundByDelay(float delay = 0.0f)
    {
        if (coroutineStartDelayCheck != null)
            StopCoroutine(coroutineStartDelayCheck);

        coroutineStartDelayCheck = StartCoroutine(Start_DelayCheck(delay));
    }

    private IEnumerator Start_DelayCheck(float delay)
    {
        yield return new WaitForSeconds(delay);
        bCheck = true; 
    }
    

    #endregion


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
