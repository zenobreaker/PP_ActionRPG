using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGaugeController : MonoBehaviour
{
    private static BossGaugeController instance;

    private void Awake()
    {
        if(instance == null)
            instance = this;
    }    
    
    public static BossGaugeController Instance { get => instance; }


    ///////////////////////////////////////////////////////////////////////////

    [SerializeField] private Gauge bossGauge;

   // public event Action OnEndGauge; 
    public void SetGauge(HealthPointComponent health)
    {
        if (health == null)
            return;
        if (bossGauge == null)
            return;

        bossGauge.gameObject.SetActive(true);
        bossGauge.SetValues(health.GetCurrentHP, health.GetMaxHP);
    }

    public void OnAppearGauge(GameObject target)
    {
        if (bossGauge == null)
            return;
        if (target == null)
            return;

        
        HealthPointComponent bossHealth = target.GetComponent<HealthPointComponent>();
        if(bossHealth == null) 
            return;
        
        bossGauge.gameObject.SetActive(true);
        bossGauge.SetValues(bossHealth.GetCurrentHP, bossHealth.GetMaxHP);
    }

    public  void OnDisappearGauge()
    {
        bossGauge?.gameObject.SetActive(false);
    }


}
