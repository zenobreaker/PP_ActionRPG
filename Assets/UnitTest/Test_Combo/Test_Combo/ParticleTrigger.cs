using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleTrigger : MonoBehaviour
{
    private void Start()
    {
        
    }

    private void OnParticleCollision(GameObject other)
    {
        if (other == null)
            return;

        Debug.Log("음 이거 할  수 있나");
    }

    private void OnParticleTrigger()
    {
        Debug.Log("이거 뭐지");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("이거 뭐지2");
    }
}
