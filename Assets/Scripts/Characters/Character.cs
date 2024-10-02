using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(ConditionComponent))]
[RequireComponent(typeof(StateComponent))]
[RequireComponent(typeof(HealthPointComponent))]
[RequireComponent(typeof(Rigidbody))]

public abstract class Character
    : MonoBehaviour,
    IStoppable,
    ISlowable
{

    protected Animator animator;
    protected new Rigidbody rigidbody;

    protected ConditionComponent condition;
    protected AirborneComponent airborne;
    protected GroundedComponent ground;
    protected StateComponent state;
    protected HealthPointComponent healthPoint;
    protected ActionComponent action;

    protected Coroutine downConditionCoroutine;

    private float originAnimSpeed;

    protected static readonly int HitImapact = Animator.StringToHash("Impact");
    protected static readonly int HitIndex = Animator.StringToHash("ImpactIndex");
    protected static readonly int DownTirgger = Animator.StringToHash("Down_Trigger");
    protected static readonly int IsDownCondition = Animator.StringToHash("IsDownCondition");
    protected static readonly int DeadTrigger = Animator.StringToHash("Dead");

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        Debug.Assert(animator != null);
        originAnimSpeed = animator.speed;

        airborne = GetComponent<AirborneComponent>();
        Debug.Assert(airborne != null);
        ground = GetComponent<GroundedComponent>();
        Debug.Assert(ground != null);
        ground.OnChangedGorund += OnChangedGround;

        rigidbody = GetComponent<Rigidbody>();
        condition = GetComponent<ConditionComponent>();
        state = GetComponent<StateComponent>();
        healthPoint = GetComponent<HealthPointComponent>();
        action = GetComponent<ActionComponent>();
    }

    protected virtual void Start()
    {
        Regist_MovableStopper();
        Regist_MovableSlower();
    }

    protected virtual void Update()
    {

    }

    protected virtual void FixedUpdate()
    {

    }
    protected virtual void End_Damaged()
    {

    }


    protected virtual void OnAnimatorMove()
    {
        transform.position += animator.deltaPosition;
        //transform.rotation *= animator.deltaRotation;
    }

    public void Regist_MovableStopper()
    {
        MovableStopper.Instance.Regist(this);
    }

    public IEnumerator Start_FrameDelay(int frame)
    {
        animator.speed = 0.0f;

        for (int i = 0; i < frame; i++)
            yield return new WaitForFixedUpdate();

        animator.speed = 1.0f;
    }

    protected virtual void OnChangedGround()
    {
        End_DownCondition();
    }

    protected virtual void DownDamaged()
    {
        if (condition.DownCondition == false)
            return;

        animator.SetTrigger(HitImapact);

        End_DownCondition();
    }
    

    protected virtual void Begin_DownImpact()
    {
        if (condition == null)
            return;


        Begin_DownCondition();

        animator.SetTrigger(DownTirgger);
        animator.SetBool(IsDownCondition, true);
    }

    protected virtual void Begin_DownCondition()
    {
        if (condition == null)
            return;

        if (condition.DownCondition)
            return;

        if (downConditionCoroutine != null)
            StopCoroutine(downConditionCoroutine);

        condition.SetDownCondition();
        state.SetIdleMode();
    }

    protected virtual void End_DownCondition()
    {
        // 공중 상태이면 일어나지 않는다.
        if (airborne != null && airborne.AirCondition)
            return;

        if (downConditionCoroutine != null)
            StopCoroutine(downConditionCoroutine);
        downConditionCoroutine = StartCoroutine(Change_GetUpCondition());
    }


    private void Regist_MovableSlower()
    {
        MovableSlower.Instance.Regist(this);
    }

    protected IEnumerator Change_GetUpCondition()
    {
        yield return new WaitForSeconds(1.5f);

        Begin_GetUp();
    }

    protected virtual void Begin_GetUp()
    {
        if (state != null && state.DeadMode)
            return;
        if (condition == null)
            return;


        condition.SetNoneConditon();
        animator.SetBool(IsDownCondition, false);
        animator.SetTrigger("GetUp");
    }

    public virtual void ApplySlow(float duration, float slowFactor)
    {
        animator.speed = originAnimSpeed * slowFactor;
        StopCoroutine(ResetSpeedAfterDelay(duration));
        StartCoroutine(ResetSpeedAfterDelay(duration));
    }

    public virtual void ResetSpeed()
    {
        animator.speed = originAnimSpeed;
    }

    public IEnumerator ResetSpeedAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        ResetSpeed();
    }
}
