using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;




public class SkillComponent : MonoBehaviour
{
    private Animator animator;
    private StateComponent state;
    private WeaponComponent weapon;

    private List<SkillData> currentSkillDatas = new List<SkillData>();
    private SkillData currentSkill; 
    public SkillData CurrSkill { get => currentSkill; }
    private Dictionary<WeaponType, List<SkillData>> skillDataTable;
    private Dictionary<string, float> skillCooldownTimerTable;
    private Dictionary<string, string> skillInputTable;

    private void Awake()
    {
        skillInputTable = new Dictionary<string, string>
        {
            {"Skill1", "" },
            {"Skill2", "" },
            {"Skill3", "" }
        };

        animator = GetComponent<Animator>();
        Debug.Assert (animator != null);
    
        state = GetComponent<StateComponent>();
        Debug.Assert(state != null);
        
        weapon = GetComponent<WeaponComponent>();
        Debug.Assert(weapon != null);
        weapon.OnWeaponTypeChanged += OnWeaponTypeChanged;
        
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


    private void Update()
    {
        if (skillCooldownTimerTable == null)
            return; 

        // ��ٿ� Ÿ�̸� ������Ʈ
        var keys = new List<string>(skillCooldownTimerTable.Keys);
        foreach (var key in keys)
        {
            if (skillCooldownTimerTable[key] > 0)
            {
                skillCooldownTimerTable[key] -= Time.deltaTime;
            }
        }
    }

    // ��ų ���� 
    public void DoSkillAction(string skillInput, Weapon weapon)
    {
        if (weapon == null)
            return;

        if (skillInputTable.ContainsKey(skillInput))
        {
            UseSkill(skillInputTable[skillInput], weapon);
        }

    }

    public void UseSkill(string skillName)
    {
        if (skillCooldownTimerTable.ContainsKey(skillName))
        {
            float currentCooldown = skillCooldownTimerTable[skillName];
            if (currentCooldown <= 0f)
            {
                // ��ų ���� ����
                ExecuteSkill(skillName);

                // ��ٿ� �ð� ����
                var skill = currentSkillDatas.Find(s => s.skillName == skillName);
                if (skill != null)
                {
                    skillCooldownTimerTable[skillName] = skill.cooldown;
                }
            }
        }
    }

    public void UseSkill(string skillName, Weapon weapon)
    {
        if (weapon == null)
            return;

        if (skillCooldownTimerTable.ContainsKey(skillName))
        {
            float currentCooldown = skillCooldownTimerTable[skillName];
            if (currentCooldown <= 0f)
            {
                // ��ų ���� ����
                ExecuteSkill(skillName, weapon);

                // ��ٿ� �ð� ����
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
        }
    }

    private void ExecuteSkill(string skillName, Weapon weapon)
    {
        if (weapon == null)
            return;

        ExecuteSkill(skillName);
        if (currentSkill != null)
            weapon.DoSkillAction(currentSkill);
    }


    public void Play_SkillEffect()
    {
        if (currentSkill == null)
            return;
        if (weapon == null)
            return;

        weapon?.PlaySkillEffect(currentSkill);
    }

    public void Begin_SkillAction()
    {
        weapon?.BeginSkillAction();   
    }

    public void End_SkillAction()
    {
        weapon?.EndSkillAction();
        currentSkill = null;
    }

    private void OnSetSkillData()
    {
        if (weapon == null)
            return;

        for (int i = 0; i < (int)WeaponType.MAX; i++)
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
        foreach (KeyValuePair<WeaponType, List<SkillData>> pair in skillDataTable)
        {
            if (pair.Value == null)
                continue;
            foreach (SkillData data in pair.Value)
                str += data.skillName + " ";
        }
        Debug.Log($"{str}");
#endif
    }

  
    private void OnWeaponTypeChanged(WeaponType prevType, WeaponType newType)
    {
        if (skillDataTable.ContainsKey(newType))
        {
            currentSkillDatas = skillDataTable[newType];

            //TODO: ��� �׷��� ���� �� �ƴ϶�� ����
            foreach(SkillData skillData in currentSkillDatas)
            {
                SkillDataManager.SkillSlot slot = SkillDataManager.Instance.GetSkillSlotBySkillData(skillData);
                if (slot == SkillDataManager.SkillSlot.NONE)
                    continue; 

                if(slot == SkillDataManager.SkillSlot.Skill1)
                    skillInputTable["Skill1"] = skillDataTable[newType][0].skillName;
                //else if(slot == SkillDataManager.SkillSlot.Skill2)
                //    skillInputTable["Skill2"] = skillDataTable[newType][1].skillName;
                //else if(slot == SkillDataManager.SkillSlot.Skill3)
                //    skillInputTable["Skill3"] = skillDataTable[newType][2].skillName;

            }
            
        }
    }


}
