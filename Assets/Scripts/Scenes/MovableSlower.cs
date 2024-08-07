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
    [SerializeField] private float slotFactor = 0.5f;       // 늦춰질 속도 값 
    [SerializeField] private float slowTimeCoolTime = 3.0f; // 슬로우 타임 쿨타임 
    private bool bCanSlowTime = true;                       // 슬로우 타임 가능 여부

    private List<ISlowable> slowers = new List<ISlowable>();

    public void Regist(ISlowable slower)
    {
        // 추가 등록해버리나?
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
            // 슬로우 타임을 일으킨 대상은 더 빨리 풀리도록 
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
