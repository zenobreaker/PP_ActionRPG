using JetBrains.Annotations;
using System;
using UnityEngine;
using static UnityEngine.ParticleSystem;


[Serializable]
public class SkillActionData : ActionData
{
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

        return skill;
    }
}

[CreateAssetMenu(fileName = "NewSkill", menuName = "ScriptableObjects/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;                // ��ų �̸�
    public WeaponType weaponType;           // ���� Ÿ��
    public bool bCanMove;
    public GameObject Particle;             // ��ų ��ƼŬ 
    public float cooldown;                  // ��ٿ� �ð�
    public float skillRange;                // ��ų ���� 
    public string animationName;            // ��ų �ִϸ��̼� �̸�
    public string skillMainSound;           // ��ų�� �ֿ� ���� ������ ��� 
    public float repeatDelayTime = 0.0f ;           // ��ų ���� �ݺ� �ð� 

    public SkillActionData[] skillActions;
    public Vector3 additionalPos;           // ����Ʈ ������ ���� ��ġ

    public SkillData DeepCopy()
    {
        SkillData s = new SkillData();
        s.skillName = skillName;
        s.Particle = Particle;
        s.weaponType = weaponType;
        s.cooldown = cooldown;
        s.skillRange = skillRange;
        s.animationName = animationName;
        s.skillMainSound = skillMainSound;
        s.repeatDelayTime = repeatDelayTime;

        s.skillActions = new SkillActionData[skillActions.Length];
        for(int i = 0; i < skillActions.Length; i++)
            s.skillActions[i] = skillActions[i].DeepCopy();

        s.additionalPos = additionalPos;
        return s;

    }
}
