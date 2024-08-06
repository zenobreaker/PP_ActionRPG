using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossStageManager : MonoBehaviour
{
    private  static BossStageManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        DontDestroyOnLoad(gameObject);

        cutscene = FindAnyObjectByType<CutsenceController>();
    }

    public static BossStageManager Instance { get=>instance; }


    ///////////////////////////////////////////////////////////////////////////


    private CutsenceController cutscene;

    public GameObject[] Boses;
    public GameObject[] SpawnPoints;

    public int enemyCount;
    public void SetEnemyCount(int count)
    {
        enemyCount -= 1;
    }

    public event Action<GameObject> OnAppearBoss;
    public GameObject tempObj; 

    private void Start()
    {
        cutscene.OnCutSceneEnd += OnAppearEndBoss;
    }

    private bool bSpawned = false;
    // ���� ����
    private AIController_Boss selctedBoss; 
    public void SpawnBoss(int index)
    {
        selctedBoss = null;
        GameObject obj = Instantiate<GameObject>(Boses[index], SpawnPoints[index].transform.position,
            Quaternion.Euler(0, 180.0f, 0));
        if (obj == null)
            return;

        bSpawned = true; 
        if (obj.TryGetComponent<AIController_Boss>(out AIController_Boss boss))
        {
            boss.CanMove = false;
            selctedBoss = boss;
            OnAppearBoss?.Invoke(obj);
        }

    }
       


    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") == false)
            return;
        if (bSpawned)
            return; 

        if( enemyCount <= 0)
        {
            StartCoroutine(Spawn_Boss(0));
        }
    }

    private IEnumerator Spawn_Boss(int index)
    {
        yield return new WaitForSeconds(1.3f);

        SpawnBoss(index);

   
        yield return new WaitForSeconds(0.5f);
        cutscene.OnPlay();
    }

    private void OnAppearEndBoss()
    {
        if (selctedBoss == null)
            return; 

        if(selctedBoss != null)
            selctedBoss.CanMove = true;

        //TODO: ���߿� ���� �����ؾ� �Ѵٰ� �����Ѵ�..
        {
            BossGaugeController.Instance.OnAppearGauge(selctedBoss.gameObject);
        }
    }

    public void EndBoss()
    {
        //if (tempObj != null)
        //    tempObj.gameObject.SetActive(true);

        cutscene.SetPlayableData("Boss_End" , true);
    }
}
