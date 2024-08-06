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

    private Queue<KeyCode> uiQueue;
    private Queue<int> comboQueue;
    [SerializeField] private float inputLimitTime = 1.0f; // 이 초안에 입력하면 큐에 삽입
    [SerializeField] private float queueCleanTime = 1.0f;
    private float currLimitTime;
    private bool bCanInput = true;  // 입력 제한을 거는 플래그
    private bool bNextable = false; // 다음 콤보를 취할 타이밍인지 체크
    //private bool bRemainTiming = false; // 입력 후 다음 입력에 남은 시간 체크 
    private int comboCount;

    private WeaponComponent weapon;
    private SO_Combo currComboObj;

    private Coroutine coroutine_ChangeLimitTime;
    private Coroutine coroutine_LimitInputCombo;
    public event Action OnClearInputQueue;
    public event Action OnInputCombo;

    private void Awake()
    {
        weapon = GetComponent<WeaponComponent>();
        Debug.Assert(weapon != null);
        weapon.OnBeginDoAction += DoNextCombo;
        weapon.OnWeaponTypeChanged_Combo += OnWeaponTypeChanged_Combo;

        comboQueue = new Queue<int>();
        uiQueue = new Queue<KeyCode>();
        comboCount = 0;


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
        currComboObj.SetOnFinishCombo(OnClearQueue);
    }

    private void SetTimeData(ComboData data)
    {
        if (data == null)
            return;

        this.inputLimitTime = data.comboInputLimitTime;
        this.queueCleanTime = data.comboQueueLimitTime;
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


    private void Test_PrintQueue()
    {
        string str = "Queue List : ";
        foreach (KeyCode keyCode in uiQueue)
        {
            str += keyCode.ToString() + " ";
        }

        Debug.Log($"{str}");
    }

    private void Execute()
    {
        if (comboQueue.Count == 0)
            return;

        int combo = comboQueue.Dequeue();
        weapon.DoAction(combo);
    }

    public void InputCombo(KeyCode keyCode)
    {
        Create_ComboUI();
        // 입력 시간 내에 큐에 넣었다면 invoke 대상자는 다음 행동을 취하게?
        SetLimitTime();
        comboQueue.Enqueue(comboCount++);
        uiQueue.Enqueue(keyCode);

        // 특정 타이밍 
        // 처음 큐는 바로 동작을 수행한다. 
        if (bCanInput)
        {
            bCanInput = false;
            bNextable = false;
            //Debug.Log("Input Call do a");
            Execute();
        }
        else if (bNextable)
        {
            //Debug.Log("Input timing over call");
            bNextable = false;
            Execute();
        }

        // 콤보 입력 제한 타이머를 돌린다. - 이미 콤보큐 제한 타이머를 돌린다면 더 돌리지 않음
        if (coroutine_LimitInputCombo != null)
            StopCoroutine(coroutine_LimitInputCombo);

        coroutine_LimitInputCombo = StartCoroutine(Coroutine_LimitInputCombo());

    }


    private void OnClearQueue()
    {
        CleanQueue("OnClearQueue");
    }

    private void CleanQueue(string debug = "CleanQueue")
    {
        //Test_PrintQueue();

        DestroyComboUIObjs();

        comboCount = 0;
        uiQueue.Clear();
        comboQueue.Clear();
        bCanInput = true;
        SetComboCount(0, debug);
        coroutine_LimitInputCombo = null;
    }

    // 큐 초기화
    private IEnumerator Coroutine_LimitInputCombo()
    {
        yield return new WaitForSeconds(queueCleanTime);
        CleanQueue();
    }

    // 제한 시간 설정
    private void SetLimitTime()
    {
        if (currComboObj != null)
        {
            ComboData data = currComboObj.GetComboDataByRewind(comboCount);
            SetTimeData(data);
        }

        // 남은 시간 체크 
        //bRemainTiming = true;

        currLimitTime = inputLimitTime;

        if (coroutine_ChangeLimitTime != null)
            StopCoroutine(coroutine_ChangeLimitTime);

        coroutine_ChangeLimitTime = StartCoroutine(Coroutine_ChangeLimitTime());
    }

   
    // 제한시간 변경 코루틴 
    private IEnumerator Coroutine_ChangeLimitTime()
    {
        while (currLimitTime > 0)
        {
            currLimitTime -= Time.deltaTime;
            DrawInputGauge();
            yield return null;
        }

        //Test_PrintQueue();

        DestroyComboUIObjs();

        //bRemainTiming = false;
        comboCount = 0;
        uiQueue.Clear();
        comboQueue.Clear();
        bCanInput = true;

        SetComboCount(0, "Coroutine_ChangeLimitTime");
    }

    // 이벤트에 의해서 전달 되면 동작을 수행한다. 
    private void DoNextCombo()
    {
        // 큐에 등록된게 있다면 해당 액션을 실행 
        if (comboQueue.Count > 0)
        {
            //Debug.Log("Combo comp call do a");
            //int combo = comboQueue.Dequeue();
            //weapon.DoAction(combo, true);
            Execute();
        }
        // 없으면 입력 받아서 동작하기 위한 플래그 
        else
        {
            bNextable = true;
        }
    }

    private void SetComboCount(int comboCount, string caller = "")
    {
        this.comboCount = comboCount;

        //if (caller != "")
        //    Debug.Log("Caller : " + caller);
    }


}


