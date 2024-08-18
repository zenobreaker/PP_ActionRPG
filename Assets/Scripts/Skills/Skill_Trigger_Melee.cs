using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Trigger_Melee : Skill_Trigger
{
    public override void ExecuteSkill()
    {
        Play_SkillMainSound();
        Play_SkillEffectParticle();
        StartCoroutine(Apply_Skill());
    }

    protected virtual IEnumerator Apply_Skill()
    {
        int maxCount = skillData.skillActions.Length;

        ShouldLoopCount(ref maxCount);

        yield return new WaitForSeconds(skillData.DelayTime);

        for (int i = 0; i < maxCount; i++)
        {
            Collider[] colliders = Physics.OverlapSphere(this.transform.position, skillData.skillRange);

            SoundManager.Instance.PlaySFX(skillData.skillActions[i].effectSoundName);

            ApplyOnSkillHitWithColliders(colliders, skillData.skillActions[i]);

            yield return new WaitForSeconds(skillData.skillActions[i].HitDelayTime);
        }

    }

    public virtual void ShouldLoopCount(ref int maxCount)
    {
        
    }
}
