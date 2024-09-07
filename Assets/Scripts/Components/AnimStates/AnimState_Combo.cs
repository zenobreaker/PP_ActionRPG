using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimState_Combo : AnimState
{
    [SerializeField] private float BegeinActionTime;
    [SerializeField] private float EndActionTime; 

    private Weapon weapon;
    private bool bEnable = false; 
    protected override void Start()
    {
        base.Start();

        weapon = GetComponent<Weapon>();
        Debug.Assert(weapon != null);   
    }

    protected override void StateBegin()
    {
        base.StateBegin();

        Melee melee = weapon as Melee;
        melee?.Begin_Combo(); 
    }

    protected override void StateEnd()
    {
        base.StateEnd();

        Melee melee = weapon as Melee;
        melee?.End_Combo();
    }

    private void DoBeginCombo()
    {
        Melee melee = weapon as Melee;
        melee?.Begin_DoAction();
        Debug.Log("Call : DoBeginCombo");
    }

    private void DoEndCombo()
    {
        Melee melee = weapon as Melee;
        melee?.End_DoAction();
    }

    public void Start_State()
    {
        if(bEnable == false)
            StartCoroutine(StateCoroutine());
    }

    private IEnumerator StateCoroutine()
    {
        bEnable = true; 
        yield return new WaitForSeconds(stateBeginTime);

        StateBegin();

        yield return new WaitForSeconds(stateEndTime);

        bEnable = false; 
        StateEnd();

        StartCoroutine(DoBeginActionCoroutine());
    }

    private IEnumerator DoBeginActionCoroutine()
    {
        yield return new WaitForSeconds(BegeinActionTime);

        DoBeginCombo();

        yield return new WaitForSeconds(EndActionTime);

        DoEndCombo();
    }
}
