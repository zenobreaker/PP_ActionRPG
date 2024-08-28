using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookUpper : MonoBehaviour
{
    private Animator animator;
    private Transform playerSpine;

    private WeaponComponent weapon;
    private CameraArm arm;

    private void Start()
    {
        animator = GetComponent<Animator>();
        Debug.Assert(animator != null);
        playerSpine = animator.GetBoneTransform(HumanBodyBones.Spine);

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
                Gun gun = weapon.GetEquippedWeapon() as Gun;
                if (gun != null)
                {
                    if (gun.Turn == false)
                        return; 

                    if (gun.SubAction)
                    {

                        // ī�޶��� ���� ���͸� �����ͼ� LookRotation ���
                        Vector3 forward = arm.GetRifleForward();

                        Quaternion spineTargetRotation = Quaternion.LookRotation(forward);

                        // ��ü�� ȸ���� �������� ��ü ȸ���� ����
                        Quaternion relativeRotation = Quaternion.Inverse(transform.rotation) * spineTargetRotation;
                        playerSpine.localRotation = relativeRotation;
                    }
                }
            }
        }
    }
}
