using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword_Finish_Air : StateMachineBehaviour
{

    private Sword sword;
    bool bFirst = false; 

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);


        GameObject gameObject = animator.gameObject;

        WeaponComponent weapon = gameObject.GetComponent<WeaponComponent>();
        if (weapon == null)
            return;

        sword = weapon.GetEquippedWeapon() as Sword;
        if (sword == null)
            return;

        bFirst = true; 
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);    

        if(stateInfo.normalizedTime > 0.32 && bFirst)
        {
            sword?.DoPlayDownFall();
            bFirst = false; 
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
