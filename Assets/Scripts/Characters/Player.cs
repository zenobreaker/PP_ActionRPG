using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.WSA;
using StateType = StateComponent.StateType;

[RequireComponent(typeof(PlayerInput))]
[RequireComponent(typeof(PlayerMovingComponent))]
[RequireComponent(typeof(ComboComponent))]
public class Player
    : Character,
    IDamagable
{

    private PlayerMovingComponent moving;
    private DashComponent dash;
    private GroundedComponent ground;
    private LaunchComponent launch;
    private ComboComponent combo;

    private Color originColor; 
    private Color targetColor;

    public event Action OnEvadeState;
    public event Action OnDamaged; 

    protected override void Awake()
    {
        base.Awake();
        moving = GetComponent<PlayerMovingComponent>();
        Debug.Assert(moving != null);

        launch = GetComponent<LaunchComponent>();
        Debug.Assert(launch != null);

        ground = GetComponent<GroundedComponent>();
        Debug.Assert(ground != null);
        combo = GetComponent<ComboComponent>();
        //sprint = GetComponent<SprintComponent>();

        PlayerInput input = GetComponent<PlayerInput>();
        InputActionMap actionMap = input.actions.FindActionMap("Player");



        actionMap.FindAction("Fist").started += (context) =>
        {
            weapon.SetFistMode();
        };

        actionMap.FindAction("Sword").started += (context) =>
        {
            weapon.SetSwordMode();
        };

        actionMap.FindAction("Hammer").started += (context) =>
        {
            weapon.SetHammerMode();
        };

        actionMap.FindAction("FireBall").started += (context) =>
        {
            weapon.SetFireBallMode();
        };

        actionMap.FindAction("Dual").started += (context) =>
        {
            weapon.SetDualMode();
        };

        actionMap.FindAction("Gun").started += (context) =>
        {
            weapon.SetGunMode();
        };


        actionMap.FindAction("Action").started += (context) =>
        {
            //weapon.DoAction();
            combo.InputCombo_Test(KeyCode.Mouse0);
        };

        actionMap.FindAction("Action2").started += (context) =>
        {
            weapon.DoSubAction();
        };

        actionMap.FindAction("Skill1").started += (context) =>
        {
            weapon.DoSkillAction("Skill1");
        };
        actionMap.FindAction("Skill2").started += (context) =>
        {
            weapon.DoSkillAction("Skill2");
        };

        actionMap.FindAction("Evade").started += (context) =>
        {
            if (state.IdleMode == false)
                return;

            state.SetEvadeMode();
        };

    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void OnAnimatorMove()
    {
        OnNonForwardAttackAnim();
    }

    private void OnNonForwardAttackAnim()
    {
        // -z�� �������� �ִٸ� 
        Vector3 position;
        position = animator.deltaPosition;
        position.y = 0.0f;


        if (state.ActionMode == true)
        {
            if (moving.InputMove.y < 0)
            {
                position = Vector3.zero;
            }
            //else if(moving.InputMove.y > 0)
            //{
            //    position = transform.forward * 1.0f * Time.deltaTime;
            //}
        }
        

        transform.position += (position);
    }


    public void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, ActionData data)
    {
        if (state.Type == StateType.Evade)
        {
            OnEvadeState?.Invoke();
            MovableSlower.Instance.Start_Slow(this);
            return;
        }

        OnDamaged?.Invoke();

        healthPoint.Damage(data.Power);

        //TODO: ���� ���ϴµ� �������� ���߸� ������?
        // MovableStopper.Instance.Start_Delay(data.StopFrame);


        if (data.HitParticle != null)
        {
            GameObject obj = Instantiate<GameObject>(data.HitParticle, transform, false);
            obj.transform.localPosition = hitPoint + data.HitParticlePositionOffset;
            obj.transform.localScale = data.HitParticleSacleOffset;
        }

        if (healthPoint.Dead == false)
        {
            state.SetDamagedMode();
            launch.DoHit(attacker, causer, data, false);

            // �ٿ� ��Ű�� �����ΰ�
            if (data.bDownable == false)
            {
                // �ƴ϶�� �ش� �ǰ� �̺�Ʈ�� �ִϸ��̼� ����
                animator.SetInteger("ImpactIndex", data.HitImpactIndex);
                animator.SetTrigger("Impact");
            }
            else
            {

                // �ٿ� ���� ����� ������ ���õ� ���� ����ȴ�. 
                animator.SetBool("IsDownCondition", true);

                // �ٿ� ���°� �ƴϸ� �ٿ� �ִ� ����
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
        MovableSlower.Instance.Delete(this);
        Destroy(gameObject, 5);

    }

    protected override void End_Damaged()
    {
        base.End_Damaged();

        state.SetIdleMode();
    }

}
