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
    private GameObject rifleHolster;
    private GameObject dualLeft;
    private GameObject dualRight;

    private float originAnimSpeed;
    private bool bStarward;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateEnter(animator, stateInfo, layerIndex);

        originAnimSpeed = animator.speed;

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
            bStarward = true;
            dual.OnStarward += () => { 
                animator.speed = originAnimSpeed;
                bStarward = false; 
            };
            models = gameObject.transform.FindChildrenByComponentType<SkinnedMeshRenderer>();
            //TODO: 임시로 이렇게 함..
            holster = gameObject.transform.FindChildByName("Holster_Sword").gameObject;
            rifleHolster = gameObject.transform.FindChildByName("Holster_Rifle").gameObject;
            dualLeft = gameObject.transform.FindChildByName("DualLeft").gameObject;
            dualRight = gameObject.transform.FindChildByName("DualRight").gameObject;
            break;
        }

    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        base.OnStateUpdate(animator, stateInfo, layerIndex);


        switch (skillName)
        {
            case "Starward":
            // 일정 구간에 도달했는지 검사 
            if(stateInfo.normalizedTime > 0.13f && stateInfo.normalizedTime < 0.31f &&
                bStarward)
            {
                foreach(var model in models)
                {
                    model.SetActive(false);
                }
                holster?.SetActive(false);
                rifleHolster?.SetActive(false);
                dualRight?.SetActive(false);
                dualLeft?.SetActive(false);
                animator.speed = 0.0f;
            }    
            else if(stateInfo.normalizedTime > 0.31f)
            {
                foreach (var model in models)
                {
                    model.SetActive(true);
                }
                holster?.SetActive(true);
                rifleHolster?.SetActive(true);
                dualRight?.SetActive(true);
                dualLeft?.SetActive(true);
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
        rifleHolster?.SetActive(true);
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
