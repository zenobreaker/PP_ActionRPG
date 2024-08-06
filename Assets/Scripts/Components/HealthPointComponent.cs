using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class HealthPointComponent : MonoBehaviour
{

    [SerializeField] private float maxHealthPoint = 100;
    private float currHealthPoint;


    [SerializeField] private string uiPlayerName = "Image_Healthbar_Foreground";


    [SerializeField] private string uiEnemyName = "EnemyHealthBar";

    private Image userInterface;
    private Canvas uiEnemyCanvas;

    public float GetMaxHP { get => maxHealthPoint; }
    public float GetCurrentHP { get => currHealthPoint; }

    public bool Dead { get => currHealthPoint <= 0.0f; }

    private void Start()
    {
        currHealthPoint = maxHealthPoint;

        if (GetComponent<Player>() != null)
        {
            GameObject ui = GameObject.Find(uiPlayerName);
            Debug.Assert(ui != null);
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
                uiEnemyCanvas = UIHelpers.CreateBillboardCanvas(uiEnemyName, transform, Camera.main);

                Transform t = uiEnemyCanvas.transform.FindChildByName("Image_Foreground");
                userInterface = t.GetComponent<Image>();
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
            userInterface.fillAmount = currHealthPoint / maxHealthPoint;
    }

    public void Update()
    {
        // 빌보드 => 항상 카메라를 바라보게 하는 UI
        if (uiEnemyCanvas != null)
            uiEnemyCanvas.transform.rotation = Camera.main.transform.rotation;

    }
}