using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_PlanetCrash : Skill_Trigger_Projectile
{
    [SerializeField] protected GameObject EffectPrefab;
    [SerializeField] private LayerMask collisionLayer; 

    protected override void Update()
    {
        transform.Translate(moveSpeed * Time.deltaTime * direction);
        
    }

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        
        if((1 << other.gameObject.layer) == collisionLayer.value)
        {
            // ÆÄ±«ÀÌÆåÆ® 
            var effectPrefab = Instantiate<GameObject>(EffectPrefab, transform.position, transform.rotation);
            if(effectPrefab.TryGetComponent<Skill_Trigger>(out Skill_Trigger st))
            {
                st.OnSkillHit += ApplyOnSkillHit;
            }

            Destroy(this.gameObject);
        }
    }

}
