using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill_Trigger : MonoBehaviour
{
    private SkillData skillData;
    private GameObject rootObject;
    public event Action<Collider, SkillData> OnSkillHit;

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
        if (skillData == null)
            return;

        OnSkillHit?.Invoke(other, skillData);
    }


}
