using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ������ ������ �������� ���� �и��� ����� ���ִ� ������Ʈ
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
    private bool CheckDoLauch(GameObject attacker, Weapon causer, DoActionData data)
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


    private IEnumerator Change_IsKinematics(DoActionData data, int frame)
    {

        for (int i = 0; i < frame; i++)
            yield return new WaitForFixedUpdate();

        // ���� ��� ���� �ʴٸ� Ű�׸�ƽ���� �� �ʿ�� ���� 
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
        //    // ���� ���¿��� �����ڿ� ���� �Ÿ��� �����ٸ� �� �и��� 
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
