using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 옵저버 패턴처럼 사용하기 위한 징검다리 클래스
/// </summary>
/// 
[CreateAssetMenu(menuName = "Events/SkillEvent", order = 1) ]
public class SkillEvent : ScriptableObject
{
    public event Action<float> OnSkillCoolDown;

    public event Action<SkillData> OnSkillData_SlotOne;

    public event Action OnDisableSkill;

    public void SetSkillOne(SkillData skillData)
    {
        OnSkillData_SlotOne?.Invoke(skillData);
    }

    public void OnCoolDown(float InCoolDown)
    {
        OnSkillCoolDown?.Invoke(InCoolDown);
    }    

    public void OnUnequipment()
    {
        OnDisableSkill?.Invoke();
    }
}
