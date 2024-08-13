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

    //TODO: 스킬 사운드 처리는 언제할까
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

    //    // 1. 감지 조건 대상자 선정
    //    foreach (Collider collider in colliders)
    //    {
    //        if(collider.gameObject == rootObject) 
    //            continue;
    //        candidateList.Add(collider);    
    //    }

    //    // 2. 감지 대상자 등록 및 시간 업데이트 
    //    foreach(Collider collider in candidateList)
    //    {
    //        if(hittedTable.ContainsKey(collider.gameObject) == false)
    //        {
    //            hittedTable.Add(collider.gameObject, Time.realtimeSinceStartup);

    //            continue;
    //        }
    //        hittedTable[collider.gameObject] = Time.realtimeSinceStartup;
    //    }

    //    //3.시간초과 대상자 선정 및 삭제 
    //    List<GameObject> removeList = new List<GameObject>();
    //    foreach(var hitted in hittedTable)
    //    {
    //        if (Time.realtimeSinceStartup - hitted.Value < skillData.repeatDelayTime)
    //            removeList.Add(hitted.Key);
    //    }

    //    removeList.RemoveAll(remove  => hittedTable.Remove(remove));

    //    // 스킬 데미지 입히는 함수 콜 
    //}

    //private void Check_SkillEffect(Collider other)
    //{
    //    OnSkillHit?.Invoke(other, skillData.skillActions[i]);
    //}

}

