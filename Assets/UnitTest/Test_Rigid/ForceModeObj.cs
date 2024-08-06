using UnityEngine;

public class ForceModeObj : MonoBehaviour
{

    [SerializeField] private Rigidbody rigid;

    public ForceMode forceMode;


    public AnimationCurve forceCurve; // 애니메이션 커브를 통해 힘을 조절
    public float maxForce;
    public float duration; // 힘을 적용할 총 시간

    private float elapsedTime = 0f;
    private bool isLaunching = false;

    public float targetHeight;
    public float launchSpeed;
    private bool isLaunching2 = false;

    private void Start()
    {
        rigid = GetComponent<Rigidbody>(); 

        Debug.Assert(rigid != null);    
    }

    private void Update()
    {
        if (isLaunching)
        {
            ApplyForce();
        }

        if(isLaunching2)
        {
            MoveTowardsTargetHeight();
        }
    }


    public void RestRigid()
    {
        rigid.velocity = Vector3.zero;  
    }

    public void DoAddForce(float accel = 1.0f)
    {
        Debug.Log($"{gameObject.name} AddForce");
        rigid.AddForce(Vector3.right* accel, forceMode);
    }
    public void DoAddForceWithMass(float aceel = 1.0f)
    {
        float forceValue = rigid.mass * aceel;

        Debug.Log($"{gameObject.name} DoAddForceMass");
        rigid.AddForce(Vector3.right * forceValue, forceMode);
    }

    public void DoUpAddForce(float accel = 1.0f)
    {
        Debug.Log($"{gameObject.name} DoUpAddForce");
        rigid.AddForce(Vector3.up * accel, forceMode);
    }

    public void DoUpAddForceWithMass(float accel = 1.0f)
    {
        float forceValue = rigid.mass * accel;
        Debug.Log($"{gameObject.name} DoUpAddForceMass");
        rigid.AddForce(Vector3.up * forceValue, forceMode);
    }

    public void LaunchObject()
    {
        elapsedTime = 0f;
        isLaunching = true;
    }

    // 탈락 애니메이션 제어하는게 번거롭다 
    public void ApplyForce()
    {
        if (elapsedTime < duration)
        {
            float forceMultiplier = forceCurve.Evaluate(elapsedTime / duration);
            rigid.AddForce(Vector3.up * maxForce * forceMultiplier, forceMode);
            elapsedTime += Time.deltaTime;
        }
        else
        {
            rigid.velocity = Vector3.zero;
            isLaunching = false;
        }
    }

    public void Launch2Object()
    {
        isLaunching2 = true;
    }

    private void MoveTowardsTargetHeight()
    {
        float step = launchSpeed * Time.deltaTime;

        // 현재 높이와 목표 높이 사이의 거리 계산
        float distanceToTarget = targetHeight - rigid.position.y;

        // 목표 높이보다 아래에 있으면 위로 이동
        if (distanceToTarget > 0)
        {
            rigid.velocity = new Vector3(rigid.velocity.x, step, rigid.velocity.z);
        }
        else
        {
            isLaunching2 = false;
            rigid.velocity = new Vector3(rigid.velocity.x, 0, rigid.velocity.z);
        }
    }



}
