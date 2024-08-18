using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Skill_Trigger : MonoBehaviour
{
    protected SkillData skillData;
    protected GameObject rootObject;
    private List<Collider> hittedList = new List<Collider>();

    public event Action<Collider, SkillActionData> OnSkillHit;
    public event Action OnSkillSpecialEvent;

    public virtual void SetRootObject(GameObject rootObject)
    {
        this.rootObject = rootObject;
        if(skillData != null)
        {
            if (skillData.bSameOwner == false)
                rootObject = this.gameObject;
        }
    }
    public virtual void SetSkillData(SkillData skillData)
    {
        this.skillData = skillData;
    }

    protected virtual void OnEnable()
    {

    }

    protected virtual void Update()
    {

    }

    protected virtual void OnTriggerEnter(Collider other)
    {

    }

    public abstract void ExecuteSkill();

    protected virtual void ApplyOnSkillHitWithColliders(Collider[] colliders, SkillActionData data)
    {
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject == rootObject)
                continue;

            ApplyOnSkillHit(collider, data);
        }
    }

    protected void ApplyOnSkillHit(Collider collider, SkillActionData actionData)
    {
        OnSkillHit?.Invoke(collider, actionData);
    }

    //TODO: 스킬 사운드 처리는 언제할까
    protected  void Play_SkillMainSound()
    {
        if (skillData == null)
            return;

        SoundManager.Instance.PlaySFX(skillData.skillMainSound);
    }

    protected void Play_SkillEffectParticle()
    {
        if (skillData == null)
            return;

        if (skillData.EffectParticle == null)
            return; 

        Instantiate<GameObject>(skillData.EffectParticle, transform.position, transform.rotation);
    }

    protected void ActivateSpecialEvent()
    {
        OnSkillSpecialEvent?.Invoke();
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

#if UNITY_EDITOR
    public void OnDrawGizmosSelected()
    {
        if (skillData == null) return;

        Gizmos.color = Color.green;

        Gizmos.DrawWireSphere(transform.position, skillData.skillRange);
    }

#endif

}

