using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static ConditionComponent;
using static StateComponent;
using static UnityEngine.Rendering.DebugUI;

public class AirborneComponent : MonoBehaviour

{
    [SerializeField] private ForceMode forceMode;
    
    [Range(1.0f, 100.0f)][SerializeField] private float acceleration = 1.0f;
    
    [SerializeField] private float additionalAccel = 0.0f;
    
    [SerializeField] private float airMaintainTime = 0.0f;
    
    [SerializeField] private float minLaunchHeight = 0.1f;
    
    [SerializeField] private float heightReductionFactor = 0.5f;

    private new Rigidbody rigidbody;
    private NavMeshAgent agent;

    private ConditionComponent condition;
    private StateComponent state;
    private GroundedComponent ground;
    private OtherStateColliderComponent otherCollider;

    private bool bSuperArmor = false;
    private bool bAir = false; 
    public bool AirCondition { get => bAir; }
    public void SetAirCondition() => bAir = true;
    public void SetGroundCondition() =>bAir = false;

    private ConditionType conditionType;

    private Coroutine airCoroutine;

    public event Action OnAirborneChange;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        Debug.Assert(rigidbody != null);

        condition = GetComponent<ConditionComponent>();
        if (condition != null)
             condition.OnConditionChanged += OnConditionChanged;
       state = GetComponent<StateComponent>();
        Debug.Assert(state != null);

        ground = GetComponent<GroundedComponent>();
        Debug.Assert(ground != null);
        ground.OnChangedGorund += OnChangeGround;

        otherCollider = GetComponent<OtherStateColliderComponent>();
        agent = GetComponent<NavMeshAgent>();

    }

    [SerializeField] private bool bDebuMode = false;
    private void Update()
    {
#if UNITY_EDITOR
        if (bDebuMode)
        {
            if (Input.GetKeyDown(KeyCode.V))
            {
                StopAllCoroutines();
                StartCoroutine(Change_Airbone(5));
                //rigidbody.isKinematic = true;
                //rigidbody.useGravity = false;
                //transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            }
        }
#endif
    }

    private void OnConditionChanged(ConditionType prevType, ConditionType newType)
    {
        if (condition == null)
            return;

        bool bEnable = false; 
        conditionType = newType;
    
        if (newType != ConditionType.None)
        {
            bEnable = false;
        }
        else
        {
            bEnable = true;
        }

        if (agent != null)
            agent.enabled = bEnable;
    }

    public void OnChangeGround()
    {
        // 이 메소드가 호출되면 땅에 닿은 것
        SetGroundCondition();
    }
   

    private bool CheckAttackerAboutData(GameObject attacker, Weapon causer, ActionData data)
    {
        bool bCheck = true;
        bCheck &= (attacker != null);
        bCheck &= (causer != null);
        bCheck &= (data != null);
        bCheck &= rigidbody != null;

        return bCheck;
    }

    public void DoAir(GameObject attacker, Weapon causer, ActionData data, bool targetView = false,
        CharacterGrade grade = CharacterGrade.Common)
    {
        bool result = CheckAttackerAboutData(attacker, causer, data);
        if (result == false)
            return;

        
        if (grade == CharacterGrade.Boss)
            bSuperArmor = true;

        BeginDoAir(data);

        if (causer.SubAction)
            DoAirFreeze(1);
      //  DoAirborneLaunch(attacker, causer, data);
    }


    #region Airbone

    private void BeginDoAir(ActionData data)
    {
        if (bSuperArmor)
            return;

        float value = data.heightValue;
        //Debug.Log($"Air comobo first step {value}");
        if (data.heightValue == 0 && bAir)
            value = additionalAccel;

        float positionY = Mathf.Abs(transform.position.y);

        Debug.Log($"Air comobo step {conditionType}");

        if (conditionType == ConditionType.Airborne ||
            conditionType == ConditionType.Down)
        {
            float reducedHeight = value * Mathf.Pow(heightReductionFactor, positionY);
            value = Mathf.Max(reducedHeight, minLaunchHeight);
            Debug.Log($"Air comobo second step{value}");
        }


        if (airCoroutine != null)
            StopCoroutine(airCoroutine);

        airCoroutine = StartCoroutine(Change_Airbone(value));
    }

    private IEnumerator Change_Airbone(float distance)
    {
        //if (ground.IsGround == false)
        //    yield break;
        if (distance == 0)
            yield break;

        float startY = transform.localPosition.y;
        float endY = startY + distance; // 현재 y 위치에서 distance 만큼 더함
        float targetDistance = distance; // 바로 공중으로 올릴 거리 설정

        rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionY;
        rigidbody.drag = 0;
        rigidbody.isKinematic = false;
        
        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce(Vector3.up * acceleration, forceMode);

        SetAirCondition();
        condition?.SetAirborneCondition();
        otherCollider?.SetAirStateCollider(true);

        
        while (targetDistance >= 0 && transform.position.y < endY)
        {
            targetDistance = endY - transform.position.y;

            yield return new WaitForFixedUpdate();
        }

        OnAirborneChange?.Invoke();

        rigidbody.velocity = Vector3.zero;
        
        rigidbody.useGravity = true;
    }

    #endregion


    Coroutine airFreezeCoroutine;
    public void DoAirFreeze(float delay)
    {
        if (condition == null)
            return;
        if (condition.AirborneCondition == false)
            return;

        if (ground.IsGround)
            return;

        if (airFreezeCoroutine != null)
            StopCoroutine(airFreezeCoroutine);
    
        airFreezeCoroutine = StartCoroutine(AirFreeze(delay));
    }

    private IEnumerator AirFreeze(float delay)
    {
        rigidbody.isKinematic = true;
        yield return new WaitForSeconds(delay);
        rigidbody.isKinematic = false;
        
    }
}
