using System;
using System.Collections;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
using static StateComponent;
using static UnityEngine.EventSystems.StandaloneInputModule;

/// <summary>
/// 스프린트 기능 컴포넌트
/// 일정 거리에 다달을 때 까지 특정 애니메이션 구간을 속도를 줄여놔서 보간한다. 
/// </summary>

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(StateComponent))]
[RequireComponent(typeof(PlayerMovingComponent))]
public class DashComponent : MonoBehaviour
{

    [SerializeField] private float dashSpeed = 5.0f;
    //[SerializeField] private float originalAnimationSpeed = 1f;
    //[SerializeField] private float sprintMultiplier = 2f; // 이 예제에서는 2초로 늘어났을 때를 가정
    [SerializeField] private float dashDistance = 3.0f;
    [SerializeField] private string sprintAnimName = ""; // 스프린트 애니메이션 

    private PlayerMovingComponent moving;
    private StateComponent state;

    private Vector2 inputMove;
    private bool bTargetMode;

    private Vector3 targetPos;
    private float distance;
    private bool bDash = false;


    private Animator animator;
    //private bool bSprint = false;

    public event Action OnBeginEvadeState;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        moving = GetComponent<PlayerMovingComponent>();
        Debug.Assert(moving != null);
        inputMove = moving.InputMove;

        state = GetComponent<StateComponent>();
        Debug.Assert(state != null);
        state.OnStateTypeChanged += OnStateTypeChanged;

    }

    public void DoAction_Dash(Vector3 direction)
    {
        StopAllCoroutines();
        StartCoroutine(Start_Dash(direction));
    }


    private void AdjustingAnimation(bool bBegin)
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        //TODO: 애니메이션이 어색하면 구간 시간을 조금씩 느리게 흘러가게 해볼까
        if (bBegin)
        { 
     
            Debug.Log($"현재 이름 :  {stateInfo.shortNameHash}");
            if (stateInfo.normalizedTime >= 0.5f && stateInfo.normalizedTime <= 0.6f)
            {
                animator.speed = 0.0f;
                return;
            }
        }

        animator.speed = 1.0f;
    }




    private IEnumerator Start_Dash(Vector3 direction)
    {
        float startTime = Time.time;
        bDash = true;
        targetPos = transform.position + (direction.normalized * dashDistance);
        distance = Vector3.Distance(targetPos, transform.position);

        float resultTime = distance / dashSpeed;
        Debug.Log($"최종 걸리는 시간 {resultTime}");

        moving.Stop();
        //AdjustingAnimation(true);

        while (distance > 0)
        {
            transform.Translate(direction.normalized * dashSpeed * Time.fixedDeltaTime);
            distance = Vector3.Distance(targetPos, transform.position);

            yield return new WaitForFixedUpdate();
            float time = Time.time;
            //Debug.Log($"측정 시간{time}");
            if (time - startTime >= resultTime)
                break;
        }
        
        Debug.Log("대쉬 종료");
        
        //AdjustingAnimation(false);
        bDash = false;
        moving.Move();
        //TODO: Test
        state.SetIdleMode();
    }

    private Quaternion? evadeRotation = null;
    private void OnStateTypeChanged(StateType prevType, StateType newType)
    {
        switch (newType)
        {
            case StateType.Evade:
            {

                OnBeginEvadeState?.Invoke();

                Vector2 value = inputMove;

                EvadeDirection direction = EvadeDirection.Forward;
                if (bTargetMode)
                {
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
                            //evadeRotation = transform.rotation;
                            transform.Rotate(Vector3.up, -45.0f); // 어색하면 보간해주자 
                        }
                        else if (value.x > 0.0f)
                        {
                            //evadeRotation = transform.rotation;
                            transform.Rotate(Vector3.up, 45.0f);
                        }
                    }
                    else
                    {
                        direction = EvadeDirection.Backward;
                    }
                }


                //// 회피 동작 실행
                animator.SetInteger("Direction", (int)0);
                animator.SetTrigger("Evade");

                if (direction == EvadeDirection.Forward)
                    DoAction_Dash(Vector3.forward);

            }
            return;
        }
    }

    private void End_Evade()
    {
        state.SetIdleMode();
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


    }

#endif


}
