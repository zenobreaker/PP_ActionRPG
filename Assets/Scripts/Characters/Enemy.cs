using System;
using System.Collections;
using Unity.VisualScripting;
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

    [SerializeField] private string surfaceName;

    private Color originColor;
    private Material skinMaterial;

    private LaunchComponent launch;
    
    private AIController aiController;
    private BTAIController bTAIController;

    public WeaponType weaponType;
    [SerializeField] private CharacterGrade grade = CharacterGrade.Common;
    public CharacterGrade Grade { get => grade; }
    public bool isLaunchable = true;
    private ICollisionHandler collisionHandler;

    protected override void Awake()
    {
        base.Awake();

        Transform surface = transform.FindChildByName(surfaceName);
        skinMaterial = surface.GetComponent<SkinnedMeshRenderer>().material;
        originColor = skinMaterial.color;

        launch = GetComponent<LaunchComponent>();
        Debug.Assert(launch != null);

        aiController = GetComponent<AIController>();
        bTAIController = GetComponent<BTAIController>();

        collisionHandler = GetComponent<ICollisionHandler>();
    }

    protected override void Start()
    {
        base.Start();
        
        if(action is WeaponComponent weapon)
            weapon.SetWeaponVisibleByType(weaponType, false);
    }

    protected virtual void OnDisable()
    {
        if (grade == CharacterGrade.Boss)
        {
            if(BossGaugeController.Instance != null)
                BossGaugeController.Instance.OnDisappearGauge();
            if(BossStageManager.Instance != null)
                BossStageManager.Instance.EndBoss();
        }

    }

    protected virtual void OnDestroy()
    {
    }

    protected override void Update()
    {
        base.Update();
    }

    private void StartEquipWeapon()
    {
        if (action is WeaponComponent weapon)
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
    }


    public void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, ActionData data)
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

        // 공격 중에 맞았다면 
        collisionHandler?.End_Collision();

        if (healthPoint.Dead == false)
        {
            aiController?.SetDamagedMode();
            bTAIController?.SetDamagedMode();
            if(grade != CharacterGrade.Boss)
                state.SetDamagedMode();
            
            if(isLaunchable)
                launch?.DoHit(attacker, causer, data, true, grade);

            // 다운 시키는 공격인가
            if (data.bDownable == false || grade == CharacterGrade.Boss)
            {
                bool bCheck = true;
                bCheck &= grade == CharacterGrade.Boss;

                if (bCheck == false)
                {
                    DownDamaged();

                    // 아니라면 해당 피격 이벤트로 애니메이션 실행
                    animator.SetInteger(HitIndex, data.HitImpactIndex);
                    animator.SetTrigger(HitImapact);
                }
            }
            else
                Begin_DownImpact();

            return;
        }

        // Dead
        state.SetDeadMode();

        Collider collider = GetComponent<Collider>();
        collider.enabled = false;

        if (!condition.DownCondition)
            animator.SetTrigger(DeadTrigger);

        MovableStopper.Instance.Delete(this);
        MovableSlower.Instance.Delete(this);
        //BossStageManager.Instance.SetEnemyCount(1);
        Destroy(gameObject, 5);
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

        animator.SetInteger(HitIndex, 0);

        if (ground != null)
        {
            if (ground.IsGround)
                state.SetIdleMode();
            else
                condition.SetAirborneCondition();
        }
        else
        {
            state.SetIdleMode();
        }

        // ai야 너 스스로가 처리해
        aiController?.End_Damage();
        bTAIController?.End_Damage();
    }


    public override void ApplySlow(float duration, float slowFactor)
    {
        base.ApplySlow(duration, slowFactor);
        aiController?.Slow_NavMeshSpeed(slowFactor);
        bTAIController?.Slow_NavMeshSpeed(slowFactor);
    }

    public override void ResetSpeed()
    {
        base.ResetSpeed();
        aiController?.Reset_NavMeshSpeed();
        bTAIController?.Reset_NavMeshSpeed();
    }
}
