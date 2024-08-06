using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Draw_NoAnimation : StateMachineBehaviour
{
    private bool bFirstExecution;
    private WeaponComponent weapon;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        if (weapon == null)
            weapon = animator.gameObject.GetComponent<WeaponComponent>();

        bFirstExecution = true;

        // 같은 프레임에 시작과 끝을 주지 말것
        weapon.Begin_Equip();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        if (bFirstExecution == false)
            return;

        bFirstExecution = false;
        weapon.End_Equip();
    }

}
