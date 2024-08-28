using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AmmoUI : MonoBehaviour
{
    [SerializeField] private GameObject emptyCatridgeImage;
    [SerializeField] private Image ammoImage;
    [SerializeField] private Sprite ammoSprite; 
    [SerializeField] private Animator animator;

    private Color originColor; 
    private void Awake()
    {
        originColor = ammoImage.color; 

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }    

    public void Use_Ammo()
    {
        animator.SetBool("IsAction", true);
    }

    public void Redraw()
    {
        ammoImage.color = originColor;
        ammoImage.sprite = ammoSprite;
        ammoImage.enabled = true;
        animator.SetBool("IsAction", false);
    }
}
