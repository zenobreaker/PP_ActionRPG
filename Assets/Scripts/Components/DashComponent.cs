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
    //[SerializeField] private string sprintAnimName = ""; // 스프린트 애니메이션 

    private PlayerMovingComponent moving;
    private ConditionComponent condition;
    private StateComponent state;
    private SlopeMovement slopeMovement;

    private bool bTargetMode;

    private Vector3 targetPos;


    private Animator animator;
    //private bool bSprint = false;

    public event Action OnBeginEvadeState;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        moving = GetComponent<PlayerMovingComponent>();
        Debug.Assert(moving != null);

        condition = GetComponent<ConditionComponent>();
        state = GetComponent<StateComponent>();
        Debug.Assert(state != null);
        state.OnStateTypeChanged += OnStateTypeChanged;

        slopeMovement = GetComponent<SlopeMovement>();
        Debug.Assert(slopeMovement != null);

    }

    public void DoAction_Dash(EvadeDirection direction)
    {
        if (condition != null && condition.DownCondition)
            return;

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



    private Vector3 GetDashDirection(EvadeDirection dir)
    {
        if (dir == EvadeDirection.Forward)
            return Vector3.forward;
        else if (dir == EvadeDirection.Backward)
            return Vector3.back;

        return Vector3.back;
    }
    
    private IEnumerator Start_Dash(EvadeDirection evadeDir)
    {
        // 1. 로컬 방향 계산 
        Vector3 direction = GetDashDirection(evadeDir);
        Vector3 dashDirection = transform.TransformDirection(direction);

        // 2. 경사면이 조정이 필요하면 적용
        if (slopeMovement?.OnSlope() == true)
        {
            dashDirection = slopeMovement.AdjustDirecionToSlope(dashDirection);
        }

        // 3. 목표 위치설정
        targetPos = transform.position + (dashDirection.normalized * dashDistance);
        Vector3 finalDir = (targetPos - transform.position).normalized;
        
        float distance = dashDistance;

        float startTime = Time.time;
        float resultTime = distance / dashSpeed;

        // 사운드 재생 및 시작 
        moving.Stop();
        SoundManager.Instance.PlaySFX("Dash_Sound");

        // 4. 대시 이동 로직
        while (distance > 0)
        {
            // 로컬 좌표계 이동 -> 이동 시 월드 좌표계를 유지하여 원하는 방향으로 대시할 수 있게 보장.
            transform.Translate(finalDir.normalized * dashSpeed * Time.fixedDeltaTime, Space.World);
            
            // 남은 거리 계산
            distance = Vector3.Distance(targetPos, transform.position);

            yield return new WaitForFixedUpdate();
            
            // 시간 초과 체크
            float time = Time.time;
            if (time - startTime >= resultTime)
                break;
        }

        moving.Move();
        state.SetIdleMode();
    }

    //private Quaternion? evadeRotation = null;
    private void OnStateTypeChanged(StateType prevType, StateType newType)
    {
        switch (newType)
        {
            case StateType.Evade:
            {

                OnBeginEvadeState?.Invoke();

                Vector2 value = moving.InputMove;

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

                if (value.magnitude == 0.0f)
                    direction = EvadeDirection.Backward;

                //// 회피 동작 실행
                animator.SetInteger("Direction", (int)direction);
                animator.SetTrigger("Evade");

                DoAction_Dash(direction);

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
        //GUILayout.Label(distance.ToString("f6"));
    }


    private void OnDrawGizmos()
    {
        if (Application.isPlaying == false)
            return;

        Gizmos.color = Color.green;

        Gizmos.DrawLine(targetPos, targetPos + Vector3.up * 5.0f);

        // 대쉬 방향
        {
            //Gizmos.color = Color.red;
            //Vector3 from = transform.position;
            //Vector3 to = transform.forward * dashDistance + Vector3.up * 1.0f;
            //Gizmos.DrawLine(from, to);
        }
    }

#endif


}
