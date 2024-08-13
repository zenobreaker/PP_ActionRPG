using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Fist_Skill : StateMachineBehaviour
{
    private string skillName;
    private bool bFirstAction = false; 
    private Vector3 position; 

    private Fist fist;
    private GameObject target = null;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        GameObject gameObject = animator.gameObject;
        position = gameObject.transform.position;

        WeaponComponent weapon = gameObject.GetComponent<WeaponComponent>();
        if (weapon == null)
            return;

        fist = weapon.GetEquippedWeapon() as Fist;
        if (fist == null)
            return;

        SkillComponent skill = gameObject.GetComponent<SkillComponent>();
        if (skill == null)
            return;

        skillName = skill.CurrSkill.skillName;
        switch (skillName)
        {
            case "PowerSpike":
            target = fist.GetperceptFrontViewNearEnemy();
            break;
        }

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);

        if (fist == null)
            return; 

        switch(skillName)
        {
            case "PowerSpike":
            if (stateInfo.normalizedTime < 0.35f)
                return;

            if (bFirstAction == true)
                return; 

            if (target != null)
            {
                if (Physics.Linecast(position, target.transform.position, out RaycastHit hit))
                {
                    if (hit.transform.gameObject == target)
                    {
                        Debug.Log("대상 간에 장애물 없음 날아감!");
                        fist.Start_ApproachToTarget(target);
                        bFirstAction = true;
                        skillName = ""; 
                    }
                }
            }
            break;
        }

    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);
        bFirstAction = false;
        target = null;
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
