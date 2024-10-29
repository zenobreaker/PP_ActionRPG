using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using static Cinemachine.CinemachineTargetGroup;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// 카메라 암을 이용하여 3인칭 카메라로 보이게 한다.
/// </summary>
public class CameraArm : MonoBehaviour
{
    [SerializeField] private float cameraRotSpeed = 1.0f;

    [Header("Mouse")]
    [SerializeField] private Vector2 mouseSensitivity = new Vector2(0.5f, 0.5f);
    [SerializeField] private Vector2 limitPitchAngle = new Vector2(20, 340);
    [SerializeField] private float mouseRotationLerp = 0.25f;
    [SerializeField] private Vector3 offset = Vector3.zero;

    [Header("Zoom")]
    [SerializeField] private float zoomSensitivity = 0.001f;
    [SerializeField] private float zoomLerp = 1.0f;
    [SerializeField] private Vector2 zoomRange = new Vector2(1, 3);

    private float zoomDistance;
    [Header("Virtual Camera")]
    [SerializeField] private CinemachineVirtualCamera vcamera;
    [SerializeField] private CinemachineVirtualCamera rifleCamera;
    private Cinemachine3rdPersonFollow tpsFollowCamera;

    [SerializeField] private GameObject target;
    [SerializeField] private CutsenceController cutscene;

    private Quaternion rotation;

    // Targeting
    private bool bTargeting = false; 
    public bool Targeting { set => bTargeting = value; }
    private GameObject targetingObject; 
    public void SetTarget(GameObject target)
    {
        if (target != null)
        {
            bTargeting = true;
        }
        else
        {
            bTargeting = false;
        }

        targetingObject = target;
        
        if(vcamera != null)
        {
            vcamera.LookAt = targetingObject?.transform;
        }
    }

    private Vector2 inputLook;
    public bool bDebugMode = false; 
    private  bool visibleCursor;

    private void Awake()
    {
        cutscene = FindObjectOfType<CutsenceController>();
     

        // 마우스 커서를 숨깁니다.
        Cursor.visible = false;

        // 마우스를 화면 중앙에 고정하고 잠급니다.
        Cursor.lockState = CursorLockMode.Locked;


        if (target == null)
            target = FindObjectOfType<Player>().gameObject;


        Awake_BindPlayerInput();
    }
    private void Awake_BindPlayerInput()
    {
        PlayerInput input = FindObjectOfType<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");
        InputAction zoomAction = actionMap.FindAction("Zoom");
        zoomAction.performed += Input_Zoom_Performed;

    }

    private void Input_Zoom_Performed(InputAction.CallbackContext context)
    {
        float value = -context.ReadValue<float>() * zoomSensitivity;
        zoomDistance += value;
        zoomDistance = Mathf.Clamp(zoomDistance, zoomRange.x, zoomRange.y);
    }


    private void Start()
    {
        if (cutscene != null)
        {
            cutscene.OnCutsceneBegin += OnCameraAnim;
            cutscene.OnCutSceneEnd += OffCameraAnim;
        }

        vcamera = GetComponentInChildren<CinemachineVirtualCamera>();
        tpsFollowCamera = vcamera.GetCinemachineComponent<Cinemachine3rdPersonFollow>();

        if (tpsFollowCamera != null)
            zoomDistance = tpsFollowCamera.CameraDistance;
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Escape))
        //{
        //    visibleCursor = !visibleCursor;
        //}
        //Test();

        Update_ZoomCamera();
    }
    private void Update_ZoomCamera()
    {
        if (bDebugMode)
            return; 

        if (MathHelpers.IsNearlyEqual(tpsFollowCamera.CameraDistance, zoomDistance, 0.01f))
        {
            tpsFollowCamera.CameraDistance = zoomDistance;
            return;
        }

        tpsFollowCamera.CameraDistance = Mathf.SmoothStep(tpsFollowCamera.CameraDistance, zoomDistance, zoomLerp * Time.deltaTime);
    }


    public void SetInputMouse(Vector2 inputMouse)
    {
        inputLook = inputMouse;
    }

    private bool bAnim =false;
    public void OnCameraAnim()
    {
        bAnim = true; 
    }

    public void OffCameraAnim()
    {
        bAnim = false; 
    }


    private void LateUpdate()
    {
        if (target == null)
            return; 

        if (visibleCursor)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        if (bAnim)
            return;

        TargetMode();

        if (!bTargeting)
        {
            RotateCinemachine();
        }

        transform.position = target.transform.position + offset;
    }

    void TargetMode()
    {
        if (bTargeting == false)
            return; 

        // 타게팅 모드일 때 카메라 => 내 캐릭터 => 타겟 오브젝트 순으로 있어야 한다. 
        // 캐릭터와 타겟의 중간 위치
        Vector3 midPoint = (target.transform.position + targetingObject.transform.position) /2;

        // 카메라를 중간 지점에서 일정 거리 떨어진 위치로 설정
        Vector3 direction = (target.transform.position - targetingObject.transform.position).normalized;
        Vector3 cameraPosition = midPoint + direction  * zoomDistance + offset;
        // 카메라 위치 및 회전 설정 
        // 카메라 위치 설정 (혹은 필요에 따라 이동 보간 추가 가능)
        //this.transform.position = Vector3.Lerp(this.transform.position, cameraPosition, Time.deltaTime * moveSpeed); // moveSpeed를 원하는 속도로 설정

        // 회전 각 제한 
        float xAngle = vcamera.transform.eulerAngles.x;
        float anglesX = 0;

        if (xAngle < 180.0f && xAngle > limitPitchAngle.x)
            anglesX = limitPitchAngle.x;
        else if (xAngle > 180.0f && xAngle < limitPitchAngle.y)
            anglesX = limitPitchAngle.y;

        Vector3 currentAngles = vcamera.transform.eulerAngles;
        currentAngles.x = anglesX; // 제한된 x 축 회전 값으로 설정
        vcamera.transform.eulerAngles = currentAngles;

        // 부드러운 회전 적용
        Quaternion targetRotation = Quaternion.LookRotation(midPoint - this.transform.position);
        targetRotation.eulerAngles = new Vector3(0, targetRotation.eulerAngles.y, targetRotation.eulerAngles.z);
        this.transform.rotation = Quaternion.Slerp(this.transform.rotation, targetRotation, Time.deltaTime * 1.5f); // rotationLerpSpeed를 원하는 회전 속도로 설정
    }
   
    void RotateCinemachine()
    {
        // 1. 회전값 변수에 저장
        rotation *= Quaternion.AngleAxis(inputLook.x * mouseSensitivity.x * cameraRotSpeed, Vector3.up);
        rotation *= Quaternion.AngleAxis(-inputLook.y * mouseSensitivity.y * cameraRotSpeed, Vector3.right);
        this.transform.rotation = rotation; // 회전 값 바로 적용 

        // 2. 회전 축 제한하기 
        Vector3 angles = rotation.eulerAngles;
        angles.z = 0;
        
        // 회전 각 제한 
        float xAngle = this.transform.eulerAngles.x;

        if (xAngle < 180.0f && xAngle > limitPitchAngle.x)
            angles.x = limitPitchAngle.x;
        else if (xAngle > 180.0f && xAngle < limitPitchAngle.y)
            angles.x = limitPitchAngle.y;

        // 3. 회전량 보정 
        rotation = Quaternion.Lerp(this.transform.rotation, rotation, mouseRotationLerp * Time.deltaTime);
        rotation.eulerAngles = new Vector3(angles.x, rotation.eulerAngles.y, 0);
        // 회전량 최종 삽입 
        this.transform.rotation = rotation;
    }

    float rotX;
    float rotY;
    void CameraRotation()
    {
        Vector2 mouseAxis = new Vector2(inputLook.x, inputLook.y);
        rotX += (mouseAxis.x * 1.0f) * Time.deltaTime;
        rotY -= (mouseAxis.y * 1.0f) * Time.deltaTime;

        rotY = Mathf.Clamp01(rotY);

        Quaternion localRotation = Quaternion.Euler(rotX, rotY, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, localRotation, Time.deltaTime);
    }

    public Vector3 GetRifleForward()
    {
        return rifleCamera.transform.forward;
    }

    public GameObject GetRifleCameraObj()
    {
        return rifleCamera.transform.gameObject;
    }

    public void TurnOnCamera()
    {
        if (vcamera == null)
            return;
        vcamera.gameObject.SetActive(true);
    }

    public void TurnOffCamera()
    {
        if (vcamera == null) 
            return;

        vcamera.gameObject.SetActive(false);
    }

    public void SetSnipeMode(GameObject snipeObj)
    {
        if (rifleCamera == null)
            return;

        rifleCamera.gameObject.SetActive(true);
        rifleCamera.transform.position = snipeObj.transform.position;
        //rifleCamera.transform.rotation = snipeObj.transform.rotation;

        TurnOffCamera();
    }

    public void EndSnipeMode()
    {
        TurnOnCamera();

        if (rifleCamera == null)
            return;

        rifleCamera.gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        //GUILayout.Label(transform.rotation.ToString());
    }


#endif

}
