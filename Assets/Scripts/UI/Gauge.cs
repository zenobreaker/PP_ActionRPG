using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gauge : MonoBehaviour
{
    [SerializeField] private string hpbarImageName = "BossGauge_HP";
    [SerializeField] private string afterbarImageName = "BossGauge_HPAfter";

    [SerializeField] private Image hpbarImage;
    [SerializeField] private Image afterHpbar;
    [SerializeField] private Text statText = null;

    [SerializeField] private float lerpSpeed = 10.0f;

    private float currentValue;
    private float maxValue;

    public float goalAmount;

    private Coroutine coroutineAfterGauge;

    private void Awake()
    {
        hpbarImage = transform.FindChildByName(hpbarImageName).GetComponent<Image>();
        Debug.Assert(hpbarImage != null);
        afterHpbar = transform.FindChildByName(afterbarImageName).GetComponent<Image>();
        Debug.Assert(afterHpbar != null);
    }

   
    private IEnumerator AfterHpDown()
    {
        while (goalAmount < afterHpbar.fillAmount)
        {
            if (afterHpbar != null)
            {
                afterHpbar.fillAmount = Mathf.Lerp(afterHpbar.fillAmount, goalAmount, lerpSpeed * Time.deltaTime);
            }

            if (MathHelpers.IsNearlyEqual(afterHpbar.fillAmount, goalAmount, 0.001f))
            {
                afterHpbar.fillAmount = goalAmount;
                yield break;
            }

            yield return null;
        }
    }


    public void SetValues(float currentValue, float maxValue)
    {
        this.currentValue = currentValue;
        this.maxValue = maxValue;
        goalAmount = currentValue / maxValue;

        Debug.Log($"{currentValue} / {maxValue} / {goalAmount}");
        //hpbarImage.fillAmount = Mathf.Lerp(hpbarImage.fillAmount, goalAmount, Time.deltaTime * lerpSpeed);
        if (currentValue <= 0)
            hpbarImage.fillAmount = 0.0f;
        else 
            hpbarImage.fillAmount = goalAmount;

        if (coroutineAfterGauge != null)
        {
            StopCoroutine(coroutineAfterGauge);
        }
        coroutineAfterGauge = StartCoroutine(AfterHpDown());
    }
}
