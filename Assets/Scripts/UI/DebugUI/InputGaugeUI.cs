using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputGaugeUI : MonoBehaviour
{
    [SerializeField] private Image Gauge;
    [SerializeField] private TextMeshProUGUI GaugeText;

    private float maxValue;
    private float curValue; 

    private void Start()
    {
        
    }

    public void InitializeGauge(float value)
    {
        maxValue = value;
        curValue = value;
    }

    public void ResetValue()
    {
        curValue = maxValue;
    }

    // 계산된 값은 저쪽에서 받아낸다.
    public void SetValue(float value)
    {
        curValue = value; 
    }

    public void SetMaxValue(float value) => maxValue = value;

    private void LateUpdate()
    {
        Gauge.fillAmount = Mathf.Clamp01(curValue / maxValue);
        GaugeText.text = Mathf.Clamp01(curValue / maxValue).ToString("F1");
    }

}
