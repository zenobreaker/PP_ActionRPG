using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableController : MonoBehaviour
{
    public float disableTime = 0.0f;
    private float currentTime; 
    [SerializeField] private Collider target;

    private void OnEnable()
    {
        currentTime = 0;
    }

    private void Update()
    {
        if (currentTime < disableTime)
        {
            currentTime += Time.deltaTime;
        }
        else
        {
            if (target != null)
                target.enabled = false;
        }
    }
}
