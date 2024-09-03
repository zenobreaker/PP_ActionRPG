using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Fist_SubAction_Strike : MonoBehaviour
{
    [SerializeField] private GameObject[] paritlces;
    private GameObject rootObject; 

    public event Action<Collider> OnSubActionHit;

     private int index = 0;


    private void Update()
    {
        if (rootObject == null)
            return;

        Vector3 position = rootObject.transform.position;
        position.y = this.transform.position.y;
        this.transform.position = position;
    }

    public void Apply_Effect(GameObject root)
    {
        if (paritlces.Length <= 0 || index >= paritlces.Length)
            return;

        rootObject = root;

        StartCoroutine(ApplyEffectCoroutine());
    }


    private IEnumerator ApplyEffectCoroutine()
    {
        if(rootObject == null)
            yield break;

        paritlces[index].SetActive(true);
        SoundManager.Instance.PlaySFX("Fist_SubAction_Effect");
        yield return new WaitForFixedUpdate();

        Vector3 center = rootObject.transform.position + rootObject.transform.forward * 1.5f;
        Collider[] colliders = Physics.OverlapSphere(center, 1.75f);
        foreach (var collider in colliders)
        {
            if (collider.gameObject.CompareTag("Player"))
                continue;
            if (paritlces[index] == collider.gameObject)
                continue;

            if (collider.TryGetComponent<IDamagable>(out var damage))
            {
                Debug.Log("Fist sub action hit");
                OnSubActionHit?.Invoke(collider);
                StartCoroutine(PulledEneyCoroutine(collider.gameObject));
            }
        }

        index++;
    }


    // ���� Ư�� �������� ���� ������� ȿ��
    private IEnumerator PulledEneyCoroutine(GameObject target)
    {
        if (target == null || rootObject == null)
            yield break;

        float elapsedTime = 0.0f;
        float duration = 0.5f;
        Vector3 position = target.transform.position;
        Vector3 center = rootObject.transform.position + rootObject.transform.forward * 1.5f;
        Vector3 direction = center - position;
        direction.y = 0f;

        Vector3 stepDistance = direction / paritlces.Length;
        Vector3 stepPosition = position + (stepDistance * index * 0.5f);
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;

            target.transform.position = Vector3.Lerp(position, stepPosition, elapsedTime / duration);

            yield return new WaitForEndOfFrame();
        }

        target.transform.position = stepPosition;
    }


#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || rootObject == null)
            return;

        Gizmos.color = Color.green;
        Vector3 center = rootObject.transform.position + rootObject.transform.forward * 1.5f;
        Gizmos.DrawWireSphere(center, 1.75f);
    }

#endif

}
