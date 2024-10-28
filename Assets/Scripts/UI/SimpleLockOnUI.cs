using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleLockOnUI : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] Canvas canvas;
    [SerializeField] Image lockOnImage;
    [SerializeField]
    [Range(0.5f, 1.0f)]
    float MaxScale = 0.5f;
    private void Start()
    {
        canvas = GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        SetCanvasCamera();

        if (target == null)
            target = Camera.main.transform;
        StartCoroutine(LookAtTarget());
    }    

    private void SetCanvasCamera()
    {
        if (canvas == null)
            return;

        if (Camera.main.transform.childCount > 0)
        {
            if (Camera.main.transform.GetChild(0).TryGetComponent<Camera>(out Camera subCamera))
                canvas.worldCamera = subCamera;
        }
    }

    public void SetScale(Vector3 scale)
    {
        float finalScale = Mathf.Clamp(scale.magnitude, scale.magnitude, MaxScale);
        lockOnImage.transform.localScale = Vector3.one * finalScale;
    }


    private IEnumerator LookAtTarget()
    {
        while(this.gameObject.activeInHierarchy)
        {
            // 카메라 바라보기
            Vector3 dir = target.position - transform.position;
            transform.rotation = Quaternion.LookRotation(dir);
            
            yield return null;
        }
    }
}
