using System;
using System.Collections;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// 스프린트 기능 컴포넌트
/// 일정 거리에 다달을 때 까지 특정 애니메이션 구간을 속도를 줄여놔서 보간한다. 
/// </summary>

[RequireComponent(typeof(Animator))]
public class SprintComponent : MonoBehaviour
{

    [SerializeField] private float sprintSpeed = 5.0f;
    //[SerializeField] private float originalAnimationSpeed = 1f;
    //[SerializeField] private float sprintMultiplier = 2f; // 이 예제에서는 2초로 늘어났을 때를 가정
    [SerializeField] private float sprintDistance = 3.0f;
    [SerializeField] private string sprintAnimName = ""; // 스프린트 애니메이션 

    private Vector3 direction;

    private Animator animator;
    private CharacterController contoller;
    private bool bSprint = false;

    public event Action OnBeginSprint;
    public event Action OnEndSprint;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetDirection(Vector3 direction)
    {
        this.direction = direction;
    }

    private Quaternion? evadeRotation = null;
    public void Execute_SprintAnimation_Old(Vector2 moveValue)
    {
        bool bCheck = false;
        bCheck |= (bSprint ==true);
        
        if (bCheck)
            return; 

        Vector2 value = moveValue;

        EvadeDirection direction = EvadeDirection.Forward;
        if (value.y == 0.0f)
        {
            direction = EvadeDirection.Forward;

            if (value.x < 0.0f)
                direction = EvadeDirection.Left;
            else if (value.x > 0.0f)
                direction = EvadeDirection.Right;
        }
        else if (value.y >= 0.0f)
        {
            direction = EvadeDirection.Forward;

            // 대각선 처리 
            if (value.x < 0.0f)
            {
                evadeRotation = transform.rotation;
                transform.Rotate(Vector3.up, -45.0f); // 어색하면 보간해주자 
            }
            else if (value.x > 0.0f)
            {
                evadeRotation = transform.rotation;
                transform.Rotate(Vector3.up, 45.0f);
            }
        }
        else
        {
            direction = EvadeDirection.Backward;
        }

        this.direction = new Vector3(moveValue.x, 0, moveValue.y);
        this.direction.Normalize();

        // 회피 동작 실행
        animator.SetInteger("Direction", (int)direction);
        animator.SetTrigger("Evade");

       // Begin_Sprint();
    }



    public void Begin_Sprint()
    {
        StopAllCoroutines();
        StartCoroutine(Start_Sprint());
    }

    float distance;
    Vector3 targetPos;
    bool bStart = false; 
    private IEnumerator Start_Sprint()
    {
        float startTime = Time.time; // need to remember this to know how long to dash
        bStart = true; 
        targetPos = transform.position + (direction * sprintDistance);
        distance = Vector3.Distance(targetPos, transform.position);

        OnBeginSprint?.Invoke();

        while (distance > 0.2f)
        {

            direction = (targetPos - transform.position).normalized;
            distance = Vector3.Distance(targetPos, transform.position);

            contoller.Move(direction * sprintSpeed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }

        bStart = false;
        contoller.Move(Vector3.zero);

        OnEndSprint?.Invoke();
    }

#if UNITY_EDITOR

    private void OnGUI()
    {
        if (Selection.activeGameObject != gameObject)
            return; 

        Gizmos.color = Color.red;
        GUILayout.Label(distance.ToString("f6"));
    }


    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false)
            return;

        if (bStart)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPos, 2.0f);
        }

    }

#endif


}
