using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RifleGun : MonoBehaviour
{
    [SerializeField] private GameObject scopeObject;
    [SerializeField] private CameraArm cameraArm;
    [SerializeField] private GameObject rifleMuzzle; 


    private void Start()
    {
        cameraArm = FindAnyObjectByType<CameraArm>();
    }

    public GameObject GetScopeObj()
    {
        return scopeObject;
    }

    public GameObject GetCameraObj()
    {
        return cameraArm.GetRifleCameraObj();
    }

    public GameObject GetMuzzleObj()
    {
        return rifleMuzzle;
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
