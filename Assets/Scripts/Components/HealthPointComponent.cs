using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class HealthPointComponent : MonoBehaviour
{

    [SerializeField] private float maxHealthPoint = 100;
    private float currHealthPoint;


    [SerializeField] private string uiPlayerName = "Healthbar_Foreground";


    [SerializeField] private string uiEnemyName = "EnemyHealthBar";

    private Image userInterface;
    private Image delayGauge;
    [SerializeField] private float speed = 1.0f;
    private Canvas uiEnemyCanvas;

    public float GetMaxHP { get => maxHealthPoint; }
    public float GetCurrentHP { get => currHealthPoint; }
    public float GetCurrentHPByPercent { get => currHealthPoint / maxHealthPoint; }
    public bool Dead { get => currHealthPoint <= 0.0f; }

    private bool isEnemy;
    private bool isShow; 
    private float hiddenTime = 60.0f;

    private void Start()
    {
        currHealthPoint = maxHealthPoint;
        isShow = false; 

        if (GetComponent<Player>() != null)
        {
            GameObject ui = GameObject.Find(uiPlayerName);
            Debug.Assert(ui != null, gameObject.name + " error ");
            if (ui != null)
            {
                userInterface = ui.GetComponent<Image>();
                Debug.Assert(userInterface != null);
            }
        }
        else if (GetComponent<Enemy>() != null)
        {
            var enemy = GetComponent<Enemy>();
            if (enemy.Grade != CharacterGrade.Boss)
            {
                isEnemy = true;
                uiEnemyCanvas = UIHelpers.CreateBillboardCanvas(uiEnemyName, transform, Camera.main);
                uiEnemyCanvas.gameObject.SetActive(false);

                Transform t = uiEnemyCanvas.transform.FindChildByName("Image_Foreground");
                Transform t1 = uiEnemyCanvas.transform.FindChildByName("Image_Foreground_Lazy");
                userInterface = t.GetComponent<Image>();
                delayGauge = t1.GetComponent<Image>();
            }
        }

    }

    public void Damage(float amount)
    {
        if (amount < 1.0f)
            return;

        currHealthPoint += (amount * -1.0f);
        currHealthPoint = Mathf.Clamp(currHealthPoint, 0, maxHealthPoint);

        if (userInterface != null)
        {
            isShow = true;
            currentHiddentTime = hiddenTime;

            userInterface.fillAmount = currHealthPoint / maxHealthPoint;
            StartCoroutine(UpdateDelayGauge(currHealthPoint / maxHealthPoint));
            uiEnemyCanvas.gameObject.SetActive(true);
        }
    }

    private IEnumerator UpdateDelayGauge(float targetValue)
    {
        while(Mathf.Abs(delayGauge.fillAmount - targetValue) > 0.01f)
        {
            delayGauge.fillAmount = Mathf.Lerp(delayGauge.fillAmount, targetValue, Time.deltaTime * speed);

            yield return null;
        }

        delayGauge.fillAmount = targetValue;
    }

    private float currentHiddentTime = 0.0f;
    public void Update()
    {
        if (uiEnemyCanvas != null)
            uiEnemyCanvas.transform.rotation = Camera.main.transform.rotation;
        
        //TODO: 나중에 전투 비전투 상태에 따라서 HP를 끄도록 해야겠군 
        if (isEnemy)
        {
            if (isShow == false)
                return; 

            if(currentHiddentTime > 0.0f)
            {
                currentHiddentTime -= Time.deltaTime;
            }
            else
            {
                uiEnemyCanvas.gameObject.SetActive(false);
                isShow = false;
                currentHiddentTime = hiddenTime;
            }
        }
    }
}