using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static StateComponent;

public class AirborneComponent : MonoBehaviour

{
    // 어떤 힘으로 힘으로 띄울지에 대한 변수 
    [SerializeField] private ForceMode forceMode;
    // 띄워질 때 가속도값 
    [Range(1.0f, 100.0f)][SerializeField] private float acceleration = 1.0f;
    // 추가 가속 수치 
    [SerializeField] private float additionalAccel = 0.0f;
    // 공중 유지 시간 
    [SerializeField] private float airMaintainTime = 0.0f;
    // 최소 띄우는 높이 값
    [SerializeField] private float minLaunchHeight = 0.1f;
    // 감소 계수 
    [SerializeField] private float heightReductionFactor = 0.5f;

    private new Rigidbody rigidbody;
    private NavMeshAgent agent;
    private Animator animator;

    private StateComponent state;
    private GroundedComponent ground;
    private OtherStateColliderComponent otherCollider;

    private bool bSuperArmor = false;
    private StateType prevType;

    private Coroutine airCoroutine;
    private Coroutine useGravityCoroutine;

    public event Action OnAirborneChange;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        Debug.Assert(rigidbody != null);

        animator = GetComponent<Animator>();

        state = GetComponent<StateComponent>();
        Debug.Assert(state != null);
        state.OnStateTypeChanging += OnStateTypeChanging;
        state.OnStateTypeChanged += OnStateTypeChanged;

        ground = GetComponent<GroundedComponent>();
        Debug.Assert(ground != null);
        ground.OnChangedGorund += OnGround;

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
                rigidbody.isKinematic = true;
                rigidbody.useGravity = false;
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            }
        }
#endif
    }
    private void OnStateTypeChanging(StateType prevType)
    {
        this.prevType = prevType;
    }

    private void OnStateTypeChanged(StateType prevType, StateType newType)
    {
        if (newType == StateType.Airborne && 
            state.DownCondition == false)
        {
            animator.SetBool("Airial", true);
        }
        else
            animator.SetBool("Airial", false);
    }

    private void OnGround()
    {
        otherCollider?.SetAirStateCollider(false);

        if (agent != null)
            agent.enabled = true;
        
        state.SetIdleMode();
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

        //TODO: 따로 빼야함
        if (grade == CharacterGrade.Boss)
            bSuperArmor = true;

        BeginDoAir(data);

        if (causer.SubAction)
            DoAirFreeze(0.75f);
      //  DoAirborneLaunch(attacker, causer, data);
    }

    #region Air_Launch
    // 공중 상태에 맞았다면 관련된 컴포넌트나 변수들을 관리 한다. 
    private void DoAirborneLaunch(GameObject attacker, Weapon causer, ActionData data)
    {
        bool result = CheckAttackerAboutData(attacker, causer, data);
        if (result == false)
            return;

        if (ground.IsGround == true)
            return;

        if (state?.AirborneMode == false)
            return;

        if (useGravityCoroutine != null)
            StopCoroutine(useGravityCoroutine);

        useGravityCoroutine = StartCoroutine(On_AirCombo(data));

    }


    private IEnumerator On_AirCombo(ActionData data)
    {
        rigidbody.useGravity = false;
        rigidbody.velocity = Vector3.zero;

        //float distance = Mathf.Clamp(data.heightValue * 1.0f , 1.0f, data.heightValue);

        yield return new WaitForSecondsRealtime(airMaintainTime);

        Debug.Log($"Change_UseGravity time is over- {true}");

        if (state?.AirborneMode ?? true)
            rigidbody.useGravity = true;

    }

    #endregion


    #region Airbone

    // 공중에 띄우기
    private void BeginDoAir(ActionData data)
    {
        if (bSuperArmor)
            return;

        // 공중 상태이고 여기까지 왔다면 이전에 히트를 당했다는 의미이다. 
        // 이 기능이 실현된다는 건 공중 상태일 수 도 있다.
        float value = data.heightValue;
        if (data.heightValue == 0)
            value = additionalAccel;
        if(prevType == StateType.Airborne)
        {
            float reducedHeight = value * Mathf.Pow(heightReductionFactor, transform.position.y);
            value = Mathf.Max(reducedHeight, minLaunchHeight);
        }

        Debug.Log($"공중 콤보 실시! {value} 로 띄웁니다.");


        // 공중에 떠 있다면 굳이 진행 중인 코루틴을 지울 필요가 없다. 
        if (airCoroutine != null)
            StopCoroutine(airCoroutine);

        airCoroutine = StartCoroutine(Change_Airbone(value));
    }

    // 공중에 띄워지는 코루틴 
    private IEnumerator Change_Airbone(float distance)
    {
        // 해당 조건은 공중 콤보 상태면 방해되니 제거 
        //if (ground.IsGround == false)
        //    yield break;
        if (distance == 0)
            yield break;

        Debug.Log($"높이 띄울까? {distance}");
        float startY = transform.localPosition.y;
        float endY = (transform.localPosition + (Vector3.up * distance)).y;
        // 목표 높이 
        float targetDistance = endY - startY;

        rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionY;
        rigidbody.drag = 0;
        rigidbody.isKinematic = false;
        // 이전s에 움직임이 있으면 잠시 멈추기 
        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce(Vector3.up * acceleration, forceMode);

        // 스테이트 변경 
        state?.SetAirborneMode();

        otherCollider?.SetAirStateCollider(true);
        OnAirborneChange?.Invoke();

        // 목표 높이까지 올라갔는지 매 FixedUpdate마다 체크 
        while (targetDistance >= 0)
        {
            targetDistance = endY - transform.position.y;

            yield return new WaitForFixedUpdate();
        }


        // 목표까지 띄웠다면 관련된 변수들 변경 
        Debug.Log("다 띄웠당");
        // 일정거리까지 올라가면 멈추게함.
        rigidbody.velocity = Vector3.zero;
        rigidbody.useGravity = true;
    }

    #endregion


    Coroutine airFreezeCoroutine;
    public void DoAirFreeze(float delay)
    {
        if (state.AirborneMode == false)
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
