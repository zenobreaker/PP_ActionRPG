using System;
using System.Collections;
using UnityEngine;


public class EffectSequenceExecuter : MonoBehaviour
{
    [Serializable]
    class EffectObject
    {
        public GameObject effect;
        public float delayTime;
    }

    [SerializeField] private bool bDestroy;
    [SerializeField] private EffectObject[] effectObjects;
    private void OnEnable()
    {
        StartCoroutine(Play_Sequence());
    }


    private IEnumerator Play_Sequence()
    {
        if(effectObjects == null || effectObjects.Length <= 0)
            yield break;

        foreach(EffectObject obj in effectObjects)
        {
            obj.effect.SetActive(true);
            yield return new WaitForSeconds(obj.delayTime);
        }

        if (bDestroy)
            Destroy(gameObject);
    }
}

