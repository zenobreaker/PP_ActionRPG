using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharStateUI : MonoBehaviour
{
    [SerializeField] private string uiStateName = "EnemyAIState";
    [SerializeField] protected bool bDrawDebug = false;

    protected TextMeshProUGUI userInterface;
    protected Canvas uiStateCanvas;

    private ConditionComponent condition;
    private StateComponent state;

    private void Start()
    {
        uiStateCanvas = UIHelpers.CreateBillboardCanvas(uiStateName, transform, Camera.main);

        Transform t = uiStateCanvas.transform.FindChildByName("Txt_AIState");
        userInterface = t.GetComponent<TextMeshProUGUI>();
        userInterface.text = "";

        condition = GetComponent<ConditionComponent>();
        state = GetComponent<StateComponent>();    
    }

    private void LateUpdate()
    {
        if (bDrawDebug == false)
            return;

        if (state == null)
            return;

        if (uiStateCanvas == null)
            return;
        
        if (userInterface == null)
            return; 

        userInterface.gameObject.SetActive(bDrawDebug);

        userInterface.text = state.Type.ToString();
        userInterface.text += "\n" + condition.MyCondition.ToString();
        uiStateCanvas.transform.rotation = Camera.main.transform.rotation;
    }
}
