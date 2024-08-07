using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class MovableSlower : MonoBehaviour
{
    private static MovableSlower instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        DontDestroyOnLoad(gameObject);

        Awake_MainLight();
        Awake_SkyBoxMaterial();
    }

    public static MovableSlower Instance { get => instance; }

    ///////////////////////////////////////////////////////////////////////////

    // ���� 
    [SerializeField] private Light mainLight;
    private float originalIntensity;
    [SerializeField] private float tragetIntensity = 0.35f; 
    // ��ī�� �ڽ�
    [SerializeField] private Material originalSkybox;
    [SerializeField] private Material darkSkybox;


    [SerializeField] private float duration = 2.0f;
    [SerializeField] private float slotFactor = 0.5f;       // ������ �ӵ� �� 
    [SerializeField] private float slowTimeCoolTime = 3.0f; // ���ο� Ÿ�� ��Ÿ�� 
    private bool bCanSlowTime = true;                       // ���ο� Ÿ�� ���� ����

    private List<ISlowable> slowers = new List<ISlowable>();

    private void Awake_MainLight()
    {
        if(mainLight != null)
            originalIntensity = mainLight.intensity;
    }

    private void Awake_SkyBoxMaterial()
    {
        originalSkybox = RenderSettings.skybox;
    }

    public void Regist(ISlowable slower)
    {
        // �߰� ����ع�����?
        slowers.Add(slower);
    }

    public void Delete(ISlowable slower)
    {
        slowers.Remove(slower);
    }

    public void Start_Slow(ISlowable causer = null)
    {
        if (bCanSlowTime == false)
            return;

        bCanSlowTime = false;
        float shortDuration = duration * 0.2f;
        slowers.ForEach(slower =>
        {
            // ���ο� Ÿ���� ����Ų ����� �� ���� Ǯ������ 
            if (causer != null)
            {
                if (slower == causer)
                {
                    slower.ApplySlow(shortDuration, slotFactor);
                }
            }

            slower.ApplySlow(duration, slotFactor);
        });


        StartCoroutine(Delay_SlowTime());

        //Lighting
        {
            StartCoroutine(Adjusting_Lighting(shortDuration, tragetIntensity));
            StartCoroutine(Reset_Ligiting(duration));
        }

        // Skybox
        {
            AdjustingSkybox();
            StartCoroutine(Reset_Skybox(duration));
        }
    }

    private IEnumerator Delay_SlowTime()
    {
        yield return new WaitForSeconds(slowTimeCoolTime);
        bCanSlowTime = true;
    }

    #region Lighting
    private IEnumerator Adjusting_Lighting(float duration, float targetIntensity)
    {
        float elapsedTime = 0f;

        float currentIntesity = mainLight.intensity;

        while (elapsedTime < duration)
        {
            mainLight.intensity = Mathf.Lerp(currentIntesity, targetIntensity, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null; 
        }

        mainLight.intensity = targetIntensity;
    }

    private IEnumerator Reset_Ligiting(float duration)
    {
        yield return new WaitForSeconds(duration);
        StartCoroutine(Adjusting_Lighting(duration * 0.2f, originalIntensity));
    }
    #endregion

    #region Skybox
    private void AdjustingSkybox()
    {
        RenderSettings.skybox = darkSkybox;
    }
    
    private IEnumerator Reset_Skybox(float duration)
    {
        yield return new WaitForSeconds(duration);
        ResetSkybox();
    }

    private void ResetSkybox()
    {
        RenderSettings.skybox = originalSkybox;
    }

    #endregion

}
