using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem;
using static StateComponent;

public enum EvadeDirection
{
    Forward = 0, Backward, Left, Right,
}

public class PlayerMovingComponent : MonoBehaviour
{
    /// <summary>
    /// Input Keyboard
    /// </summary>
    [Header("Speed")]
    [SerializeField] private float walkSpeed = 2.0f;
    [SerializeField] private float runSpeed = 4.0f;
    [SerializeField] private float sensitivity = 10.0f;
    [SerializeField] private float deadZone = 0.001f;

    /// <summary>
    /// Input Mouse 
    /// </summary>
    [Header("Mouse")]
    [SerializeField] private string followTargetName = "FollowTarget";
    [SerializeField] private Vector2 mouseSensitivity = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 limitPitchAngle = new Vector2(20, 340);
    [SerializeField] private float mouseRotationLerp = 0.25f;

    private SlopeMovement slopeMovement;


    private float walkToRunRatio = 2.0f;
    private bool bCanMove = true;
    //private bool bTargetMode = false; 

    private Animator animator;
    private CameraArm cameraArm;
    private WeaponComponent weapon;
    private StateComponent state;
    private ConditionComponent condition;

    private Vector3 inputMove;
    public Vector2 InputMove { get => inputMove; }

    private Vector2 currentInputMove; // 현재 입력한 이동값 
    private bool bRun;

    private Vector2 inputLook; // 마우스의 델타값 
    public void Move()
    {
        bCanMove = true;
    }

    public void Stop()
    {
        bCanMove = false;
    }

    private Transform followTargetTransform;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        weapon = GetComponent<WeaponComponent>();
        state = GetComponent<StateComponent>();
        condition = GetComponent<ConditionComponent>();
        slopeMovement = GetComponent<SlopeMovement>();
        Debug.Assert(slopeMovement != null);
        cameraArm = FindObjectOfType<CameraArm>();
        Debug.Assert(cameraArm != null);

        // 마우스 커서를 숨깁니다.
        Cursor.visible = false;

        // 마우스를 화면 중앙에 고정하고 잠급니다.
        Cursor.lockState = CursorLockMode.Locked;

        Awake_BindPlayerInput();
    }

    private void Awake_BindPlayerInput()
    {
        PlayerInput input = GetComponent<PlayerInput>();
        // Q. 어디서 찾게?~ A. 인풋시스템 액션 안에 플레이어 액션즈 시키야 
        InputActionMap actionMap = input.actions.FindActionMap("Player");
        // 인풋 시스템에 설정한 이름으로 액션을 가져온다
        InputAction moveAction = actionMap.FindAction("Move");
        moveAction.performed += Input_Move_Performed;   // 이벤트 연결
        moveAction.canceled += Input_Move_Canceled;

        InputAction lookAction = actionMap.FindAction("Look");
        lookAction.performed += Input_Look_Performed;
        lookAction.canceled += Input_Look_Canceled;

        InputAction runAction = actionMap.FindAction("Run");
        runAction.started += Input_Run_Started;
        runAction.performed += Input_Run_Performed;
        runAction.canceled += Input_Run_Canceled;

    }

    private void Start()
    {
        followTargetTransform = transform.FindChildByName(followTargetName);
    }

    #region Keyboard

    private void Input_Move_Performed(InputAction.CallbackContext context)
    {
        inputMove = context.ReadValue<Vector2>();
    }

    private void Input_Move_Canceled(InputAction.CallbackContext context)
    {
        inputMove = Vector3.zero;
    }
    #endregion


    #region Mouse_Move
    private void Input_Look_Performed(InputAction.CallbackContext context)
    {
        inputLook = context.ReadValue<Vector2>();
        cameraArm?.SetInputMouse(inputLook);

    }

    private void Input_Look_Canceled(InputAction.CallbackContext context)
    {
        inputLook = Vector3.zero;
        cameraArm?.SetInputMouse(inputLook);
    }
    #endregion


    #region Run
    private void Input_Run_Started(InputAction.CallbackContext context)
    {
        //bRun = true;
    }

    private void Input_Run_Performed(InputAction.CallbackContext context)
    {
        bRun = true;
    }

    private void Input_Run_Canceled(InputAction.CallbackContext context)
    {
        bRun = false;
    }
    #endregion
    private Vector2 velocity;
    private Quaternion rotation;
    public Quaternion Rotation { set => rotation = value; }

    private void Update()
    {
        currentInputMove = Vector2.SmoothDamp(currentInputMove, inputMove, ref velocity, 1.0f / sensitivity);

        if (condition.NoneCondition == false)
            return;

        if (bCanMove == false)
            return;

        //Update_PlayerRotate(inputMove.y);

        float speed = bRun ? runSpeed : walkSpeed;
        Vector3 direction = Vector3.zero;
        if (currentInputMove.magnitude > deadZone)
        {

            //direction = (Vector3.right * currentInputMove.x) + (Vector3.forward * currentInputMove.y);
            //direction = (transform.right * currentInputMove.x) + (transform.forward * currentInputMove.y);
            //TODO: 카메라 바뀌면 여기 수정 
            direction = (cameraArm.transform.right * currentInputMove.x) + (cameraArm.transform.forward * currentInputMove.y);

            direction.y = 0;
            transform.localRotation = Quaternion.LookRotation(direction);
        }

        // 경사면일 경우 
        if (slopeMovement?.OnSlope() == true)
        {
            // 경사면에서 투영된 결과로 방향을 보정
            direction = slopeMovement.AdjustDirecionToSlope(direction);
            Debug.Log("경사면 처리 중");
        }

        direction = direction.normalized * speed;
        transform.Translate(direction * Time.deltaTime, Space.World);

        float deltaSpeed = direction.magnitude / walkSpeed * walkToRunRatio;
        if (weapon.UnarmedMode)
        {
            animator.SetFloat("SpeedY", deltaSpeed);
            return;
        }

        //if(bTargetMode)
        //animator.SetFloat("SpeedX", currentInputMove.x * deltaSpeed);
        //animator.SetFloat("SpeedY", currentInputMove.y * deltaSpeed);
        animator.SetFloat("SpeedY", deltaSpeed);

    }

    private void Update_PlayerRotate(float forwardValue)
    {
        // 카메라 회전

        rotation *= Quaternion.AngleAxis(inputLook.x * mouseSensitivity.x, Vector3.up);
        rotation *= Quaternion.AngleAxis(-inputLook.y * mouseSensitivity.y, Vector3.right);
        followTargetTransform.rotation = rotation;

        // 회전 축 제한 
        Vector3 angles = followTargetTransform.localEulerAngles;
        angles.z = 0.0f;

        // 회전각 제한
        float xAngle = followTargetTransform.localEulerAngles.x;

        if (xAngle < 180.0f && xAngle > limitPitchAngle.x)
            angles.x = limitPitchAngle.x;
        else if (xAngle > 180.0f && xAngle < limitPitchAngle.y)
            angles.x = limitPitchAngle.y;

        followTargetTransform.localEulerAngles = angles;

        rotation = Quaternion.Lerp(followTargetTransform.rotation, rotation, mouseRotationLerp * Time.deltaTime);

        // 먼저 플레이어를 회전 
        transform.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        // 자식 놈 회전하는데 이미 위에서 y 햇으니까 .. ? -> 짐벌락 방지 이미 돌았으니
        followTargetTransform.localEulerAngles = new Vector3(angles.x, 0, 0);

    }

}
