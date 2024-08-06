using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public enum CharacterGrade
{
    Common = 0,
    Eleite = 1,
    Boss = 2,
    Max,
}


public class Enemy :
    Character,
    IDamagable
{
    [SerializeField]
    private Color damageColor = Color.red;

    [SerializeField]
    private float changeColorTime = 0.15f;

    private Color originColor;
    private Material skinMaterial;

    //private DetectComponent detect; 
    //private EnemyMovingComponent moving;

    private GroundedComponent ground;
    private LaunchComponent launch;
    private AIController aiController;

    public WeaponType weaponType;
    [SerializeField] private CharacterGrade grade = CharacterGrade.Common; 
    public CharacterGrade Grade { get => grade; }
    protected override void Awake()
    {
        base.Awake();

        Transform surface = transform.FindChildByName("Alpha_Surface");
        skinMaterial = surface.GetComponent<SkinnedMeshRenderer>().material;
        originColor = skinMaterial.color;

        ground = GetComponent<GroundedComponent>();
        Debug.Assert(ground != null);
        ground.OnCharacterGround += Begin_DownCondition;

        launch = GetComponent<LaunchComponent>();
        Debug.Assert(launch != null);

        Debug.Assert(weapon != null);

        // 무기 바로 장착시키는 이벤트 전달 
        weapon.OnEquipWeapon += StartEquipWeapon;

        aiController = GetComponent<AIController>();

    }

    protected override void Start()
    {
        base.Start();

        weapon.SetWeaponVisibleByType(weaponType, false);
    }

    protected virtual void OnDestroy()
    {
        if (grade == CharacterGrade.Boss)
        {
            BossGaugeController.Instance.OnDisappearGauge();
            BossStageManager.Instance.EndBoss();
        }
    }

    protected override void Update()
    {
        base.Update();
    }

    private void StartEquipWeapon()
    {
        switch (weaponType)
        {

            case WeaponType.Fist:
            weapon.SetFistMode();
            break;

            case WeaponType.Sword:
            weapon.SetSwordMode();
            break;
            case WeaponType.Hammer:
            weapon.SetHammerMode();
            break;

            case WeaponType.FireBall:
            weapon.SetFireBallMode();
            break;
            case WeaponType.Dual:
            weapon.SetDualMode();
            break;
            case WeaponType.Unarmed:
            weapon.SetUnarmedMode();
            break;

        }
    }

    private void RandAttack()
    {
        int random = Random.Range(0, 3);

        state.SetActionMode();
        animator.SetInteger("Sword_Combo_Index", random);
        animator.SetBool("IsAction", true);
    }

    public void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data)
    {
        healthPoint.Damage(data.Power);
        if (grade == CharacterGrade.Boss)
            BossGaugeController.Instance.SetGauge(healthPoint);

        StartCoroutine(Change_Color(changeColorTime));
        MovableStopper.Instance.Start_Delay(data.StopFrame);

        if (data.HitParticle != null)
        {
            GameObject obj = Instantiate<GameObject>(data.HitParticle, transform, false);
            obj.transform.localPosition = hitPoint + data.HitParticlePositionOffset;
            obj.transform.localScale = data.HitParticleSacleOffset;
        }

        weapon.End_Collision();

        if (healthPoint.Dead == false)
        {
            aiController?.SetDamagedMode();
            state.SetDamagedMode();
            launch.DoHit(attacker, causer, data, true,grade);
          
            // 다운 시키는 공격인가
            if (data.bDownable == false || grade == CharacterGrade.Boss)
            {
                bool bCheck = true;
                bCheck &= grade == CharacterGrade.Boss;
                bCheck &= (aiController != null && aiController.ActionMode);
                
                if (bCheck == false)
                {
                    // 아니라면 해당 피격 이벤트로 애니메이션 실행
                    animator.SetInteger("ImpactIndex", data.HitImpactIndex);
                    animator.SetTrigger("Impact");
                }
            }
            else
            {

                // 다운 상태 기술을 맞으면 관련된 값이 변경된다. 
                animator.SetBool("IsDownCondition", true);

                // 다운 상태가 아니면 다운 애니 실행
                if (animator.GetBool("IsDownCondition"))
                    animator.SetTrigger("Down_Trigger");

                if (downConditionCoroutine != null)
                    StopCoroutine(downConditionCoroutine);


            }

            return;
        }

        // Dead
        state.SetDeadMode();

        Collider collider = GetComponent<Collider>();
        collider.enabled = false;

        animator.SetTrigger("Dead");
        MovableStopper.Instance.Delete(this);
        BossStageManager.Instance.SetEnemyCount(1);
        Destroy(gameObject, 5);
    }



    private IEnumerator Start_Launch(int frame)
    {
        yield return null;
    }


    private IEnumerator Change_Color(float time)
    {
        skinMaterial.color = damageColor;

        yield return new WaitForSeconds(time);

        skinMaterial.color = originColor;
    }


    private IEnumerator Change_IsKinematics(int frame)
    {
        for (int i = 0; i < frame; i++)
            yield return new WaitForFixedUpdate();

        rigidbody.isKinematic = true;
    }


    protected override void End_Damaged()
    {
        base.End_Damaged();

        animator.SetInteger("ImpactIndex", 0);
        state.SetIdleMode();

        // ai야 너 스스로가 처리해
        aiController?.End_Damage();
    }

    protected override void Begin_DownCondition()
    {
        if (downConditionCoroutine != null)
            StopCoroutine(downConditionCoroutine);

        if (ground.IsGround == false)
            return;

        if (launch.IsAir == true)
            return;

        base.Begin_DownCondition();
    }

    
}
