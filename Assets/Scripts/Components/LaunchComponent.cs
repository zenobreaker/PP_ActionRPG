using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 공격을 맞으면 경직으로 인해 밀리는 기능을 해주는 컴포넌트
/// </summary>
public class LaunchComponent : MonoBehaviour
{
    private StateComponent state;
    private OtherStateColliderComponent otherCollider;

    private new Rigidbody rigidbody;
    private NavMeshAgent agent;

    private bool bSuperArmor = false;

    private float originDrag;
    private float originMass;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        Debug.Assert(rigidbody != null);
        originDrag = rigidbody.drag;
        originMass = rigidbody.mass;


        state = GetComponent<StateComponent>();
        otherCollider = GetComponent<OtherStateColliderComponent>();
        agent = GetComponent<NavMeshAgent>();

    }

    private bool CheckAttackerAboutData(GameObject attacker, Weapon causer, DoActionData data)
    {
        bool bCheck = true;
        bCheck &= (attacker != null);
        bCheck &= (causer != null);
        bCheck &= (data != null);

        return bCheck;
    }



    public void DoHit(GameObject attacker, Weapon causer, DoActionData data, bool targetView = false,
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
    private bool CheckDoLauch(GameObject attacker, Weapon causer, DoActionData data)
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


    private IEnumerator Change_IsKinematics(DoActionData data, int frame)
    {

        for (int i = 0; i < frame; i++)
            yield return new WaitForFixedUpdate();

        // 땅에 닿아 있지 않다면 키네메틱스를 끌 필요는 없다 
        if(state.AirborneMode == false)
            rigidbody.isKinematic = true;
    }

    private void DoLaunch(GameObject attacker, Weapon causer,
        DoActionData data)
    {
        bool bResult = true;
        bResult &= CheckDoLauch(attacker, causer, data);

        float distanace = data.Distance;
        float launch = rigidbody.drag * distanace * 10.0f;

        Vector3 forceDir = attacker.transform.forward;
        var fm = ForceMode.Force;

        //if (bAir)
        //{
        //    rigidbody.mass = originMass * airLauncherValue;
        //    //distanace = distanace * ; 
        //    fm = airforceMode;
        //    if (data.bLauncher)
        //    {
        //        forceDir += Vector3.down * 0.5f;
        //        distanace = data.Distance;
        //        Debug.Log($"to launcher {rigidbody.mass * data.Distance * 10.0f} ");
        //    }

        //    float toDistance = Vector3.Distance(attacker.transform.localPosition, transform.localPosition);
        //    // 공중 상태에서 공격자와 나의 거리가 가깝다면 더 밀리게 
        //    //if (toDistance <= 2.5f)
        //    //{
        //    //    Debug.Log($"too near");
        //    //    distanace *= 2.0f;
        //    //}

        //    launch = rigidbody.mass * distanace;
        //    Debug.Log($"air launcher => {launch}");
        //}

        if (bResult)
        {
            rigidbody.isKinematic = false;
            rigidbody.AddForce(forceDir * launch, fm);
        }

        StartCoroutine(Change_IsKinematics(data, 5));
    }

    #endregion

  
}
