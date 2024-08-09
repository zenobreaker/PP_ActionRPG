using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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

        //TODO: ���� ������
        if (grade == CharacterGrade.Boss)
            bSuperArmor = true;

        BeginDoAir(data);

        DoAirborneLaunch(attacker, causer, data);
    }


    #region Air_Launch
    // ���� ���¿� �¾Ҵٸ� ���õ� ������Ʈ�� �������� ���� �Ѵ�. 
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

    // ���߿� ���� �Լ�
    private void BeginDoAir(DoActionData data)
    {
        if (bSuperArmor)
            return;

        // ���� �����̰� ������� �Դٸ� ������ ��Ʈ�� ���ߴٴ� �ǹ��̴�. 
        float value = data.heightValue;
        if (bAir)
        {
            if (data.heightValue == 0)
                value = additionalAccel;

            Debug.Log($"���� �޺� �ǽ�! ");
        }

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
        Vector3 startPosition = transform.localPosition;
        Vector3 endPosition = transform.localPosition + (Vector3.up * distance);
        // ��ǥ ���� 
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

        // �̺�Ʈ ȣ�� 
        OnChangeAirState?.Invoke(0.3f);

        otherCollider?.SetAirStateCollider(true);

        //TODO ���⼭ ���� ����� �����ؾ��� ��..?
        // ��ǥ ���̱��� �ö󰬴��� �� FixedUpdate���� üũ 
        while (targetDistance >= 0)
        {
            targetDistance = endPosition.y - transform.localPosition.y;

            yield return new WaitForFixedUpdate();
        }
        ground.SetGroundCheck(true);

        // ��ǥ���� ����ٸ� ���õ� ������ ���� 
        Debug.Log("�� �����");
        // �����Ÿ����� �ö󰡸� ���߰���.
        rigidbody.velocity = Vector3.zero;
        rigidbody.useGravity = true;
        rigidbody.isKinematic = false;
    }


    #endregion
}
