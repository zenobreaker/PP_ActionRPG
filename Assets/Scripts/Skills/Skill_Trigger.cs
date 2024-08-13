using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Skill_Trigger : MonoBehaviour
{
    private SkillData skillData;
    private GameObject rootObject;
    private List<Collider> hittedList = new List<Collider>();

    public event Action<Collider, SkillActionData> OnSkillHit;

    public void SetRootObject(GameObject rootObject)
    {
        this.rootObject = rootObject;
    }
    public void SetSkillData(SkillData skillData)
    {
        this.skillData = skillData;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == rootObject)
            return;

        hittedList.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        hittedList.Remove(other);
    }


    public void ExecuteSkill()
    {
        Play_SkillMainSound();
        StartCoroutine(Apply_Skill());
    }

    private IEnumerator Apply_Skill()
    {

        for (int i = 0; i < skillData.skillActions.Length; i++)
        {
            Collider[] colliders = Physics.OverlapSphere(this.transform.position, 5.0f);

            foreach (Collider collider in colliders)
            {
                if (collider.gameObject == rootObject)
                    continue;

                //var target = colliders.ToList().Find(x => x == collider);
                
                Debug.Log("skill count " + i);
                
                SoundManager.Instance.PlaySFX(skillData.skillActions[i].effectSoundName);

                OnSkillHit?.Invoke(collider, skillData.skillActions[i]);
            }

            yield return new WaitForSeconds(skillData.repeatDelayTime);
        }

    }

    //TODO: ��ų ���� ó���� �����ұ�
    private void Play_SkillMainSound()
    {
        if (skillData == null)
            return;

        SoundManager.Instance.PlaySFX(skillData.skillMainSound);
    }


    //private void Update()
    //{
    //    if (skillData == null)
    //        return;

    //    Collider[] colliders = Physics.OverlapSphere(this.transform.position, 5.0f);
    //    List<Collider> candidateList = new List<Collider>();

    //    // 1. ���� ���� ����� ����
    //    foreach (Collider collider in colliders)
    //    {
    //        if(collider.gameObject == rootObject) 
    //            continue;
    //        candidateList.Add(collider);    
    //    }

    //    // 2. ���� ����� ��� �� �ð� ������Ʈ 
    //    foreach(Collider collider in candidateList)
    //    {
    //        if(hittedTable.ContainsKey(collider.gameObject) == false)
    //        {
    //            hittedTable.Add(collider.gameObject, Time.realtimeSinceStartup);

    //            continue;
    //        }
    //        hittedTable[collider.gameObject] = Time.realtimeSinceStartup;
    //    }

    //    //3.�ð��ʰ� ����� ���� �� ���� 
    //    List<GameObject> removeList = new List<GameObject>();
    //    foreach(var hitted in hittedTable)
    //    {
    //        if (Time.realtimeSinceStartup - hitted.Value < skillData.repeatDelayTime)
    //            removeList.Add(hitted.Key);
    //    }

    //    removeList.RemoveAll(remove  => hittedTable.Remove(remove));

    //    // ��ų ������ ������ �Լ� �� 
    //}

    //private void Check_SkillEffect(Collider other)
    //{
    //    OnSkillHit?.Invoke(other, skillData.skillActions[i]);
    //}

}

