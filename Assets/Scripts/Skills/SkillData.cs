using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "ScriptableObjects/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;                // 스킬 이름
    public WeaponType weaponType; // 무기 타입
    public float cooldown;                  // 쿨다운 시간
    public string animationName;            // 스킬 애니메이션 이름
    //public float damage;                    // 스킬 데미지
    //public GameObject effectPrefab;         // 스킬 효과 프리팹
    //public float knockbackDistance;         // 넉백 거리
    //public float airborneHeight;            // 공중에 띄우는 높이 값
    public DoActionData doAction;
    public Vector3 additionalPos;           // 이펙트 프리팹 생성 위치

    public SkillData DeepCopy()
    {
        SkillData s = new SkillData();
        s.skillName = skillName;
        s.weaponType = weaponType;
        s.cooldown = cooldown;
        s.animationName = animationName;
        s.doAction = doAction.DeepCopy();
        s.additionalPos = additionalPos;
        return s;

    }
}
