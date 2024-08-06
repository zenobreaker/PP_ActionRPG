using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ������ ������ �������� ���� �и��� ����� ���ִ� ������Ʈ
/// </summary>
public class LaunchComponent : MonoBehaviour
{

    private GroundedComponent ground;
    private OtherStateColliderComponent otherCollider;



    [SerializeField] private ForceMode forceMode;
    [Range(1.0f, 100.0f)][SerializeField] private float acceleration = 1.0f;
    [SerializeField] private float additionalAccel = 0.0f;
    [SerializeField] private float airMaintainTime = 0.0f;

    private new Rigidbody rigidbody;
    private NavMeshAgent agent; 

    private bool bAir = false;
    private bool bSuperArmor = false;
    public bool IsAir => bAir;

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


        ground = GetComponent<GroundedComponent>();
        Debug.Assert(ground != null);
        ground.OnChangedGorund += OnGround;

        otherCollider = GetComponent<OtherStateColliderComponent>();
        agent = GetComponent<NavMeshAgent>();

    }

    [SerializeField] private bool bDebuMode = false;
    private void Update()
    {
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
    }

    private void SetAirMode(bool bAir)
    {
        this.bAir = bAir;
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
        CharacterGrade grade = CharacterGrade.Common )
    {

        bool result = CheckAttackerAboutData(attacker, causer, data);
        if (result == false)
            return;

        if (grade == CharacterGrade.Boss)
            bSuperArmor = true;

        // ���� ���¿��� ���� ���� 
        DoHitInAir(attacker, causer, data);

        // ���� �ٶ� ��� �ٶ󺸱� 
        if (targetView)
            StartCoroutine(Change_Rotate(attacker));

        // ��ġ ����
        DoLaunch(attacker, causer, data);
    }

 


    #region Air_Condition
    // ���� ���¿� �¾Ҵٸ� ���õ� ������Ʈ�� �������� ���� �Ѵ�. 
    private void DoHitInAir(GameObject attacker, Weapon causer, DoActionData data)
    {
        bool result = CheckAttackerAboutData(attacker, causer, data);
        if (result == false)
            return;

        if (rigidbody == null)
            return;

        if (ground.IsGround == true)
            return;

        if (useGravityCoroutine != null)
            StopCoroutine(useGravityCoroutine);
        
        useGravityCoroutine = StartCoroutine(On_AirCombo(data));
    }

    
    private IEnumerator On_AirCombo(DoActionData data)
    {
        rigidbody.useGravity = false;
        rigidbody.velocity = Vector3.zero;

        //float distance = Mathf.Clamp(data.heightValue * 1.0f , 1.0f, data.heightValue);

        yield return new WaitForSecondsRealtime(airMaintainTime + data.airConditionTime);
        
        Debug.Log($"Change_UseGravity time is over- {true}");
        
        if(bAir)
            rigidbody.useGravity = true;

    }

    #endregion

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
        if (ground.IsGround)
            rigidbody.isKinematic = true;

        //Debug.Log($"Change_IsKinematics end  {rigidbody.isKinematic}");
        
        // ���߿� ���� �Լ�
        DoAir(data);
    }
    

    [SerializeField] ForceMode airforceMode = ForceMode.Force;
    [SerializeField] float airLauncherValue = 10.0f;
    private void DoLaunch(GameObject attacker, Weapon causer,
        DoActionData data)
    {
        bool bResult = true; 
        bResult &= CheckDoLauch(attacker, causer, data);

        float distanace = data.Distance;
        float launch = rigidbody.drag * distanace * 10.0f;
        
        Vector3 forceDir = attacker.transform.forward;
        var fm = ForceMode.Force;
        
        if (bAir)
        {
            rigidbody.mass = originMass * airLauncherValue;
            //distanace = distanace * ; 
             fm = airforceMode;
            if (data.bLauncher)
            {
                forceDir += Vector3.down * 0.5f;
                distanace = data.Distance;
                Debug.Log($"to launcher {rigidbody.mass * data.Distance * 10.0f} ");
            }

           float toDistance = Vector3.Distance(attacker.transform.localPosition, transform.localPosition);
            // ���� ���¿��� �����ڿ� ���� �Ÿ��� �����ٸ� �� �и��� 
            //if (toDistance <= 2.5f)
            //{
            //    Debug.Log($"too near");
            //    distanace *= 2.0f;
            //}

            launch = rigidbody.mass * distanace;
            Debug.Log($"air launcher => {launch}");
        }

        if (bResult)
        {
            rigidbody.isKinematic = false;
            rigidbody.AddForce(forceDir * launch, fm);
        }

        StartCoroutine(Change_IsKinematics(data, 5));
    }

    #endregion

    #region Airbone

    // ���߿� ���� �Լ�
    private void DoAir(DoActionData data)
    {
        if (bSuperArmor)
            return;

        // ���� �����̰� ������� �Դٸ� ������ ��Ʈ�� ���ߴٴ� �ǹ��̴�. 
        float value = data.heightValue;
        if (bAir)
        {
            if(data.heightValue == 0)
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

        if(agent != null)
            agent.enabled = false;

        yield return new WaitForFixedUpdate();

        rigidbody.constraints &= ~RigidbodyConstraints.FreezePositionY;
        rigidbody.drag = 0;
        rigidbody.isKinematic = false;
        //rigidbody.AddForce(Vector3.up *distance * acceleration, forceMode);
        rigidbody.AddForce(Vector3.up * acceleration, forceMode);

        ground.SetGroundCheck(false);
        SetAirMode(true);


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

    private void OnGround()
    {
        otherCollider?.SetAirStateCollider(false);
        SetAirMode(false);

        if (agent != null)
            agent.enabled = true;
    }

    #endregion
}
