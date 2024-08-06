using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(StateComponent))]
[RequireComponent(typeof(HealthPointComponent))]
[RequireComponent(typeof(WeaponComponent))]
[RequireComponent(typeof(Rigidbody))]

public abstract class Character 
    : MonoBehaviour,
    IStoppable
{
    //TODO: 이 값들은 따로 빼주기 
    private enum CharacterCondition
    {
        None, Down, Max, 
    }
    private CharacterCondition myCondition;

    public bool NoneCondition { get => myCondition == CharacterCondition.None; }
    public bool DownCondition { get => myCondition == CharacterCondition.Down; }
    public void SetNoneConditon() {  myCondition = CharacterCondition.None; }
    public void SetDownCondition() {  myCondition = CharacterCondition.Down; }

    protected Animator animator;
    protected new Rigidbody rigidbody;

    protected StateComponent state;
    protected HealthPointComponent healthPoint;
    protected WeaponComponent weapon;
   
    protected Coroutine downConditionCoroutine; 

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();

        state = GetComponent<StateComponent>();
        healthPoint = GetComponent<HealthPointComponent>();
        weapon = GetComponent<WeaponComponent>();
    }

    protected virtual void Start()
    {
        // 등록
        Regist_MovableStopper();
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


    
    protected virtual void Begin_DownCondition()
    {
        if (downConditionCoroutine != null)
            StopCoroutine(downConditionCoroutine);

        if (animator.GetBool("IsDownCondition") == false)
            return;

        SetDownCondition();
        downConditionCoroutine = StartCoroutine(Change_GetUpCondition());
    }


    // 일어나기 시작 
    protected IEnumerator Change_GetUpCondition()
    {
        //TODO: 기상 시간 값 설정하기 
        //yield return new WaitForSecondsRealtime(3.0f);
        yield return new WaitForSeconds(3.0f);
        Begin_GetUp();
    }

    protected virtual void Begin_GetUp()
    {
        if (state.DeadMode)
            return; 

        animator.SetBool("IsDownCondition", false);

        animator.SetTrigger("GetUp");

        //TODO: 일단 여기 호출
        SetNoneConditon();
    }

    
}
