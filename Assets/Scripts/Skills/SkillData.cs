using JetBrains.Annotations;
using System;
using UnityEngine;
using static UnityEngine.ParticleSystem;


[Serializable]
public class SkillActionData : ActionData
{
    public float HitDelayTime;

    public SkillActionData DeepCopy()
    {
        SkillActionData skill = new SkillActionData();
       
        skill.bDownable = bDownable;
        skill.bLauncher = bLauncher;

        skill.Power = Power;
        skill.Distance = Distance;
        skill.heightValue = heightValue;

        skill.StopFrame = StopFrame;

        skill.effectSoundName = effectSoundName;

        skill.impulseDirection = impulseDirection;
        skill.impulseSettings = impulseSettings;

        skill.HitImpactIndex = HitImpactIndex;
        skill.hitSoundName = hitSoundName;
        skill.HitParticle = HitParticle;
        skill.HitParticlePositionOffset = HitParticlePositionOffset;
        skill.HitParticleSacleOffset = HitParticleSacleOffset;

        skill.HitDelayTime = HitDelayTime;

        return skill;
    }
}

[CreateAssetMenu(fileName = "NewSkill", menuName = "ScriptableObjects/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;                // 스킬 이름
    public WeaponType weaponType;           // 무기 타입
    public bool bCanMove;
    public GameObject Particle;             // 스킬 파티클 
    public float cooldown;                  // 쿨다운 시간
    public float skillRange;                // 스킬 범위 
    public string animationName;            // 스킬 애니메이션 이름
    public string skillMainSound;           // 스킬에 주요 사운드 없으면 비움 
    public float DelayTime = 0.0f;           // 스킬 시전 시간 

    public SkillActionData[] skillActions;
    public GameObject EffectParticle;       // 스킬 이펙트 
    public Vector3 additionalPos;           // 이펙트 프리팹 생성 위치
    public bool bSameOwner;                 // 주체자가 누구인가

    public SkillData DeepCopy()
    {
        SkillData s = ScriptableObject.CreateInstance<SkillData>();
        s.skillName = skillName;
        s.Particle = Particle;
        s.weaponType = weaponType;
        s.cooldown = cooldown;
        s.skillRange = skillRange;
        s.animationName = animationName;
        s.skillMainSound = skillMainSound;
        s.DelayTime = DelayTime;

        s.skillActions = new SkillActionData[skillActions.Length];
        for(int i = 0; i < skillActions.Length; i++)
            s.skillActions[i] = skillActions[i].DeepCopy();

        s.EffectParticle = EffectParticle;
        s.additionalPos = additionalPos;
        s.bSameOwner = bSameOwner;
        return s;

    }
}
