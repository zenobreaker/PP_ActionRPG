using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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


    private bool bAppearFlag = false; 

    private int enemyCount;
    public void SetEnemyCount(int count)
    {
        enemyCount -= 1;
    }

    


    public event Action<GameObject> OnAppearBoss;
    public GameObject tempObj; 

    private void Start()
    {
        //cutscene.OnCutsceneBegin += OnCutSceneBegin;
        cutscene.OnCutSceneEnd += OnAppearEndBoss;
        cutscene.OnCutSceneEnd += OnBossSpawn;
        //cutscene.OnBossSpawn += OnBossSpawn;
    }

    private bool bSpawned = false;
    // 보스 스폰
    private AIController_Boss selctedBoss; 
    public void SpawnBoss(int index)
    {
        selctedBoss = null;
        GameObject obj = Instantiate<GameObject>(Boses[index], SpawnPoints[index].transform.position,
            Quaternion.Euler(0, 180.0f, 0));
        if (obj == null)
            return;

        bSpawned = true;
        //if (obj.TryGetComponent<AIController_Boss>(out AIController_Boss boss))
        //{
        //    boss.CanMove = false;
        //    selctedBoss = boss;
        //    OnAppearBoss?.Invoke(obj);
        //}

        SetBossAppear_Dragon(obj);
    }
       


    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") == false)
            return;
        if (bSpawned)
            return; 

        if( enemyCount <= 0)
        {
            //TODO: 한시적으로 고정값 전달 
            StartCoroutine(Spawn_Boss(0));
        }
    }

    private IEnumerator Spawn_Boss(int index)
    {

        // 컷씬에서 컨신을 실행 후에 보스를 등장시킨다.
        cutscene?.OnPlay();

        // 특정 플래그가 될 때까지 무한정 대기 
        while(true)
        {
            if (bAppearFlag == true)
                break; 

            yield return null; 
        }

   
        yield return new WaitForSeconds(0.5f);
        SpawnBoss(index);
    }

    private void OnBossSpawn()
    {
        bAppearFlag = true; 
    }

    private void OnAppearEndBoss()
    {
        if (selctedBoss == null)
            return; 

        if(selctedBoss != null)
            selctedBoss.CanMove = true;

        //TODO: 나중에 여길 수정해야 한다고 생각한다..
        {
            BossGaugeController.Instance.OnAppearGauge(selctedBoss.gameObject);
        }
    }

    public void EndBoss()
    {
        //if (tempObj != null)
        //    tempObj.gameObject.SetActive(true);

        cutscene?.SetPlayableData("Boss_End" , true);
    }


    private void SetBossAppear_Dragon(GameObject boss)
    {
        if (boss == null)
            return;
        if (!boss.TryGetComponent<BTAIController_Dragon>(out BTAIController_Dragon dragon))
            return;

        dragon.OnEnableAI();

    }
}
