using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookUpper : MonoBehaviour
{
    private Animator animator;
    private Transform playerSpine;

    private StateComponent state;
    private WeaponComponent weapon;
    private CameraArm arm;

    private void Start()
    {
        animator = GetComponent<Animator>();
        Debug.Assert(animator != null);
        playerSpine = animator.GetBoneTransform(HumanBodyBones.Spine);

        state = GetComponent<StateComponent>();
        weapon = GetComponent<WeaponComponent>();
        arm = FindObjectOfType<CameraArm>();
    }

    private void LateUpdate()
    {
        if (playerSpine == null || arm == null)
            return;

        if (weapon.GunMode)
        {
            if (weapon.GetEquippedWeapon() != null)
            {
                if(weapon.GetEquippedWeapon().SubAction)
                    playerSpine.localRotation = arm.GetRotation();
            }
        }
    }
}
