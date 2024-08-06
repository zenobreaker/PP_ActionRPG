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
    [SerializeField] private float inputLimitTime = 1.0f; // �� �ʾȿ� �Է��ϸ� ť�� ����
    [SerializeField] private float queueCleanTime = 1.0f;
    private float currLimitTime;
    private bool bCanInput = true;  // �Է� ������ �Ŵ� �÷���
    private bool bNextable = false; // ���� �޺��� ���� Ÿ�̹����� üũ
    //private bool bRemainTiming = false; // �Է� �� ���� �Է¿� ���� �ð� üũ 
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
        // �Է� �ð� ���� ť�� �־��ٸ� invoke ����ڴ� ���� �ൿ�� ���ϰ�?
        SetLimitTime();
        comboQueue.Enqueue(comboCount++);
        uiQueue.Enqueue(keyCode);

        // Ư�� Ÿ�̹� 
        // ó�� ť�� �ٷ� ������ �����Ѵ�. 
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

        // �޺� �Է� ���� Ÿ�̸Ӹ� ������. - �̹� �޺�ť ���� Ÿ�̸Ӹ� �����ٸ� �� ������ ����
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

    // ť �ʱ�ȭ
    private IEnumerator Coroutine_LimitInputCombo()
    {
        yield return new WaitForSeconds(queueCleanTime);
        CleanQueue();
    }

    // ���� �ð� ����
    private void SetLimitTime()
    {
        if (currComboObj != null)
        {
            ComboData data = currComboObj.GetComboDataByRewind(comboCount);
            SetTimeData(data);
        }

        // ���� �ð� üũ 
        //bRemainTiming = true;

        currLimitTime = inputLimitTime;

        if (coroutine_ChangeLimitTime != null)
            StopCoroutine(coroutine_ChangeLimitTime);

        coroutine_ChangeLimitTime = StartCoroutine(Coroutine_ChangeLimitTime());
    }

   
    // ���ѽð� ���� �ڷ�ƾ 
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

    // �̺�Ʈ�� ���ؼ� ���� �Ǹ� ������ �����Ѵ�. 
    private void DoNextCombo()
    {
        // ť�� ��ϵȰ� �ִٸ� �ش� �׼��� ���� 
        if (comboQueue.Count > 0)
        {
            //Debug.Log("Combo comp call do a");
            //int combo = comboQueue.Dequeue();
            //weapon.DoAction(combo, true);
            Execute();
        }
        // ������ �Է� �޾Ƽ� �����ϱ� ���� �÷��� 
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


