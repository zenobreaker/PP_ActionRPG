using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static StateComponent;

/// <summary>
/// 공격을 맞으면 경직으로 인해 밀리는 기능을 해주는 컴포넌트
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

        // 나를 바라본 대상 바라보기 
        if (targetView)
            StartCoroutine(Change_Rotate(attacker));

        // 런치 실행
        DoLaunch(attacker, causer, data);
    }



    #region Rotate

    private IEnumerator Change_Rotate(GameObject target)
    {

        // 이 코드는 y 축 차이가 일어나면 x축 회전을 해버린다. 
        //transform.LookAt(target.transform, Vector3.up);

        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0; // Y축 회전을 무시합니다.

        // 방향이 0이 아니면 회전합니다.
        if (direction != Vector3.zero)
        {
            // 새로운 회전 값을 계산합니다.
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            // 현재 회전을 새로운 회전 값으로 설정합니다.
            transform.rotation = targetRotation;
        }

        yield return null;
    }
    #endregion

    #region Launch

    /// <summary>
    /// 공격을 맞게 되면 무기의 타입에 따라 밀리는 정도를 판별한다. 
    /// </summary>
    /// <param name="attacker"></param>
    /// <param name="causer"></param>
    /// <param name="data"></param>
    private bool CheckDoLauch(GameObject attacker, Weapon causer, ActionData data)
    {
        // 근접 무기가 아니라면 어느 거리든 상관 없이 해당 무기의 데이터로 처리한다. 
        Melee melee = causer as Melee;
        if (melee == null)
            return true;


        float distance = Vector3.Distance(attacker.transform.localPosition, transform.localPosition);

        // 공격자와의 거리가 밀리는 거리와의 차이가 크거나 같다면 밀리지 않는다.
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
