using System;
using System.Collections;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;
using static StateComponent;
using static UnityEngine.EventSystems.StandaloneInputModule;

/// <summary>
/// ������Ʈ ��� ������Ʈ
/// ���� �Ÿ��� �ٴ��� �� ���� Ư�� �ִϸ��̼� ������ �ӵ��� �ٿ����� �����Ѵ�. 
/// </summary>

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(StateComponent))]
[RequireComponent(typeof(PlayerMovingComponent))]
public class DashComponent : MonoBehaviour
{

    [SerializeField] private float dashSpeed = 5.0f;
    //[SerializeField] private float originalAnimationSpeed = 1f;
    //[SerializeField] private float sprintMultiplier = 2f; // �� ���������� 2�ʷ� �þ�� ���� ����
    [SerializeField] private float dashDistance = 3.0f;
    //[SerializeField] private string sprintAnimName = ""; // ������Ʈ �ִϸ��̼� 

    private PlayerMovingComponent moving;
    private StateComponent state;

    private bool bTargetMode;

    private Vector3 targetPos;
    private float distance;


    private Animator animator;
    //private bool bSprint = false;

    public event Action OnBeginEvadeState;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        moving = GetComponent<PlayerMovingComponent>();
        Debug.Assert(moving != null);
        
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

        //TODO: �ִϸ��̼��� ����ϸ� ���� �ð��� ���ݾ� ������ �귯���� �غ���
        if (bBegin)
        { 
     
            Debug.Log($"���� �̸� :  {stateInfo.shortNameHash}");
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
        targetPos = transform.position + (direction.normalized * dashDistance);
        distance = Vector3.Distance(targetPos, transform.position);

        float resultTime = distance / dashSpeed;
        Debug.Log($"���� �ɸ��� �ð� {resultTime}");

        moving.Stop();
        //AdjustingAnimation(true);

        while (distance > 0)
        {
            transform.Translate(direction.normalized * dashSpeed * Time.fixedDeltaTime);
            distance = Vector3.Distance(targetPos, transform.position);

            yield return new WaitForFixedUpdate();
            float time = Time.time;
            //Debug.Log($"���� �ð�{time}");
            if (time - startTime >= resultTime)
                break;
        }
        
        Debug.Log("�뽬 ����");
        
        //AdjustingAnimation(false);
        moving.Move();
        //TODO: Test
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

                        // �밢�� ó�� 
                        if (value.x < 0.0f)
                        {
                            //evadeRotation = transform.rotation;
                            transform.Rotate(Vector3.up, -45.0f); // ����ϸ� ���������� 
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

                if(value.magnitude == 0.0f)
                    direction = EvadeDirection.Backward;

                //// ȸ�� ���� ����
                animator.SetInteger("Direction", (int)direction);
                animator.SetTrigger("Evade");

                Vector3 dir = Vector3.zero;
                if (direction == EvadeDirection.Forward)
                    dir = Vector3.forward;
                else if(direction == EvadeDirection.Backward)
                    dir = Vector3.back;

                DoAction_Dash(dir);

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
