using System;
using UnityEngine;

public class Skill : MonoBehaviour
{
    [SerializeField]
    protected SkillType type;

    [SerializeField]
    protected DoActionData doActionData;

    public SkillType Type { get => type; }

    protected GameObject rootObject;

    protected StateComponent state;
    protected Animator animator;
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

    public virtual void DoAction()
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

        Move();
    }


    public virtual void Play_Particle()
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
        if (doActionData.bCanMove == false)
        {
            PlayerMovingComponent moving = rootObject.GetComponent<PlayerMovingComponent>();

            if (moving != null)
                moving.Stop();
        }
    }
}
