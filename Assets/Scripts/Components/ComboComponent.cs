using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ComboComponent : MonoBehaviour
{
    #region UI
    [SerializeField] GameObject comboInputUIPrefab;
    [SerializeField] GameObject comboInputBase;
    [SerializeField] Image comboInputTimingGauge;
    [SerializeField] bool bDebugDraw = false;

    private string comboInputUIName = "Input_Action";
    private string comboTimingGaugeName = "InputTimingGauge";
    private string inputBaseName = "InputComboBase";
    private string canvasName = "UserUI";
    private Canvas uiCanvas;

    #endregion

    class InputElement
    {
        public string InputType; // 입력 타입
        public float TimeStamp;  // 입력이 발새한 시간
        public bool bNext;
    }

    private Queue<KeyCode> uiQueue;
    private Queue<InputElement> inputQueue;
    [SerializeField] private float inputLimitTime = 1.0f; // 입력 제한 시간 
    private float currLimitTime;
    private bool bEnable = true;  // 입력 가능 여부 
    private bool bExist = false;  // 입력한 다음 내용이 존재하는가에 대한 여부
    private bool bNextable = false; // 다음 콤보 여부 

    private WeaponComponent weapon;
    private SO_Combo currComboObj;

    private Coroutine comboableCoroutine;

    private void Awake()
    {
        weapon = GetComponent<WeaponComponent>();
        Debug.Assert(weapon != null);
        weapon.OnWeaponTypeChanged_Combo += OnWeaponTypeChanged_Combo;
        weapon.OnBeginDoAction += OnBeginDoAction;
        weapon.OnEndDoAction += OnEndDoAction;


        uiQueue = new Queue<KeyCode>();
        inputQueue = new Queue<InputElement>();

        uiCanvas = GameObject.Find(canvasName).GetComponent<Canvas>();
        Debug.Assert(uiCanvas != null);
        comboInputUIPrefab = Resources.Load<GameObject>(comboInputUIName);
        comboInputBase = uiCanvas.transform.FindChildByName(inputBaseName).gameObject;
        comboInputTimingGauge = uiCanvas.transform.FindChildByName(comboTimingGaugeName).GetComponent<Image>();
    }

    private void OnWeaponTypeChanged_Combo(SO_Combo comboData)
    {
        if (comboData == null)
            return;

        currComboObj = comboData;
        
        ResetCombo();
    }


    #region UI Draw & Create & Destroy
    private void Create_ComboUI()
    {
        if (comboInputUIPrefab == null || bDebugDraw == false)
            return;

        // UI Input
        Instantiate(comboInputUIPrefab, comboInputBase.transform);
    }
    private void DestroyComboUIObjs()
    {
        if (comboInputBase == null)
            return;

        for (int i = 0; i < comboInputBase.transform.childCount; i++)
            Destroy(comboInputBase.transform.GetChild(i).gameObject);
    }

    private void DrawInputGauge()
    {
        if (bDebugDraw == false)
            return;


        comboInputTimingGauge.fillAmount = currLimitTime / inputLimitTime;
    }

    #endregion


    private void Update()
    {
        if (currLimitTime > 0)
            currLimitTime -= Time.deltaTime;
    }

    private void Test_PrintQueue()
    {
        string str = "Queue List : ";
        foreach (KeyCode keyCode in uiQueue)
        {
            str += keyCode.ToString() + " ";
        }

        Debug.Log($"{str}");
    }

    private void ExecuteAttack(ref InputElement inputElement)
    {
        bNextable = false;
        if (inputElement.bNext)
            bNextable = true;
        
        if (currComboObj == null)
            return;

        ComboData data = currComboObj.GetComboDataByRewind(inputQueue.Count);

        //currLimitTime = 0.5f; 
        currLimitTime = data.comboInputLimitTime;
        inputLimitTime = data.comboNextInputLimitTime;
        bExist = false;
        weapon.DoAction(bNextable);
    }

    private void ExecuteNextAttackFromQueue()
    {
        if(inputQueue.Count < 1) 
            return;

        var inputData = inputQueue.Dequeue();
        ExecuteAttack(ref inputData);
    }
    
    private void SetInputTime()
    {

    }

    public void InputCombo_Test(KeyCode keycode)
    {
        //TODO: 입력 큐 조건 변경 공격중일 때 큐에 넣고 입력받으면 실행하고..

        // 입력 제한 시간 관리 
        if (comboableCoroutine != null)
            comboableCoroutine = StartCoroutine(ComboableCoroutine());

        // 큐에 있는 다음 공격 실행 
        if (bEnable)
            bExist = true;

        if (currLimitTime <= 0) // 현재 입력 쿨타임이 끝난 경우 
        {
            if(inputQueue.Count == 0) // 큐가 비어있으면
            {
                // 공격 실행 
                float currentTime = Time.time;
                var inputElement = new InputElement
                {
                    InputType = keycode.ToString(),
                    TimeStamp = currentTime,
                    bNext = bExist, // 필요에 의하여 입력 유지시간을 계산 후 추가 
                };
                ExecuteAttack(ref inputElement);
            }
            else
            {
                ExecuteNextAttackFromQueue();
            }

        }
        else 
        {
            // 공격이 진행 중이라면 입력을 큐에 추가한다.
            // 큐 입력 
            float currentTime = Time.time;
            inputQueue.Enqueue(new InputElement
            {
                InputType = keycode.ToString(),
                TimeStamp = currentTime,
                bNext = bExist, // 필요에 의하여 입력 유지시간을 계산 후 추가 
            });
        }

        // 입력 큐를 정리하여 오래된 입력을 제거
        //while (inputQueue.Count > 0 && (currentTime - inputQueue.Peek().TimeStamp) > maxQueueTime)
        //{
        //    inputQueue.Dequeue();
        //}
    }

  
    private IEnumerator ComboableCoroutine()
    {
        bEnable = true;

        yield return new WaitForSeconds(inputLimitTime);

        bEnable = false;
       
    }

    public void OnBeginDoAction()
    {
        if(inputQueue.Count > 0 )
        {
            ExecuteNextAttackFromQueue();
        }
    }

    public void OnEndDoAction()
    {
        ResetCombo();
    }

    private void ResetCombo()
    {
        bEnable = false; 
        comboableCoroutine = null; 

        currLimitTime = 0;
        inputQueue.Clear(); 
    }


}


