using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: ������ ���������� ���� ����
/// <summary>
/// �� ��ų�� ��ų�� ������ ���� ũ��� ����Ʈ ũ�Ⱑ ��ġ�ؾ� �ϸ� 
/// �ּ� ������Ʈ ���� ���� ������ ũ�Ⱑ ���Ƽ� �ȵȴ�
/// �ܺ� �����͸� �����ϸ� ��ų ������ �����ؾ� �ϴ� �����̴� 
/// </summary>

public class Skill_Starward : Skill_Trigger_Melee
{
    [SerializeField] private GameObject[] effectObjs; // ��ų ����Ʈ�� 


    public override void ExecuteSkill()
    {
        //base.ExecuteSkill();
        Play_SkillMainSound();
        Play_SkillEffectParticle();
        StartCoroutine(Play_Starward());
    }

    /// <summary>
    /// �ش� ��ų�� 1ȸ ����Ʈ �� 2ȸ Ÿ���� �ǽ��Ѵ�.
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

        // ��� ������ �� ������ �� ��ų ����Ʈ ����(Ȥ�� diable) 
        Destroy(gameObject);
    }
}
