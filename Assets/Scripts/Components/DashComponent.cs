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

    [SerializeField] private float sprintSpeed = 5.0f;
    //[SerializeField] private float originalAnimationSpeed = 1f;
    //[SerializeField] private float sprintMultiplier = 2f; // �� ���������� 2�ʷ� �þ�� ���� ����
    [SerializeField] private float sprintDistance = 3.0f;
    [SerializeField] private string sprintAnimName = ""; // ������Ʈ �ִϸ��̼� 

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


                //// ȸ�� ���� ����
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
