using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static StateComponent;

/// <summary>
/// ������ ������ �������� ���� �и��� ����� ���ִ� ������Ʈ
/// </summary>
public class LaunchComponent : MonoBehaviour
{
    private StateComponent state;
    private AirborneComponent airborne;
    private OtherStateColliderComponent otherCollider;
    private new Rigidbody rigidbody;
    private NavMeshAgent agent;

    [SerializeField] private AnimationCurve knockbackCurve;
    [SerializeField] private float knockbackTime = 1.0f;

    private bool bSuperArmor = false;
    private StateType prevType;

    private float originDrag;
    private float originMass;


    private void Awake()
    {
        state = GetComponent<StateComponent>();
        Debug.Assert(state != null);
        state.OnStateTypeChanging += OnStateTypeChanging;

        otherCollider = GetComponent<OtherStateColliderComponent>();
        airborne = GetComponent<AirborneComponent>();    
        agent = GetComponent<NavMeshAgent>();

        rigidbody = GetComponent<Rigidbody>();
        Debug.Assert(rigidbody != null);
        originDrag = rigidbody.drag;
        originMass = rigidbody.mass;


    }

    private void OnStateTypeChanging(StateType prevType)
    {
        this.prevType = prevType;
    }

    private bool CheckAttackerAboutData(GameObject attacker, Weapon causer, ActionData data)
    {
        bool bCheck = true;
        bCheck &= (attacker != null);
        bCheck &= (causer != null);
        bCheck &= (data != null);

        return bCheck;
    }



    public void DoHit(GameObject attacker, Weapon causer, ActionData data, bool targetView = false,
        CharacterGrade grade = CharacterGrade.Common)
    {

        bool result = CheckAttackerAboutData(attacker, causer, data);
        if (result == false)
            return;

        if (grade == CharacterGrade.Boss)
            bSuperArmor = true;

        // ���� �ٶ� ��� �ٶ󺸱� 
        if (targetView)
            StartCoroutine(Change_Rotate(attacker));

        // ��ġ ����
        DoLaunch(attacker, causer, data);
    }



    #region Rotate

    private IEnumerator Change_Rotate(GameObject target)
    {

        // �� �ڵ�� y �� ���̰� �Ͼ�� x�� ȸ���� �ع�����. 
        //transform.LookAt(target.transform, Vector3.up);

        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0; // Y�� ȸ���� �����մϴ�.

        // ������ 0�� �ƴϸ� ȸ���մϴ�.
        if (direction != Vector3.zero)
        {
            // ���ο� ȸ�� ���� ����մϴ�.
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            // ���� ȸ���� ���ο� ȸ�� ������ �����մϴ�.
            transform.rotation = targetRotation;
        }

        yield return null;
    }
    #endregion

    #region Launch

    /// <summary>
    /// ������ �°� �Ǹ� ������ Ÿ�Կ� ���� �и��� ������ �Ǻ��Ѵ�. 
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="causer"></param>
    /// <param name="data"></param>
    private bool CheckDoLauch(GameObject attacker, Weapon causer, ActionData data)
    {
        // ���� ���Ⱑ �ƴ϶�� ��� �Ÿ��� ��� ���� �ش� ������ �����ͷ� ó���Ѵ�. 
        Melee melee = causer as Melee;
        if (melee == null)
            return true;


        float distance = Vector3.Distance(attacker.transform.localPosition, transform.localPosition);

        // �����ڿ��� �Ÿ��� �и��� �Ÿ����� ���̰� ũ�ų� ���ٸ� �и��� �ʴ´�.
        if (distance >= data.Distance)
        {
            //Debug.Log("Too far");
            return false;
        }

        return true;
    }


    private IEnumerator Change_IsKinematics(ActionData data, int frame)
    {

        for (int i = 0; i < frame; i++)
            yield return new WaitForFixedUpdate();

        if (state?.AirborneMode == false)
        {

            rigidbody.isKinematic = true;
        }
    }

    private void DoLaunch(GameObject attacker, Weapon causer,
        ActionData data)
    {
        bool bResult = true;
        bResult &= CheckDoLauch(attacker, causer, data);
        bResult &= data.bLauncher == false; 

        float distanace = data.Distance;
        float launch = rigidbody.drag * distanace * 10.0f;

        Vector3 forceDir = attacker.transform.forward;
        var fm = ForceMode.Force;

        if (data.bLauncher)
        {
            StartCoroutine(Do_Knockback(forceDir.normalized, distanace, knockbackTime));
            return; 
        }
            
        if (prevType == StateType.Airborne)
        {
            rigidbody.mass = originMass /** 0.05f*/;
            fm = ForceMode.Impulse;
            launch = rigidbody.mass * distanace;
            Debug.Log($"air launcher => {launch}");
        }

        if (bResult)
        {
            rigidbody.isKinematic = false;
            rigidbody.AddForce(forceDir * launch, fm);
        }

        if(airborne != null && data.heightValue > 0.0f || prevType == StateType.Airborne)
            airborne.DoAir(attacker, causer, data);
        else 
            StartCoroutine(Change_IsKinematics(data, 5));
    }


    private IEnumerator Do_Knockback(Vector3 direciton, float distance, float knockbackTime)
    {
        float elapsedTime = 0;
        Vector3 startPositin = transform.position;

        while(elapsedTime < knockbackTime)
        {
            float curveValue = knockbackCurve.Evaluate(elapsedTime / 2);
            Vector3 resultPos = startPositin + direciton * curveValue * distance;

            transform.position = new Vector3(resultPos.x, transform.position.y, resultPos.z);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    #endregion


}
