using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 공격을 맞으면 경직으로 인해 밀리는 기능을 해주는 컴포넌트
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

        // 공중 상태에서 맞은 관련 
        DoHitInAir(attacker, causer, data);

        // 나를 바라본 대상 바라보기 
        if (targetView)
            StartCoroutine(Change_Rotate(attacker));

        // 런치 실행
        DoLaunch(attacker, causer, data);
    }

 


    #region Air_Condition
    // 공중 상태에 맞았다면 관련된 컴포넌트나 변수들을 관리 한다. 
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
        if (ground.IsGround)
            rigidbody.isKinematic = true;

        //Debug.Log($"Change_IsKinematics end  {rigidbody.isKinematic}");
        
        // 공중에 띄우는 함수
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
            // 공중 상태에서 공격자와 나의 거리가 가깝다면 더 밀리게 
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

    // 공중에 띄우는 함수
    private void DoAir(DoActionData data)
    {
        if (bSuperArmor)
            return;

        // 공중 상태이고 여기까지 왔다면 이전에 히트를 당했다는 의미이다. 
        float value = data.heightValue;
        if (bAir)
        {
            if(data.heightValue == 0)
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

    private void OnGround()
    {
        otherCollider?.SetAirStateCollider(false);
        SetAirMode(false);

        if (agent != null)
            agent.enabled = true;
    }

    #endregion
}
