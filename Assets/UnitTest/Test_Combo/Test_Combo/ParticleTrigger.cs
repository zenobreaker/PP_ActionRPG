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

        Debug.Log("�� �̰� ��  �� �ֳ�");
    }

    private void OnParticleTrigger()
    {
        Debug.Log("�̰� ����");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("�̰� ����2");
    }
}
