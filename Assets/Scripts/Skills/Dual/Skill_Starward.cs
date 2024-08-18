using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: 구조가 수동적인지 못한 문제
/// <summary>
/// 이 스킬은 스킬의 데이터 정보 크기와 이펙트 크기가 일치해야 하며 
/// 최소 오브젝트 개수 보다 데이터 크기가 많아서 안된다
/// 외부 데이터를 수정하면 스킬 로직도 수정해야 하는 구조이다 
/// </summary>

public class Skill_Starward : Skill_Trigger_Melee
{
    [SerializeField] private GameObject[] effectObjs; // 스킬 이펙트들 


    public override void ExecuteSkill()
    {
        //base.ExecuteSkill();
        Play_SkillMainSound();
        Play_SkillEffectParticle();
        StartCoroutine(Play_Starward());
    }

    /// <summary>
    /// 해당 스킬은 1회 이펙트 때 2회 타격을 실시한다.
    /// </summary>
    private IEnumerator Play_Starward()
    {
        yield return new WaitForSeconds(skillData.DelayTime);


        effectObjs[0].SetActive(true);
        for (int x = 0; x < 2; x++)
        {
            Collider[] colliders = Physics.OverlapSphere(this.transform.position, skillData.skillRange);

            SoundManager.Instance.PlaySFX(skillData.skillActions[x].effectSoundName);

            ApplyOnSkillHitWithColliders(colliders, skillData.skillActions[x]);


            yield return new WaitForSeconds(skillData.skillActions[x].HitDelayTime);
        }

        effectObjs[1].SetActive(true);
        for (int x = 2; x < 4; x++)
        {
            Collider[] colliders = Physics.OverlapSphere(this.transform.position, skillData.skillRange);

            SoundManager.Instance.PlaySFX(skillData.skillActions[x].effectSoundName);

            ApplyOnSkillHitWithColliders(colliders, skillData.skillActions[x]);


            yield return new WaitForSeconds(skillData.skillActions[x].HitDelayTime);
        }

        effectObjs[2].SetActive(true);
        for (int x = 4; x < 6; x++)
        {
            Collider[] colliders = Physics.OverlapSphere(this.transform.position, skillData.skillRange);

            SoundManager.Instance.PlaySFX(skillData.skillActions[x].effectSoundName);

            ApplyOnSkillHitWithColliders(colliders, skillData.skillActions[x]);


            yield return new WaitForSeconds(skillData.skillActions[x].HitDelayTime);
        }

        effectObjs[3].SetActive(true);
        for (int x = 6; x < 8; x++)
        {
            Collider[] colliders = Physics.OverlapSphere(this.transform.position, skillData.skillRange);

            SoundManager.Instance.PlaySFX(skillData.skillActions[x].effectSoundName);

            ApplyOnSkillHitWithColliders(colliders, skillData.skillActions[x]);


            yield return new WaitForSeconds(skillData.skillActions[x].HitDelayTime);
        }


        int max = skillData.skillActions.Length - 1;
        effectObjs[4].SetActive(true);
        {
            ActivateSpecialEvent();

            Collider[] colliders = Physics.OverlapSphere(this.transform.position, skillData.skillRange);

            SoundManager.Instance.PlaySFX(skillData.skillActions[max].effectSoundName);

            ApplyOnSkillHitWithColliders(colliders, skillData.skillActions[max]);
        }

        yield return new WaitForSeconds(skillData.skillActions[max].HitDelayTime);

        // 모든 수행을 다 했으면 이 스킬 이펙트 삭제(혹은 diable) 
        Destroy(gameObject);
    }
}
