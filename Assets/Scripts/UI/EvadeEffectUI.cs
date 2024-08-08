using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvadeEffectUI : MonoBehaviour
{
    private Image evadeImage;
    private Animator animator;

    private void Awake()
    {

        animator = GetComponent<Animator>();
        evadeImage = GetComponent<Image>();
    }

    public void OnEnable()
    {
        OnEffectImage();
    }

    public void OnEffectImage()
    {
        bool bCheck = true;
        bCheck &= evadeImage != null;
        bCheck &= animator != null;

        if (bCheck == false)
            return;

        animator.SetTrigger("Play");
    }


    public void End_EffectPlay()
    {
        this.gameObject.SetActive(false);
    }
}
