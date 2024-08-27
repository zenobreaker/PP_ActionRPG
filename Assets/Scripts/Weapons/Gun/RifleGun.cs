using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RifleGun : MonoBehaviour
{
    [SerializeField] private GameObject scopeObject;
    [SerializeField] private CameraArm cameraArm;



    private void Start()
    {
        cameraArm = FindAnyObjectByType<CameraArm>();
    }

    public GameObject GetScopeObj()
    {
        return cameraArm.gameObject;
    }

    public void SetSnipeMode()
    {
        if (scopeObject == null)
            return;

        scopeObject.SetActive(true);
        if(cameraArm != null)
        {
            cameraArm.SetSnipeMode(scopeObject);
        }
    }

    public void EndSnipeMode()
    {
        if (scopeObject == null)
            return;

        scopeObject.SetActive(false);
        if (cameraArm != null)
        {
            cameraArm.EndSnipeMode();
        }
    }
}
