using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ScopeUI : MonoBehaviour
{
    [SerializeField] private RectTransform mainImage;
    [SerializeField] private RectTransform background;
    [SerializeField] private Image scopeUI;

    [SerializeField] private float reboundPosY = 5.0f;
    [SerializeField] private float reboundTime = 0.5f;
    private Vector2 originSize;
    private float originAlpha;

    private void Awake()
    {
        Awake_ResetUI();
    }

    private void Awake_ResetUI()
    {

        originSize = mainImage.sizeDelta;

        background.sizeDelta = new Vector2(Screen.width, Screen.height); 

        originAlpha = scopeUI.color.a;
        Color color = scopeUI.color;
        color.a = 0;
        scopeUI.color = color;
        scopeUI.gameObject.SetActive(false);
    }

    public void SetDrawSnipeUI()
    {
        StartCoroutine(Draw_SnipeUI());
    }

    public void EndDrawSnipeUI()
    {
        mainImage.gameObject.SetActive(false);
        scopeUI.gameObject.SetActive(false);
    }



    private IEnumerator Draw_SnipeUI()
    {
        //1. mainImage�� ��û Ŀ�� ���·� ����.
        Vector2 twoTerm = new Vector2(originSize.x * 2, originSize.y * 2);
        mainImage.sizeDelta = twoTerm;

        mainImage.gameObject.SetActive(true);

        yield return new WaitForSeconds(0.2f);

        float elasedTime = .0f;
        float duration = 0.25f; 

        // 2 ���� �پ��.
        while(elasedTime < duration)
        {
            elasedTime += Time.deltaTime;

            mainImage.sizeDelta = Vector2.Lerp(twoTerm, originSize, elasedTime / duration);

            yield return null; 
        }

        mainImage.sizeDelta = originSize;

        // 3. scope ui ����
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
        
    }


    public void Shoot_Snipe()
    {
        StartCoroutine(Vibreate_ShootDelay());
    }


    private IEnumerator Vibreate_ShootDelay()
    {
        // ���� �̵�
        float originX = mainImage.anchoredPosition.x;
        float originY = mainImage.anchoredPosition.y;

        float elapsedTime = 0.0f;
        float duration = reboundTime;
        Vector2 moveUp = Vector2.zero;

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

        //���� ����
        elapsedTime = 0;
        while(elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float lerpY = Mathf.Lerp(originY + reboundPosY, originY , elapsedTime / duration);
            moveUp = new Vector2(originX, lerpY);
            mainImage.anchoredPosition = moveUp;
            yield return new WaitForEndOfFrame();
        }

        mainImage.anchoredPosition = new Vector2(originX, originY);
    }
}
