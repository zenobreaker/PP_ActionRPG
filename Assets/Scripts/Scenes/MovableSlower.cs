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
    }

    public static MovableSlower Instance { get => instance; }

    ///////////////////////////////////////////////////////////////////////////

    [SerializeField] private float duration = 2.0f;
    [SerializeField] private float slotFactor = 0.5f;       // ������ �ӵ� �� 
    [SerializeField] private float slowTimeCoolTime = 3.0f; // ���ο� Ÿ�� ��Ÿ�� 
    private bool bCanSlowTime = true;                       // ���ο� Ÿ�� ���� ����

    private List<ISlowable> slowers = new List<ISlowable>();

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
        slowers.ForEach(slower =>
        {
            // ���ο� Ÿ���� ����Ų ����� �� ���� Ǯ������ 
            if (causer != null)
            {
                if(slower == causer)
                {
                    float shortDuration = duration * 0.2f;
                    slower.ApplySlow(shortDuration, slotFactor);
                }
            }
            else 
                slower.ApplySlow(duration, slotFactor);
        });



        StartCoroutine(Delay_SlowTime());
    }

    private IEnumerator Delay_SlowTime()
    {
        yield return new WaitForSeconds(slowTimeCoolTime);
        bCanSlowTime = true; 
    }

}
