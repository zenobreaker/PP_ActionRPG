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

    [SerializeField] private float sprintSpeed = 5.0f;
    //[SerializeField] private float originalAnimationSpeed = 1f;
    //[SerializeField] private float sprintMultiplier = 2f; // 이 예제에서는 2초로 늘어났을 때를 가정
    [SerializeField] private float sprintDistance = 3.0f;
    [SerializeField] private string sprintAnimName = ""; // 스프린트 애니메이션 

    private PlayerMovingComponent moving; 
    private StateComponent state;

    private Vector2 inputMove;
    private bool bTargetMode; 

    private Vector3 direction;
    private Vector3 targetPos;
    private float distance;
    private bool bStart = false;


    private Animator animator;
    private CharacterController contoller;
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

    public void DoAction_Dash()
    {
        if (state.IdleMode == false)
            return; 

        StopAllCoroutines();
        StartCoroutine(Start_Sprint());
    }


    private IEnumerator Start_Sprint()
    {
        float startTime = Time.time; 
        bStart = true; 
        targetPos = transform.position + (direction * sprintDistance);
        distance = Vector3.Distance(targetPos, transform.position);


        while (distance > 0.2f)
        {

            direction = (targetPos - transform.position).normalized;
            distance = Vector3.Distance(targetPos, transform.position);

            contoller.Move(direction * sprintSpeed * Time.deltaTime);
            yield return new WaitForFixedUpdate();
        }

        bStart = false;
        contoller.Move(Vector3.zero);

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
                animator.SetInteger("Direction", (int)direction);
                animator.SetTrigger("Evade");

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

        if (bStart)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetPos, 2.0f);
        }

    }

#endif


}
