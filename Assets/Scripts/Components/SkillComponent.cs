using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum SkillType
{
    None = 0, Fist, Sword, Hammer, Dual, Warp, Meteor, Max,
}

public class SkillComponent : MonoBehaviour
{
    [SerializeField] private GameObject[] originPrefabs;

    private Animator animator;
    private StateComponent state;

    private SkillType type = SkillType.None;

    //public event Action<SkillType, SkillType> OnSkillTypeChanged;

    public bool NoneMode { get => type == SkillType.None; }
    public bool WarpMode { get => type == SkillType.Warp; }
    public bool MeteorMode { get => type == SkillType.Meteor; }

    private Dictionary<SkillType, Skill> skillTable;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();
    }

    private void Start()
    {
        skillTable = new Dictionary<SkillType, Skill>();

        for (int i = 0; i < (int)SkillType.Max; i++)
            skillTable.Add((SkillType)i, null);


        for (int i = 0; i < originPrefabs.Length; i++)
        {
            GameObject obj = Instantiate<GameObject>(originPrefabs[i], transform);
            Skill skill = obj.GetComponent<Skill>();
            obj.name = skill.Type.ToString();

            skillTable[skill.Type] = skill;
        }
    }


    public void DoSkillAction()
    {
        if (skillTable[type] == null)
            return;

        animator.SetTrigger("SkillAction");

        skillTable[type].DoAction();
    }

    private void Begin_DoSkillAction()
    {
        skillTable[type].Begin_DoAction();
    }

    private void End_DoSkillAction()
    {

    }

}
