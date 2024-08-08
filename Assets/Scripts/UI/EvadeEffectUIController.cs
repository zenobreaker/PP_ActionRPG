using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EvadeEffectUIController : MonoBehaviour
{
    [SerializeField] private EvadeEffectUI evadeUIEffect;


    private void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform t = transform.GetChild(i); 
            if (t != null)
            {
                if(t.TryGetComponent<EvadeEffectUI>(out var component))
                {
                    evadeUIEffect = component;
                    break;
                }
            }
        }
    }

    public void OnEvadeEffectUi()
    {
        if (evadeUIEffect == null)
            return;

        evadeUIEffect.gameObject.SetActive(true);
    }
 
}
