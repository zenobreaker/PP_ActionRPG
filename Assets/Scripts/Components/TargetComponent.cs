using Cinemachine.Utility;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;

public class TargetComponent : MonoBehaviour
{
    [SerializeField] private float radius = 1f;
    [SerializeField] private LayerMask layerMask;
    //[SerializeField] private float rotateSpeed = 1.0f; // 타게팅 대상까지 회전할 속도 

    [SerializeField] private GameObject targetObject;
    [SerializeField] private bool bMovingFocus;

    [Header("Targeting Settings")]
    [SerializeField] private string lockOnUI = "LockOnUI";
    [SerializeField]
    [Range(0, 1.0f)]
    private float lockOnUIScale = 0.1f;
    private CameraArm cameraArm;

    private void Awake()
    {
        PlayerInput input = GetComponent<PlayerInput>();
        Debug.Assert(input != null);

        cameraArm = FindObjectOfType<CameraArm>();
        Debug.Assert(cameraArm != null);

        InputActionMap actionMap = input.actions.FindActionMap("Player");
        Debug.Assert(actionMap != null);

        InputAction targetingAction = actionMap.FindAction("Targeting");
        targetingAction.performed += Input_Targeting_Performed;

        InputAction targeting_Right_Action = actionMap.FindAction("Targeting_Right");
        targeting_Right_Action.performed += Input_Targeting_Right_Performed;


        InputAction targeting_Left_Action = actionMap.FindAction("Targeting_Left");
        targeting_Left_Action.performed += Input_Targeting_Left_Performed;
    }


    private void Input_Targeting_Performed(InputAction.CallbackContext context)
    {
        Begin_Targeting();
    }

    private void Input_Targeting_Right_Performed(InputAction.CallbackContext context)
    {
        ChangeFocucs(true);
    }

    private void Input_Targeting_Left_Performed(InputAction.CallbackContext context)
    {
        ChangeFocucs(false);
    }


    public void Begin_Targeting(bool bRotation = false)
    {
        if(targetObject != null)
        {
            EndTargeting();

            return; 
        }

        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, layerMask.value);

        GameObject[] candinates = colliders.Select(colliders => colliders.gameObject).ToArray();

        // 가장 가까운 대상 
        GameObject nearlyObject = GetNearlyFrontAngle(candinates);

        ChangeTarget(nearlyObject);

        // 대상을 향해 회전
        if (bRotation == false)
            return;

        RotateToTarget();
    }

    private GameObject GetNearlyFrontAngle(GameObject[] candinates)
    {
        Vector3 position = transform.position;

        GameObject candinate = null;
        float minAngle = float.MinValue;
        float minDistance = float.MaxValue;

        foreach (GameObject obj in candinates)
        {
            Vector3 enemyPosition = obj.transform.position;
            Vector3 direction = enemyPosition - position;
            float distance = Vector3.Distance(enemyPosition, position);

            float angle = Vector3.Dot(transform.forward, direction.normalized);

            if (angle < 1.0f - 0.5f)
                continue;

            if (minDistance > distance)
            {
                minDistance = distance;
                minAngle = angle;
                candinate = obj;
            }
        }

        return candinate;
    }

    private void ChangeTarget(GameObject target)
    {
        if (target == null)
        {
            EndTargeting(true);
            return;
        }

        EndTargeting();

        targetObject = target;

        if (cameraArm != null)
        {
            cameraArm.SetTarget(targetObject);
        }
        //TODO: 회전 기능이 없으므로 여기에 정리했음
        bMovingFocus = false;

        if (lockOnUI != null)
        {
            if (Camera.main.transform.childCount > 0)
            {
                if (Camera.main.transform.GetChild(0).TryGetComponent<Camera>(out Camera subCamera))
                {
                    var lockOnCanvas = UIHelpers.CreateBillboardCanvas(lockOnUI, targetObject.transform, subCamera);
                    if (lockOnCanvas != null)
                    {
                        lockOnCanvas.name = "LockOn";
                        // 캔버스가 원래 위치로 가려는 성질을 죽이기 위한 작업
                        // 자식으로 설정하면서 위치와 회전을 초기화합니다.
                        lockOnCanvas.transform.SetParent(targetObject.transform, false);

                        // 위치 초기화
                        lockOnCanvas.transform.localPosition = Vector3.zero;
                        lockOnCanvas.transform.localRotation = Quaternion.identity;

                        // 필요한 오프셋 적용
                        Vector3 targetPosition = Vector3.zero;
                        if (targetObject.TryGetComponent<Collider>(out Collider collider))
                        {
                            float height = collider.bounds.size.y;
                            targetPosition = new Vector3(0, height * 0.5f, 0);
                        }
                        lockOnCanvas.transform.localPosition += targetPosition;

                        // 거리에 따른 UI 크기 설정 
                        if (lockOnCanvas.TryGetComponent<SimpleLockOnUI>(out SimpleLockOnUI lockOnUI))
                        {
                            StartCoroutine(Corotuine_LockOnUIScale(lockOnUI));
                        }

                    }
                }
            }
        }
    }

    IEnumerator Corotuine_LockOnUIScale(SimpleLockOnUI lockOnUI)
    {
        if (targetObject == null || lockOnUI == null)
            yield break;

        while (true)
        {
            if (targetObject == null || lockOnUI == null)
                yield break;

            lockOnUI.SetScale(Vector3.one * ((transform.position - targetObject.transform.position).magnitude
                                * lockOnUIScale));

            yield return new WaitForEndOfFrame();
        }
    }


    private void RotateToTarget()
    {
        if (targetObject == null)
        {
            return;
        }

        Vector3 position = transform.position;
        Vector3 targetPosition = targetObject.transform.position;
        Vector3 direction = targetPosition - position;
        direction.y = 0.0f;

        Quaternion from = transform.localRotation;
        Quaternion to = Quaternion.LookRotation(direction.normalized, Vector3.up);

        if (Quaternion.Angle(from, to) < 2.0f)
        {
            //deltaRotation = 0.0f;
            transform.localRotation = to;

            return;
        }

        transform.localRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        //deltaRotation += rotateSpeed * Time.deltaTime;
        //transform.localRotation = Quaternion.RotateTowards(from, to, deltaRotation);
    }

    private void ChangeFocucs(bool bRight)
    {
        if (targetObject == null)
            return;


        // 움직이는 중일 땐 입력 받지 않도록 
        if (bMovingFocus == true)
            return;

        bMovingFocus = true;


        Collider[] colliders = Physics.OverlapSphere(transform.position, radius, layerMask.value);

        // 외적 , 겜오브젝
        Dictionary<float, GameObject> candinateTable = new Dictionary<float, GameObject>();
        foreach (Collider collider in colliders)
        {
            if (targetObject == collider.gameObject)
                continue;

            Vector3 vec1 = collider.transform.position;
            Vector3 vec2 = transform.position;
            Vector3 direction = vec1 - vec2;

            // 평행간 거리 등장 
            Vector3 cross = Vector3.Cross(transform.forward, direction);
            float distance = Vector3.Dot(cross, Vector3.up); // <- y값 추출하기 외적에 내적을 추출하면 평행간에 거리가 나온다

            candinateTable.Add(distance, collider.gameObject);
        }

        float minmum = float.MaxValue;
        GameObject candinate = null;

        foreach (float distance in candinateTable.Keys)
        {
            if (Mathf.Abs(distance) >= minmum)
                continue;

            if (bRight && distance > 0.0f)
            {
                minmum = Mathf.Abs(distance);
                candinate = candinateTable[distance];
            }

            if (bRight == false && distance < 0.0f)
            {
                minmum = Mathf.Abs(distance);
                candinate = candinateTable[distance];
            }
        }

        ChangeTarget(candinate);
    }



    private void EndTargeting(bool bLookForward = false)
    {

        if (targetObject != null)
        {
            Transform particle = targetObject.transform.FindChildByName("Target(Clone)");
            Transform lockon = targetObject.transform.FindChildByName("LockOn");

            if (particle != null)
                Destroy(particle.gameObject);

            if (lockon != null)
                Destroy(lockon.gameObject);

            StopAllCoroutines();
        }


        //deltaRotation = 0.0f;
        targetObject = null;
        if (cameraArm != null)
            cameraArm.SetTarget(null);

        // 후보자가 없으면 그냥 다 풀어.. 
        //if (bLookForward == true)
        //    transform.localRotation = Quaternion.identity;
    }

}
