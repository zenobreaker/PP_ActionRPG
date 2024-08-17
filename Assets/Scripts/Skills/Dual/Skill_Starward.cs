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

    // �� ��ų�� �������� ���� Ÿ���� ���ؼ� �� Ƚ�� �� ������.
    public override void ShouldLoopCount(ref int maxCount)
    {
        base.ShouldLoopCount(ref maxCount);
        maxCount -= 1;
    }

    // ������ ������ ��� ���� ����ϰ� ����ǰ� ���� �ڷ�ƾ���� ó���� �Ŀ� �� ���̴�?
    private IEnumerator Finish_Evidence()
    {
        SkillActionData data = skillData.skillActions[skillData.skillActions.Length - 1];

        //�� ��ų�� ������ ������ �����ǰ�
        yield return new WaitForSeconds(3.0f);

        Collider[] colliders = Physics.OverlapSphere(this.transform.position, skillData.skillRange);

        foreach (Collider collider in colliders)
        {
            if (collider.gameObject == rootObject)
                continue;


            SoundManager.Instance.PlaySFX(data.effectSoundName);
            Debug.Log("�ǴϽ� ��Ʈ" + collider.name);
            ApplyOnSkillHit(collider, data);
        }

        yield return null;
    }

}
