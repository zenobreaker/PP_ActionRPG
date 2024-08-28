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
    //[SerializeField] private new Camera camera;

    //[SerializeField] private float cameraDistance = 1.0f;
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
    [SerializeField] private CinemachineVirtualCamera vcamera;
    [SerializeField] private CinemachineVirtualCamera rifleCamera;
    private Cinemachine3rdPersonFollow tpsFollowCamera;

    [SerializeField] private GameObject target;
    [SerializeField] private CutsenceController cutscene;

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

        RotateCinemachine();
            
        transform.position = target.transform.position + offset;
    }


    private void OriginRotate()
    {
        //Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"),
        //    Input.GetAxis("Mouse Y")) * cameraRotSpeed;
        //Vector3 camAngle = this.transform.rotation.eulerAngles;
        //float x = camAngle.x - mouseDelta.y;

        ////target.transform.root.rotation = Quaternion.Euler(0, camAngle.y + mouseDelta.x, 0);
        //if (x < 180f)
        //{
        //    x = Mathf.Clamp(x, -1f, 70f);
        //}
        //else
        //{
        //    x = Mathf.Clamp(x, 325f, 361f);
        //}


        //transform.rotation = Quaternion.Euler(x, camAngle.y + mouseDelta.x, camAngle.z);
    }

    private Quaternion rotation;
    void RotateCinemachine()
    {
        rotation *= Quaternion.AngleAxis(inputLook.x * mouseSensitivity.x * cameraRotSpeed, Vector3.up);
        rotation *= Quaternion.AngleAxis(-inputLook.y * mouseSensitivity.y * cameraRotSpeed, Vector3.right);
        this.transform.rotation = rotation;

        //회전 축 제한하기 
        Vector3 angles = rotation.eulerAngles;
        angles.z = 0;
        
        // 회전 각 제한 
        float xAngle = this.transform.eulerAngles.x;

        if (xAngle < 180.0f && xAngle > limitPitchAngle.x)
            angles.x = limitPitchAngle.x;
        else if (xAngle > 180.0f && xAngle < limitPitchAngle.y)
            angles.x = limitPitchAngle.y;

        //  회전량 보정 
        rotation = Quaternion.Lerp(this.transform.rotation, rotation, mouseRotationLerp * Time.deltaTime);
        rotation.eulerAngles = new Vector3(angles.x, rotation.eulerAngles.y, 0);
        // 회전량 최종 삽입 
        this.transform.rotation = rotation;
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
