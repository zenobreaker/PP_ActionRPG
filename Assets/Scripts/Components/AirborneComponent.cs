using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static StateComponent;

public class AirborneComponent : MonoBehaviour

{
    // � ������ ������ ������� ���� ���� 
    [SerializeField] private ForceMode forceMode;
    // ����� �� ���ӵ��� 
    [Range(1.0f, 100.0f)][SerializeField] private float acceleration = 1.0f;
    // �߰� ���� ��ġ 
    [SerializeField] private float additionalAccel = 0.0f;
    // ���� ���� �ð� 
    [SerializeField] private float airMaintainTime = 0.0f;
    // �ּ� ���� ���� ��
    [SerializeField] private float minLaunchHeight = 0.1f;
    // ���� ��� 
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

        //TODO: ���� ������
        if (grade == CharacterGrade.Boss)
            bSuperArmor = true;

        BeginDoAir(data);

        if (causer.SubAction)
            DoAirFreeze(0.75f);
      //  DoAirborneLaunch(attacker, causer, data);
    }

    #region Air_Launch
    // ���� ���¿� �¾Ҵٸ� ���õ� ������Ʈ�� �������� ���� �Ѵ�. 
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

    // ���߿� ����
    private void BeginDoAir(ActionData data)
    {
        if (bSuperArmor)
            return;

        // ���� �����̰� ������� �Դٸ� ������ ��Ʈ�� ���ߴٴ� �ǹ��̴�. 
        // �� ����� �����ȴٴ� �� ���� ������ �� �� �ִ�.
        float value = data.heightValue;
        if (data.heightValue == 0)
            value = additionalAccel;
        if(prevType == StateType.Airborne)
        {
            float reducedHeight = value * Mathf.Pow(heightReductionFactor, transform.position.y);
            value = Mathf.Max(reducedHeight, minLaunchHeight);
        }

        Debug.Log($"���� �޺� �ǽ�! {value} �� ���ϴ�.");


        // ���߿� �� �ִٸ� ���� ���� ���� �ڷ�ƾ�� ���� �ʿ䰡 ����. 
        if (airCoroutine != null)
            StopCoroutine(airCoroutine);

        airCoroutine = StartCoroutine(Change_Airbone(value));
    }

    // ���߿� ������� �ڷ�ƾ 
    private IEnumerator Change_Airbone(float distance)
    {
        // �ش� ������ ���� �޺� ���¸� ���صǴ� ���� 
        //if (ground.IsGround == false)
        //    yield break;
        if (distance == 0)
            yield break;

        Debug.Log($"���� ����? {distance}");
        float startY = transform.localPosition.y;
        float endY = (transform.localPosition + (Vector3.up * distance)).y;
        // ��ǥ ���� 
        float targetDistance = endY - startY;

        rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionY;
        rigidbody.drag = 0;
        rigidbody.isKinematic = false;
        // ����s�� �������� ������ ��� ���߱� 
        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce(Vector3.up * acceleration, forceMode);

        // ������Ʈ ���� 
        state?.SetAirborneMode();

        otherCollider?.SetAirStateCollider(true);
        OnAirborneChange?.Invoke();

        // ��ǥ ���̱��� �ö󰬴��� �� FixedUpdate���� üũ 
        while (targetDistance >= 0)
        {
            targetDistance = endY - transform.position.y;

            yield return new WaitForFixedUpdate();
        }


        // ��ǥ���� ����ٸ� ���õ� ������ ���� 
        Debug.Log("�� �����");
        // �����Ÿ����� �ö󰡸� ���߰���.
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
