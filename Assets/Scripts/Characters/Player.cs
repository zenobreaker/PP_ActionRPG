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


        actionMap.FindAction("Action").started += (context) =>
        {
            //weapon.DoAction();
            combo.InputCombo(KeyCode.Mouse0);
        };

        actionMap.FindAction("Action2").started += (context) =>
        {
            weapon.DoSubAction();
        };

        actionMap.FindAction("Evade").started += (context) =>
        {
            if (state.IdleMode == false)
                return;

            state.SetEvadeMode();
        };

    }


    protected override void OnAnimatorMove()
    {
        OnNonForwardAttackAnim();
    }

    private void OnNonForwardAttackAnim()
    {
        // -z로 눌러지고 있다면 
        Vector3 position;
        position = animator.deltaPosition;
        position.y = 0.0f;


        if (state.ActionMode == true)
        {
            if (moving.InputMove.y < 0)
            {
                position = Vector3.zero;
            }
            else if(moving.InputMove.y > 0)
            {
                position = transform.forward * 1.0f * Time.deltaTime;
            }
        }
        

        transform.position += (position);
    }


    public void OnDamage(GameObject attacker, Weapon causer, Vector3 hitPoint, DoActionData data)
    {
        if (state.Type == StateType.Evade)
        {
            OnEvadeState?.Invoke();
            return;
        }

        OnDamaged?.Invoke();

        healthPoint.Damage(data.Power);

        //TODO: 내가 당하는데 프레임이 멈추면 좋은가?
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

            // 다운 시키는 공격인가
            if (data.bDownable == false)
            {
                // 아니라면 해당 피격 이벤트로 애니메이션 실행
                animator.SetInteger("ImpactIndex", data.HitImpactIndex);
                animator.SetTrigger("Impact");
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
        Destroy(gameObject, 5);

    }

    protected override void End_Damaged()
    {
        base.End_Damaged();

        state.SetIdleMode();
    }
}
