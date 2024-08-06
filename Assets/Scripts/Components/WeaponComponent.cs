using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum WeaponType
{
    Unarmed = 0, Fist = 1, Sword, Hammer, FireBall, Dual, Warp, MAX,
}

public class WeaponComponent : MonoBehaviour
{
    [SerializeField] private GameObject[] originPrefabs;

    private StateComponent state;
    private TargetComponent target;

    protected Animator animator;

    private WeaponType type = WeaponType.Unarmed;
    public WeaponType Type { get => type; }

    private event Action<WeaponType, WeaponType> OnWeaponTypeChanged; // ���� ��ü �̺�Ʈ
    public event Action<SO_Combo> OnWeaponTypeChanged_Combo; 
    public event Action OnEquipWeapon;
    public event Action OnEndEquip;
    public event Action OnBeginDoAction;
    public event Action OnEndDoAction;

    public bool UnarmedMode { get => type == WeaponType.Unarmed; }
    public bool FistMode { get => type == WeaponType.Fist; }
    public bool SwordMode { get => type == WeaponType.Sword; }
    public bool HammerMode { get => type == WeaponType.Hammer; }
    public bool FireBallMode { get => type == WeaponType.FireBall; }
    public bool DualMode { get => type == WeaponType.Dual; }
    public bool WarpMode { get => type == WeaponType.Warp; }

    public bool IsEquippingMode()
    {
        if (UnarmedMode)
            return false;

        Weapon weapon = weaponTable[type];
        if (weapon == null)
            return false;

        return weapon.Equipping;
    }

    public Weapon GetEquippedWeapon()
    {
        return weaponTable[type];
    }

    public DoActionData GetCurrentData(int index)
    {
        if (UnarmedMode)
            return null;

        Weapon weapon = weaponTable[type];
        if(weapon == null) 
            return null;

        return weapon.GetCurrentData(index);
    }

    private Dictionary<WeaponType, Weapon> weaponTable;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        state = GetComponent<StateComponent>();
        target = GetComponent<TargetComponent>();

        Debug.Assert(state != null, $"{gameObject.name} has not");
        weaponTable = new Dictionary<WeaponType, Weapon>();
        // ���� ���̺� �ʱ�ȭ 
        for (int i = 0; i < (int)WeaponType.MAX; i++)
            weaponTable.Add((WeaponType)i, null);

        if (originPrefabs == null)
            return;

        for (int i = 0; i < originPrefabs.Length; i++)
        {
            GameObject obj = Instantiate<GameObject>(originPrefabs[i], transform);
            Weapon weapon = obj.GetComponent<Weapon>();
            obj.name = weapon.Type.ToString();
            //weapon?.PerpareWeapon();

            // ���� �ֱ� 
            weaponTable[weapon.Type] = weapon;
        }

    }


    public void SetWeaponVisibleByType(WeaponType type, bool bVisible)
    {
        foreach(var pair in weaponTable)
        {
            if (pair.Key == type)
                continue;

            if (pair.Value != null)
            {
                //pair.Value.Unequip();
                pair.Value?.gameObject.SetActive(bVisible);
            }
        }
    }

    public void SetFistMode()
    {
        if (state.IdleMode == false)
            return;

        SetMode(WeaponType.Fist);
    }

    // Į ����
    public void SetSwordMode()
    {
        if (state.IdleMode == false)
            return;

        SetMode(WeaponType.Sword);// Į ������ �Ŵ� ĮŸ���� �ش�
    }

    public void SetHammerMode()
    {
        if (state.IdleMode == false)
            return;

        SetMode(WeaponType.Hammer);
    }

    public void SetFireBallMode()
    {
        if (state.IdleMode == false)
            return;

        SetMode(WeaponType.FireBall);
    }


    public void SetDualMode()
    {
        if (state.IdleMode == false)
            return;

        SetMode(WeaponType.Dual);
    }



    public void SetUnarmedMode()
    {
        if (state.IdleMode == false)
            return;

        animator.SetInteger("WeaponType", (int)WeaponType.Unarmed);

        if (weaponTable[type] != null)
            weaponTable[(type)].Unequip();


        ChangeType(WeaponType.Unarmed);
    }

    private void SetMode(WeaponType type)
    {
        // ���� Ÿ���� ��������
        if (this.type == type)
        {
            SetUnarmedMode();

            return;
        }
        // �ٸ� ���⸦ ���� �ִٸ�
        else if (UnarmedMode == false)
        {
            // ��� ���� 
            weaponTable[this.type].Unequip();

        }
        // �����Ϸ��� ���Ⱑ ������ ����
        if (weaponTable[type] == null)
        {
            SetUnarmedMode();
            return;
        }

        animator.SetBool("IsEquipping", true);
        animator.SetInteger("WeaponType", (int)type);

        // ���� 
        weaponTable[type].Equip();

        ChangeType(type);
    }

    private void ChangeType(WeaponType type)
    {
        if (this.type == type)
            return;

        WeaponType prevType = this.type;
        this.type = type;

        OnWeaponTypeChanged?.Invoke(prevType, type);
        ChangeWeaponData(type);
    }

    private void ChangeWeaponData(WeaponType type)
    {
        Melee melee = weaponTable[type] as Melee;

        SO_Combo comboData = melee?.ComboObjData;
        OnWeaponTypeChanged_Combo?.Invoke(comboData);
    }


    // �̺�Ʈ �� ����� SendMessage ����̴� �̰� ���÷��� �̿��ϴϱ�..
    public void Begin_Equip()
    {
        weaponTable[type]?.Begin_Equip();
    }

    public void End_Equip()
    {
        animator.SetBool("IsEquipping", false);

        weaponTable[type]?.End_Equip();
        OnEndEquip?.Invoke();
    }

    public void DoIdleAction()
    {
        weaponTable[type].DoIdleAction();
    }

    public void DoAction()
    {
        if (weaponTable[type] == null)
            return;

        animator.SetBool("IsAction", true);

        weaponTable[type].DoAction();
    }


    public void DoAction(int comboIndex = 0, bool bNext = false)
    {
        if (weaponTable[type] == null)
            return;

        if (type == WeaponType.FireBall)
        {
            target.Begin_Targeting(true);
            animator.SetBool("IsAction", true);
        }

        weaponTable[type].DoAction(comboIndex, bNext);
    }

    public void DoSubAction()
    {
        if (weaponTable[type] == null)
            return;

        weaponTable[type].DoSubAction();
    }

    private void Begin_DoAction()
    {
        weaponTable[type].Begin_DoAction();
        //Debug.Log("Call first?");
        OnBeginDoAction?.Invoke();
    }

    public void End_DoAction()
    {
        animator.SetBool("IsAction", false);

        weaponTable[type]?.End_DoAction();
        OnEndDoAction?.Invoke();
    }

    private void Begin_Combo()
    {
        Melee melee = weaponTable[type] as Melee;

        melee?.Begin_Combo();
    }

    private void End_Combo()
    {
        Melee melee = weaponTable[type] as Melee;

        melee?.End_Combo();
    }

    public void Begin_Collision(AnimationEvent e)
    {
        Melee melee = weaponTable[type] as Melee;

        melee?.Begin_Collision(e);
    }

    public void End_Collision()
    {
        Melee melee = weaponTable[type] as Melee;

        melee?.End_Collision();
    }

    private void Play_DoAction_Particle(AnimationEvent e)
    {
        // ���� �´ٴ°� ���Ⱑ �̹� �� �̺�Ʈ�� ���� �ִٴ� �ǹ�
        weaponTable[type]?.Play_Particle(e);
    }

    private void Play_Sound()
    {
        weaponTable[type].Play_Sound();
    }

    private void Play_Impulse()
    {
        Melee melee = weaponTable[type] as Melee;

        melee?.Play_Impulse();
    }

    private void Begin_EnemyAttack(AnimationEvent e)
    {
        weaponTable[type].Begin_EnemyAttack(e);
    }


    public void SetWarpPosition(Vector3 position)
    {
        //Warp warp = weaponTable[type] as Warp;

        //if (warp == null)
        //    return;

        //warp.MoveToPosition = position;
    }
}
