using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonMeleeWeapon : MonoBehaviour
{
    public DoActionData actitonData;
    public GameObject owner; 

    private List<GameObject> hitList = new List<GameObject>();
    private Collider[] colliders;

    private void Awake()
    {
        colliders = GetComponentsInChildren<Collider>();
    }

    private void OnEnable()
    {
        hitList.Clear();

        SoundManager.Instance.PlaySFX(actitonData.effectSoundName);

        if (actitonData.Particle == null)
            return;

        Instantiate(actitonData.Particle, this.gameObject.transform.position, Quaternion.identity);
    }

    private void OnDisable()
    {
        hitList.Clear();
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == owner)
            return;

        if (hitList.Contains(other.gameObject) == true)
            return; 

        hitList.Add(other.gameObject);
        
        IDamagable damagable = other.GetComponent<IDamagable>();

        // hit Sound Play
        SoundManager.Instance.PlaySFX(actitonData.hitSoundName);
        if (damagable == null)
            return;

        Vector3 hitPoint = Vector3.zero;

        Collider enabledCollider = null;
        foreach (Collider collider in colliders)
        {
            if (collider.enabled)
            {
                enabledCollider = collider;
                break;
            }
        }


        hitPoint = enabledCollider.ClosestPoint(other.transform.position);

        hitPoint = other.transform.InverseTransformPoint(hitPoint);

        damagable.OnDamage(owner, null, hitPoint, actitonData);

    }
}
