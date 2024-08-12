using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillDataManager : MonoBehaviour
{
    private static SkillDataManager instance;
    public static SkillDataManager Instance { get { return instance; } }

    public List<SkillData> skillDatas;
    
    private Dictionary<WeaponType, List<SkillData>> skillDataTable;
    
    public event Action OnSetSkillData;

    private void Awake()
    {
        if (instance == null)
            instance = this; 
        else
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        // 게임 시작 시, 무기 타입별로 스킬을 참조하여 저장
        skillDataTable = new Dictionary<WeaponType, List<SkillData>>();

        foreach (var skill in skillDatas)
        {
            if (!skillDataTable.ContainsKey(skill.weaponType))
            {
                skillDataTable[skill.weaponType] = new List<SkillData>();
            }
            skillDataTable[skill.weaponType].Add(skill);
        }

        OnSetSkillData?.Invoke();
    }    


    public List<SkillData> GetSkillForWeaponType(WeaponType type)
    {
        Debug.Log($"{type}");
        if (skillDataTable.ContainsKey(type))
            return skillDataTable[type];
        return new List<SkillData>();
    }

}
