using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Starward : Skill_Trigger_Melee
{
    public override void ExecuteSkill()
    {
        base.ExecuteSkill();

        StartCoroutine(Finish_Evidence());
    }

    // 이 스킬은 마지막엔 지연 타격을 위해서 한 횟수 덜 때린다.
    public override void ShouldLoopCount(ref int maxCount)
    {
        base.ShouldLoopCount(ref maxCount);
        maxCount -= 1;
    }

    // 마무리 공격은 어느 정도 대기하고 실행되게 이전 코루틴에서 처리한 후에 올 것이다?
    private IEnumerator Finish_Evidence()
    {
        SkillActionData data = skillData.skillActions[skillData.skillActions.Length - 1];

        //이 스킬의 마지막 공격은 지연되게
        yield return new WaitForSeconds(3.0f);

        Collider[] colliders = Physics.OverlapSphere(this.transform.position, skillData.skillRange);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject == rootObject)
                continue;


            SoundManager.Instance.PlaySFX(data.effectSoundName);
            Debug.Log("피니쉬 히트" + collider.name);
            ApplyOnSkillHit(collider, data);
        }

        yield return null;
    }

}
