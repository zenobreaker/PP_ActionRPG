using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DoActionData
{
    public int dataID;

    public bool bCanMove;
    public bool bUpper = false; 
    public bool bDownable = false; 
    public bool bLauncher;  // 적을 날려버리는게 가능한지 여부 

    public float Power;     // 위력 
    public float Distance;  // 적 런치 길이 
    public float heightValue; // 공중에 띄우는 높이값 

    public int StopFrame;   // 히트스탑 프레임 
    public float airConditionTime; // 공중에 유지시키는 시간 

    [Header("Camera Shake")]
    public Vector3 impulseDirection;
    public Cinemachine.NoiseSettings impulseSettings;

    public GameObject Particle;
    public string effectSoundName; // 재생 시킬 사운드 

    public int HitImpactIndex; // 피격당했을 때 모션 인덱스
    public string hitSoundName;     // 피격 당했을 때 사운드 

    public GameObject HitParticle;
    public Vector3 HitParticlePositionOffset;
    public Vector3 HitParticleSacleOffset = Vector3.one;
}


public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected WeaponType type;
    [SerializeField] protected DoActionData[] doActionDatas;

    protected Dictionary<int,  DoActionData> doActionDataTable;

    public DoActionData GetCurrentData(int index)
    {
        return doActionDataTable[index];
    }

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

        doActionDataTable = new Dictionary<int, DoActionData>();
        foreach (DoActionData data in doActionDatas)
            doActionDataTable.Add(data.dataID, data);
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
        state.SetEquipMode(); // 상태 변경
    }

    public virtual void Begin_Equip()
    {

    }

    public virtual void End_Equip()
    {
        bEquipping = true; 
        state.SetIdleMode();
    }

    // 해제하는 자식에 따라 해제할 수 있으니 가상
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

        Begin_SkillAction();

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

    #region Skill_Action
    public virtual void Begin_SkillAction()
    {

    }

    public virtual void End_SkillAciton()
    {

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

    public bool IsDoesCombo(int index)
    {
        if(doActionDatas == null)
            return false;
        if(index >= doActionDatas.Length)
            return false;

        return true; 
    }

}

