using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Evade_Animation : StateMachineBehaviour
{
    /// <summary>
    /// ���� �����ӿ� ���۰� ���� ���� �ʱ� ���� ���� 
    /// </summary>
    private bool bFirstExecution;
    //private SprintComponent sprint;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

       

        bFirstExecution = true; 


    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        if (bFirstExecution == false)
            return; 

        bFirstExecution = false;
    }

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
