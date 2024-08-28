using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScopeUI : MonoBehaviour
{
    [SerializeField] private RectTransform mainImage;
    [SerializeField] private RectTransform background;
    [SerializeField] private Image scopeUI;
    [SerializeField] private Image lockOnScopeImage;

    [SerializeField] private float reboundPosY = 5.0f;
    [SerializeField] private float reboundTime = 0.5f;
    private Vector2 originSize;
    private float originAlpha;

    [SerializeField] private GameObject ammoParent;
    [SerializeField] private AmmoUI ammo;
    private AmmoUI[] ammoUIs;
    private int maxAmmo;
    private int curr_Ammo;
    private bool bSnipe = false;

    private void Awake()
    {
        Awake_ResetUI();
    }

    private void Awake_ResetUI()
    {
        originSize = mainImage.sizeDelta;

        background.sizeDelta = new Vector2(Screen.width*1.5f, Screen.height *1.5f); 

        originAlpha = scopeUI.color.a;
        Color color = scopeUI.color;
        color.a = 0;
        scopeUI.color = color;
        scopeUI.gameObject.SetActive(false);
        
        lockOnScopeImage.gameObject.SetActive(false);
    }

    public void SetDrawSnipeUI(int ammoCount = 0)
    {
        StartCoroutine(Draw_SnipeUI());
        
        DrawRifleAmmo(ammoCount);
    }

    private void DrawRifleAmmo(int ammoCount = 0)
    {
        if (ammoParent == null)
            return;

        curr_Ammo = maxAmmo = ammoCount;
        ammoUIs ??= new AmmoUI[ammoCount];

        if (ammoParent.transform.childCount <= 0)
        {
            for (int i = 0; i < ammoCount; i++)
            {
                ammoUIs[i] = Instantiate<AmmoUI>(ammo, ammoParent.transform);
            }
        }
        else
        {
            for (int i = 0; i < ammoCount; i++)
                ammoUIs[i].Redraw();
        }
    }

    public void EndDrawSnipeUI()
    {
        mainImage.gameObject.SetActive(false);
        scopeUI.gameObject.SetActive(false);
        ammoParent.SetActive(false);

        bSnipe = false; 

        LockOff_AdditiveScope();
    }



    private IEnumerator Draw_SnipeUI()
    {
        //1. mainImage°¡ ¾öÃ» Ä¿Áø »óÅÂ·Î ÄÑÁü.
        Vector2 twoTerm = new Vector2(originSize.x * 2, originSize.y * 2);
        mainImage.sizeDelta = twoTerm;

        mainImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        float elasedTime = .0f;
        float duration = 0.25f; 

        // 2 Á¡Á¡ ÁÙ¾îµë.
        while(elasedTime < duration)
        {
            elasedTime += Time.deltaTime;

            mainImage.sizeDelta = Vector2.Lerp(twoTerm, originSize, elasedTime / duration);

            yield return null; 
        }

        mainImage.sizeDelta = originSize;

        // 3. scope ui ÄÑÁü
        scopeUI.gameObject.SetActive(true);
        float fadeDurtaion = 0.25f;
        Color color = scopeUI.color;
        for (float t = 0.01f; t < fadeDurtaion; t+=Time.deltaTime)
        {
            color.a = Mathf.Lerp(0, originAlpha, t/fadeDurtaion);
            scopeUI.color = color; 
            yield return null;
        }
        color.a = originAlpha; 
        scopeUI.color = color;

        // 4. ÃÑ¾Ë ÀÌ¹ÌÁö ÄÑÁü
        ammoParent.SetActive(true);

        bSnipe = true; 
    }


    public void Shoot_Snipe()
    {
        StartCoroutine(Vibreate_ShootDelay());

        Use_Ammo();
    }

    public void Use_Ammo()
    {
        if (curr_Ammo <= 0)
            return;
        int index = maxAmmo - (curr_Ammo--);
        ammoUIs[index].Use_Ammo();
    }

    private IEnumerator Vibreate_ShootDelay()
    {
        // À§·Î ÀÌµ¿
        float originX = mainImage.anchoredPosition.x;
        float originY = mainImage.anchoredPosition.y;

        float elapsedTime = 0.0f;
        float duration = reboundTime;
        Vector2 moveUp = Vector2.zero;
        
        bSnipe = false;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float lerpY = Mathf.Lerp(originY, originY + reboundPosY, elapsedTime / duration);
            moveUp = new Vector2(originX, lerpY);
            mainImage.anchoredPosition = moveUp;
            yield return new WaitForEndOfFrame();
        }
        moveUp = new Vector2(originX, originY + reboundPosY);
        mainImage.anchoredPosition = moveUp;

        //¿ø»ó º¹±Í
        elapsedTime = 0;
        while(elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float lerpY = Mathf.Lerp(originY + reboundPosY, originY , elapsedTime / duration);
            moveUp = new Vector2(originX, lerpY);
            mainImage.anchoredPosition = moveUp;
            yield return new WaitForEndOfFrame();
        }

        bSnipe = true; 
        mainImage.anchoredPosition = new Vector2(originX, originY);
    }



    public void LockOn_AdditiveScope()
    {
        if (bSnipe == false)
        {
            LockOff_AdditiveScope();
            return;
        }

        lockOnScopeImage.gameObject.SetActive(true);
    }

    public void LockOff_AdditiveScope()
    {
        lockOnScopeImage.gameObject.SetActive(false);
    }
}
