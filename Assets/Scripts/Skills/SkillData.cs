using UnityEngine;

[CreateAssetMenu(fileName = "NewSkill", menuName = "ScriptableObjects/Skill")]
public class SkillData : ScriptableObject
{
    public string skillName;                // ��ų �̸�
    public WeaponType weaponType; // ���� Ÿ��
    public float cooldown;                  // ��ٿ� �ð�
    public string animationName;            // ��ų �ִϸ��̼� �̸�
    //public float damage;                    // ��ų ������
    //public GameObject effectPrefab;         // ��ų ȿ�� ������
    //public float knockbackDistance;         // �˹� �Ÿ�
    //public float airborneHeight;            // ���߿� ���� ���� ��
    public DoActionData doAction;
    public Vector3 additionalPos;           // ����Ʈ ������ ���� ��ġ

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
