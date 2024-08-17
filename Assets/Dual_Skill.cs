using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Dual_Skill : StateMachineBehaviour
{
    private string skillName;

    private Dual dual;
    private GameObject[] models;
    private GameObject holster; 

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        GameObject gameObject = animator.gameObject;

        WeaponComponent weapon = gameObject.GetComponent<WeaponComponent>();
        if (weapon == null)
            return;

        dual = weapon.GetEquippedWeapon() as Dual;
        if (dual == null)
            return;

        SkillComponent skill = gameObject.GetComponent<SkillComponent>();
        if (skill == null)
            return;

        skillName = skill.CurrSkill.skillName;
        switch(skillName)
        {
            case "Starward":
            models = gameObject.transform.FindChildrenByComponentType<SkinnedMeshRenderer>();
            //TODO: 이건 나중에 처리하자
            holster = gameObject.transform.FindChildByName("Holster_Sword").gameObject;
            break;
        }

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);


        switch (skillName)
        {
            case "Starward":
            // 캐릭터의 모델을 일시적으로 안보이게 한다. 
            if(stateInfo.normalizedTime > 0.1f && stateInfo.normalizedTime < 0.31f)
            {
                foreach(var model in models)
                {
                    model.SetActive(false);
                }
                holster?.SetActive(false);
            }    
            else if(stateInfo.normalizedTime > 0.31f)
            {
                foreach (var model in models)
                {
                    model.SetActive(true);
                }
                holster?.SetActive(true);
            }

            break;
        }
    }



    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateExit(animator, stateInfo, layerIndex);

        foreach (var model in models)
        {
            model.SetActive(true);
        }
        holster?.SetActive(true);
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
