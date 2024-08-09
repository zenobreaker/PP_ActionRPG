using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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


    private new Rigidbody rigidbody;
    private NavMeshAgent agent;

    private StateComponent state; 
    private GroundedComponent ground;
    private OtherStateColliderComponent otherCollider;

    private bool bAir = false;
    private bool bSuperArmor = false;

    private float originDrag;
    private float originMass;
    private Coroutine airCoroutine;
    private Coroutine useGravityCoroutine;

    public event Action<float> OnChangeAirState;


    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        Debug.Assert(rigidbody != null);
        originDrag = rigidbody.drag;
        originMass = rigidbody.mass;

        state = GetComponent<StateComponent>();
        Debug.Assert(state != null);

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

    private void SetAirMode(bool bAir)
    {
        this.bAir = bAir;
    }

    private void OnGround()
    {
        otherCollider?.SetAirStateCollider(false);
        SetAirMode(false);

        if (agent != null)
            agent.enabled = true;
    }

    private bool CheckAttackerAboutData(GameObject attacker, Weapon causer, DoActionData data)
    {
        bool bCheck = true;
        bCheck &= (attacker != null);
        bCheck &= (causer != null);
        bCheck &= (data != null);
        bCheck &= rigidbody != null; 

        return bCheck;
    }

    public void DoAir(GameObject attacker, Weapon causer, DoActionData data, bool targetView = false,
        CharacterGrade grade = CharacterGrade.Common)
    {
        bool result = CheckAttackerAboutData(attacker, causer, data);
        if (result == false)
            return;

        //TODO: 따로 빼야함
        if (grade == CharacterGrade.Boss)
            bSuperArmor = true;

        BeginDoAir(data);

        DoAirborneLaunch(attacker, causer, data);
    }


    #region Air_Launch
    // 공중 상태에 맞았다면 관련된 컴포넌트나 변수들을 관리 한다. 
    private void DoAirborneLaunch(GameObject attacker, Weapon causer, DoActionData data)
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

        DoLaunch();
    }


    private void DoLaunch()
    {
        
    }

    private IEnumerator On_AirCombo(DoActionData data)
    {
        rigidbody.useGravity = false;
        rigidbody.velocity = Vector3.zero;

        //float distance = Mathf.Clamp(data.heightValue * 1.0f , 1.0f, data.heightValue);

        yield return new WaitForSecondsRealtime(airMaintainTime + data.airConditionTime);

        Debug.Log($"Change_UseGravity time is over- {true}");

        if (bAir)
            rigidbody.useGravity = true;

    }

    #endregion


    #region Airbone

    // 공중에 띄우는 함수
    private void BeginDoAir(DoActionData data)
    {
        if (bSuperArmor)
            return;

        // 공중 상태이고 여기까지 왔다면 이전에 히트를 당했다는 의미이다. 
        float value = data.heightValue;
        if (bAir)
        {
            if (data.heightValue == 0)
                value = additionalAccel;

            Debug.Log($"공중 콤보 실시! ");
        }

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
        Vector3 startPosition = transform.localPosition;
        Vector3 endPosition = transform.localPosition + (Vector3.up * distance);
        // 목표 높이 
        float targetDistance = Vector3.Distance(endPosition, startPosition);

        if (agent != null)
            agent.enabled = false;

        yield return new WaitForFixedUpdate();

        rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionY;
        rigidbody.drag = 0;
        rigidbody.isKinematic = false;
        //rigidbody.AddForce(Vector3.up *distance * acceleration, forceMode);
        rigidbody.AddForce(Vector3.up * acceleration, forceMode);

        state?.SetAirborneMode();

        // 이벤트 호출 
        OnChangeAirState?.Invoke(0.3f);

        otherCollider?.SetAirStateCollider(true);

        //TODO 여기서 높이 계산을 수정해야할 듯..?
        // 목표 높이까지 올라갔는지 매 FixedUpdate마다 체크 
        while (targetDistance >= 0)
        {
            targetDistance = endPosition.y - transform.localPosition.y;

            yield return new WaitForFixedUpdate();
        }
        ground.SetGroundCheck(true);

        // 목표까지 띄웠다면 관련된 변수들 변경 
        Debug.Log("다 띄웠당");
        // 일정거리까지 올라가면 멈추게함.
        rigidbody.velocity = Vector3.zero;
        rigidbody.useGravity = true;
        rigidbody.isKinematic = false;
    }


    #endregion
}
