using UnityEngine;
using UnityEngine.UI;

public class Skill_Icon_UI : MonoBehaviour
{
    [SerializeField] private Image icon_CoolDown;
    [SerializeField] private Image icon_SkillImage;
    [SerializeField] private Image icon_SkillKeySlot;

    private float max_CoolDown; 

    public SkillEvent skillEvent;

    private void Awake()
    {
        skillEvent = SkillEventHelpers.CreateSkillEvent("SkillEvent");
        if(skillEvent != null )
        {
            skillEvent.OnSkillData_SlotOne += OnSkillData_SlotOne;
            skillEvent.OnSkillCoolDown += OnCoolDown;
            skillEvent.OnDisableSkill += OnDisableSkill;
        }
    }

    public void OnEnable()
    {
        if (skillEvent != null)
        {
            skillEvent.OnSkillData_SlotOne += OnSkillData_SlotOne;
            skillEvent.OnSkillCoolDown += OnCoolDown;
        }

        OnDisableSkill();
    }

    public void OnDisable()
    {
        if (skillEvent != null)
        {
            skillEvent.OnSkillData_SlotOne -= OnSkillData_SlotOne;
            skillEvent.OnSkillCoolDown -= OnCoolDown;
        }

        OnDisableSkill();
    }

    public void OnSkillData_SlotOne(SkillData skillData)
    {
        if(skillData == null) 
            return;

       // this.skillData = skillData;
        icon_SkillImage.sprite = SkillDataManager.Instance.GetSkillSprite(skillData.name);
        icon_CoolDown.sprite = icon_SkillImage.sprite;
        icon_CoolDown.fillAmount = 0;

        OnEnableSkill();

        max_CoolDown = skillData.cooldown;
    }

    public void OnCoolDown(float coolTime)
    {
        icon_CoolDown.fillAmount = coolTime / max_CoolDown;
    }

    public void OnDisableSkill()
    {
        Color color1 = icon_CoolDown.color;
        color1.a = 0;
        icon_CoolDown.color = color1;
        Color color2 = icon_SkillImage.color;
        color2.a = 0;
        icon_SkillImage.color = color2;

        icon_SkillKeySlot.gameObject.SetActive(false);
    }

    public void OnEnableSkill()
    {
        Color color1 = icon_CoolDown.color;
        color1.a = 1;
        icon_CoolDown.color = color1;
        Color color2 = icon_SkillImage.color;
        color2.a = 1;
        icon_SkillImage.color = color2;
        icon_SkillKeySlot.gameObject.SetActive(true);
    }
}
