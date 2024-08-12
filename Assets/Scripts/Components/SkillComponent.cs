using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class SkillComponent : MonoBehaviour
{
    private Animator animator;
    private StateComponent state;
    private WeaponComponent weapon;

    private List<SkillData> currentSkillDatas = new List<SkillData>();
    private SkillData currentSkill; 
    private Dictionary<WeaponType, List<SkillData>> skillDataTable;
    private Dictionary<string, float> skillCooldownTimerTable;
    private Dictionary<string, string> skillInputTable;

    private void Awake()
    {
        skillInputTable = new Dictionary<string, string>
        {
            {"Skill1", "" },
            {"Skill2", "" },
        };

        animator = GetComponent<Animator>();
        Debug.Assert (animator != null);
    
        state = GetComponent<StateComponent>();
        Debug.Assert(state != null);
        
        weapon = GetComponent<WeaponComponent>();
        Debug.Assert(weapon != null);
        weapon.OnWeaponTypeChanged += OnWeaponTypeChanged;
        weapon.OnSkillAction += OnSkillAction;

        Awake_SkillData();

        skillCooldownTimerTable = new Dictionary<string, float>();
        
    }

    private void Start()
    {
        OnSetSkillData();
    }

    private void Awake_SkillData()
    {
        skillDataTable = new Dictionary<WeaponType, List<SkillData>>();
    }

    private void OnSetSkillData()
    {
        if (weapon == null)
            return; 

        for(int i = 0; i < (int)WeaponType.MAX; i++)
        {
            Debug.Log($"{(WeaponType)i}");
            skillDataTable.Add((WeaponType)i,
                SkillDataManager.Instance.GetSkillForWeaponType((WeaponType)i));
        }

        foreach (KeyValuePair<WeaponType, List<SkillData>> pair in skillDataTable)
        {
            if (pair.Value == null)
                continue;
            foreach (SkillData data in pair.Value)
                skillCooldownTimerTable.Add(data.skillName, 0);
                
        }


#if UNITY_EDITOR

        string str = "skills : "; 
        foreach(KeyValuePair<WeaponType, List<SkillData>> pair in skillDataTable)
        {
            if (pair.Value == null)
                continue;
            foreach (SkillData data in pair.Value)
                str += data.skillName + " ";
        }
        Debug.Log($"{str}");
#endif
    }

    private void OnSkillAction(string skillInput)
    {
        if(skillInputTable.ContainsKey(skillInput))
            UseSkill(skillInputTable[skillInput]);
    }

    private void OnSkillHit(Collider other)
    {
        Debug.Log("Skill hit!");

        if (currentSkill == null)
        {
            Debug.Log("No skill data");
            return;
        }

        IDamagable damage = other.GetComponent<IDamagable>();

        if (damage != null)
        {
            Vector3 hitPoint = Vector3.zero;
            hitPoint = other.transform.InverseTransformPoint(hitPoint);
            damage?.OnDamage(transform.gameObject, weapon.GetEquippedWeapon(), hitPoint, currentSkill.doAction);

            return;
        }

    }

    private void Update()
    {
        if (skillCooldownTimerTable == null)
            return; 

        // 쿨다운 타이머 업데이트
        var keys = new List<string>(skillCooldownTimerTable.Keys);
        foreach (var key in keys)
        {
            if (skillCooldownTimerTable[key] > 0)
            {
                skillCooldownTimerTable[key] -= Time.deltaTime;
            }
        }
    }

    private void OnWeaponTypeChanged(WeaponType prevType, WeaponType newType)
    {
        if (skillDataTable.ContainsKey(newType))
        {
            currentSkillDatas = skillDataTable[newType];
            
            if (skillDataTable[newType].Count > 0)
            {
                if (skillDataTable[newType].Count == 1)
                    skillInputTable["Skill1"] = skillDataTable[newType][0].skillName;
                if (skillDataTable[newType].Count % 2 == 0)
                    skillInputTable["Skill2"] = skillDataTable[newType][1].skillName;
            }
        }
    }

    public void UseSkill(string skillName)
    {
        if (skillCooldownTimerTable.ContainsKey(skillName))
        {
            float currentCooldown = skillCooldownTimerTable[skillName];
            if (currentCooldown <= 0f)
            {
                // 스킬 실행 로직
                ExecuteSkill(skillName);

                // 쿨다운 시간 설정
                var skill = currentSkillDatas.Find(s => s.skillName == skillName);
                if (skill != null)
                {
                    skillCooldownTimerTable[skillName] = skill.cooldown;
                }
            }
        }
    }


    private void ExecuteSkill(string skillName)
    {
        SkillData skill = currentSkillDatas.Find(s => s.skillName == skillName);
        if (skill != null)
        {
            Debug.Log($"Executing skill: {skill.skillName}");

            currentSkill = skill; 
            // 애니메이션 재생
            if (!string.IsNullOrEmpty(skill.animationName))
            {
                // 애니메이션 재생 로직
                animator.Play(skill.animationName);
            }
        }
    }
    private void CreateSkillEffect()
    {
        if (currentSkill == null)
            return;

        DoActionData aData = currentSkill.doAction;
        if (aData == null)
            return;

        if (aData.Particle == null)
            return; 

        GameObject obj = Instantiate<GameObject>(aData.Particle, transform.position, transform.rotation);
        if(obj.TryGetComponent<Skill_Trigger>(out Skill_Trigger trigger))
        {
            trigger.SetRootObject(gameObject);
            trigger.OnSkillHit += OnSkillHit;    
        }
    }


    private void Begin_SkillAction()
    {
        if (currentSkill == null)
            return;

        CreateSkillEffect(); 
    }

    private void End_SkillAction()
    {
        currentSkill = null;
    }


}
