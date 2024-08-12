using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DoActionData
{
    public bool bCanMove;
    public bool bDownable = false;
    public bool bLauncher;  // ���� ���������°� �������� ���� 

    public float Power;     // ���� 
    public float Distance;  // �� ��ġ ���� 
    public float heightValue; // ���߿� ���� ���̰� 

    public int StopFrame;   // ��Ʈ��ž ������ 
    public float airConditionTime; // ���߿� ������Ű�� �ð� 

    [Header("Camera Shake")]
    public Vector3 impulseDirection;
    public Cinemachine.NoiseSettings impulseSettings;

    public GameObject Particle;
    public string effectSoundName; // ��� ��ų ���� 

    public int HitImpactIndex; // �ǰݴ����� �� ��� �ε���
    public string hitSoundName;     // �ǰ� ������ �� ���� 

    public GameObject HitParticle;
    public Vector3 HitParticlePositionOffset;
    public Vector3 HitParticleSacleOffset = Vector3.one;

    public DoActionData DeepCopy()
    {
        DoActionData doAction = new DoActionData();
        doAction.bCanMove = bCanMove;
        doAction.bDownable = bDownable;
        doAction.bLauncher = bLauncher;
        
        doAction.Power = Power;
        doAction.Distance = Distance;
        doAction.heightValue = heightValue;
        
        doAction.StopFrame = StopFrame;
        
        doAction.Particle = Particle;
        doAction.effectSoundName = effectSoundName;
        
        doAction.impulseDirection = impulseDirection;
        doAction.impulseSettings = impulseSettings;
        
        doAction.HitImpactIndex = HitImpactIndex;
        doAction.hitSoundName = hitSoundName;
        doAction.HitParticle = HitParticle;
        doAction.HitParticlePositionOffset = HitParticlePositionOffset;
        doAction.HitParticleSacleOffset = HitParticleSacleOffset;

        return doAction;
    }
}


public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected WeaponType type;
    [SerializeField] protected DoActionData[] doActionDatas;

    public WeaponType Type { get => type; }

    private bool bEquipping;
    public bool Equipping { get => bEquipping; }
    protected int currentComboCount = 0;

    protected GameObject rootObject;
    protected StateComponent state;
    protected Animator animator;

    protected static readonly int SkillNumberHash = Animator.StringToHash("SkillNumber");
    protected static readonly int SkillActionHash = Animator.StringToHash("SkillAction");

    protected virtual void Reset()
    {

    }

    protected virtual void Awake()
    {
        rootObject = transform.root.gameObject;
        Debug.Assert(rootObject != null);

        state = rootObject.GetComponent<StateComponent>();
        animator = rootObject.GetComponent<Animator>();
    }

    protected virtual void Start()
    {

    }

    protected virtual void Update()
    {

    }

    public virtual void SetVisibleWeapon(bool bVisible)
    {
        this.gameObject.SetActive(bVisible);
    }

    public void Equip()
    {
        state.SetEquipMode(); // ���� ����
    }

    public virtual void Begin_Equip()
    {

    }

    public virtual void End_Equip()
    {
        bEquipping = true;
        state.SetIdleMode();
    }

    // �����ϴ� �ڽĿ� ���� ������ �� ������ ����
    public virtual void Unequip()
    {
        bEquipping = false;
    }


    public void DoIdleAction()
    {
        animator.Play($"{type}.Blend Tree", 0);
    }

    public virtual void DoAction()
    {
        state.SetActionMode();

        CheckStop(0);
    }

    public virtual void DoAction(int comboIndex = 0, bool bNext = false)
    {
        state.SetActionMode();

        CheckStop(0);
    }

    public virtual void DoSubAction()
    {
        state.SetActionMode();

        CheckStop(0);
    }

    public virtual void Begin_DoAction()
    {

    }


    public virtual void End_DoAction()
    {
        state.SetIdleMode();
        currentComboCount = 0;
        Move();
    }

    public virtual void Play_Impulse()
    {

    }

    #region Skill_Action

    public virtual void DoSkillAction(SkillData skill)
    {
        // �ִϸ��̼� ���
        if (!string.IsNullOrEmpty(skill.animationName))
        {
            // �ִϸ��̼� ��� ����
            animator.Play(skill.animationName);
            CheckStop(0, skill);
        }
    }

    public virtual void Begin_SkillAction(SkillData currentSkill)
    {
        CreateSkillEffect(currentSkill);
    }

    public virtual void End_SkillAciton()
    {

    }

    private void CreateSkillEffect(SkillData currentSkill)
    {
        if (rootObject == null)
            return;

        if (currentSkill == null)
            return;

        DoActionData aData = currentSkill.doAction;
        if (aData == null)
            return;

        if (aData.Particle == null)
            return;

        Vector3 position = transform.position + currentSkill.additionalPos;
        GameObject obj = Instantiate<GameObject>(aData.Particle, position, rootObject.transform.rotation);
        if (obj.TryGetComponent<Skill_Trigger>(out Skill_Trigger trigger))
        {
            trigger.SetRootObject(rootObject);
            trigger.SetSkillData(currentSkill.DeepCopy());
            trigger.OnSkillHit += OnSkillHit;
        }
    }

    private void OnSkillHit(Collider other, SkillData skillData)
    {
        Debug.Log("Skill hit!");

        if (skillData == null)
        {
            Debug.Log("No skill data");
            return;
        }

        IDamagable damage = other.GetComponent<IDamagable>();

        if (damage != null)
        {
            Vector3 hitPoint = Vector3.zero;

            // ���� �� ��ǥ 
            hitPoint = other.ClosestPoint(other.transform.position);
            // ���࿭ ���ؼ� ���� �� ��ǥ�� �Ұ��ؼ� ������ǥ�� ��ȯ
            hitPoint = other.transform.InverseTransformPoint(hitPoint);

            damage?.OnDamage(transform.gameObject, this, hitPoint, skillData.doAction);
            // hit Sound Play
            SoundManager.Instance.PlaySFX(skillData.doAction.hitSoundName);

            Play_Impulse();

            return;
        }

    }
    #endregion

    public virtual void Play_Particle(AnimationEvent e)
    {

    }

    public virtual void Play_Sound()
    {

    }

    public virtual void Begin_EnemyAttack(AnimationEvent e)
    {

    }

    protected void Move()
    {
        PlayerMovingComponent moving = rootObject.GetComponent<PlayerMovingComponent>();

        if (moving != null)
            moving.Move();


    }

    protected void CheckStop(int index)
    {
        if (doActionDatas[index].bCanMove == false)
        {
            PlayerMovingComponent moving = rootObject.GetComponent<PlayerMovingComponent>();

            if (moving != null)
                moving.Stop();
        }
    }
    protected void CheckStop(int index, SkillData skill)
    {
        if (skill == null)
            return;

        if (skill.doAction.bCanMove == false)
        {
            PlayerMovingComponent moving = rootObject.GetComponent<PlayerMovingComponent>();

            if (moving != null)
                moving.Stop();
        }
    }

    public bool IsDoesCombo(int index)
    {
        if (doActionDatas == null)
            return false;
        if (index >= doActionDatas.Length)
            return false;

        return true;
    }



    public virtual void EndSkillAction()
    {
        Move();
    }



}

