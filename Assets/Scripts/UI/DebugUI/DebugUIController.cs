using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using static StateComponent;

public class DebugUIController : MonoBehaviour
{
    private Player player;
    private HealthPointComponent health;
    private StateComponent state;

    [Header("UI Base")]
    [SerializeField] private GameObject uiBaseObject; 

    [Header("Player State")]
    [SerializeField] private TextMeshProUGUI stateText;
    [Header("Player HP")]
    [SerializeField] private TextMeshProUGUI hpText;
    [Header("Player Damage Condition")]
    [SerializeField] private TextMeshProUGUI damageText;

    private bool bSuccessEvade;
    private bool bBeginEvade; 

    private void Awake()
    {
        player = FindObjectOfType<Player>();
        if (player != null)
        {
            health = player.GetComponent<HealthPointComponent>();
            state = player.GetComponent<StateComponent>();
            player.OnEvadeState += OnEvadeState;
            player.OnDamaged += OnDamaged;
            state.OnStateTypeChanged += OnStateTypeChanged;
        }
    }


    bool toggle = false; 
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad7))
            toggle =  !toggle; 

    }

    private void LateUpdate()
    {
        if(uiBaseObject != null)
            uiBaseObject.SetActive(toggle);

        LateUpdate_StateText();
        LateUpdate_PlayerHP();
        LateUpdate_Damage();
    }

    private void LateUpdate_StateText()
    {
        if (state == null)
            return;

        stateText.text = "Player State \n " + state.Type.ToString();
    }


    private void LateUpdate_PlayerHP()
    {
        if (health == null)
            return;

        hpText.text = "Player HP \n " + health.GetCurrentHP.ToString("f0");
    }

    private void LateUpdate_Damage()
    {
        if(bBeginEvade)
            damageText.text = "Evade...";
        else
            damageText.text = "...";

        if (bSuccessEvade)
            damageText.text = "Success!";
    }

    private void OnEvadeState()
    {
        bSuccessEvade = true;
    }

    private void OnDamaged()
    {
        if(bBeginEvade && bSuccessEvade == false)
        {
            damageText.text = "Damaged..!";
        }
    }

    private void OnStateTypeChanged(StateType prevType, StateType newType)
    {
        if (newType == StateType.Idle)
        {
            bBeginEvade = false;
            bSuccessEvade = false;
        }
        else
            bBeginEvade = true;
    }
}

