using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public class ComboComponent : MonoBehaviour
{
    #region UI
    [SerializeField] GameObject comboInputUIPrefab;
    [SerializeField] GameObject comboInputBase;
    [SerializeField] InputGaugeUI comboMaintainTimeGauge;
    [SerializeField] bool bDebugDraw = false;

    private string comboInputUIName = "Input_Action";
    private string comboMaintainGaugeName = "InputTimingGauge";
    private string inputBaseName = "InputComboBase";
    private string canvasName = "UserUI";
    private Canvas uiCanvas;

    #endregion

    class InputElement
    {
        public string InputType; // 입력 타입
        public float TimeStamp;  // 입력이 발생한 시간
        public int comboCount;
    }

    [SerializeField] private float comboCheckTime = 0.5f;         // 공격 후 다음 공격이 있는지 확인할 시간
    [SerializeField] private float lastInputCheckTime = 0.25f;    // 다음 콤보를 입력을 바라는 제한 시간
    [SerializeField] private float comboMaintainTime = 1.0f;        // 콤보(입력 큐) 유지 시간 
    private float curr_MaintainTime;

    private float lastInputTime = 0.0f;             // 마지막에 입력한 콤보 입력 시간  
    private float lastComboEnd = 0.0f;              // 마지막 동작 종료 시간
    private int comboCount = 0;

    private Queue<InputElement> inputQueue;

    [SerializeField] private SO_Combo currComboObj;
    private WeaponComponent weapon;

    private Coroutine comboMaintainCoroutine;

    private void Awake()
    {
        weapon = GetComponent<WeaponComponent>();
        Debug.Assert(weapon != null);
        weapon.OnWeaponTypeChanged_Combo += OnWeaponTypeChanged_Combo;
        weapon.OnBeginDoAction += OnBeginDoAction;
        weapon.OnEndDoAction += OnEndDoAction;

        inputQueue = new Queue<InputElement>();

        uiCanvas = GameObject.Find(canvasName).GetComponent<Canvas>();
        Debug.Assert(uiCanvas != null);
        comboInputUIPrefab = Resources.Load<GameObject>(comboInputUIName);
        var targetTransform = uiCanvas.transform.FindChildByName(inputBaseName);
        if(targetTransform != null)
        {
            comboInputBase = targetTransform.gameObject;
        }
        var uiGauge = uiCanvas.transform.FindChildByName(comboMaintainGaugeName);
        Debug.Assert(uiGauge != null);
        if(uiGauge != null)
        {
            comboMaintainTimeGauge = uiGauge.GetComponent<InputGaugeUI>();
        }

    }

    private void ResetTimerValue(ComboData data)
    {
        comboCheckTime = data.lastComboCheckTime;
        lastInputCheckTime = data.lastInputCheckTime;
        comboMaintainTime = data.comboMaintainTime;
    }

    private void OnWeaponTypeChanged_Combo(SO_Combo comboData)
    {
        if (comboData == null)
            return;

        currComboObj = comboData;
        ResetCombo();


        comboMaintainTimeGauge?.InitializeGauge(comboMaintainTime);

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
        {
            comboMaintainTimeGauge?.gameObject.SetActive(false);
            return;
        }

        comboMaintainTimeGauge?.gameObject.SetActive(true);
        comboMaintainTimeGauge?.SetValue(curr_MaintainTime);
    }

    #endregion

    private void LateUpdate()
    {
        DrawInputGauge();
    }

    private void ExecuteAttack(ref InputElement inputElement)
    {
        if(weapon.Type == WeaponType.FireBall)
        {
            weapon.DoAction();
            return; 
        }

        if (currComboObj == null)
            return;

        // Debug
        Create_ComboUI();
        ComboData data = currComboObj.GetComboDataByRewind(comboCount);
        //Debug.Log($"over does breakre  {data.ComboName} ");

        // 시간 초기화 
        {
            ResetTimerValue(data);
        }

        // Action 실행
        {
            //Debug.Log($"Execute Combodata {data.ComboName} / Time stamp {inputElement.TimeStamp}");
            weapon.DoAction(data.ComboIndex);
        }

        //UI 처리 
        {
            comboMaintainTimeGauge?.SetMaxValue(data.comboMaintainTime);
        }
    }

    public void Test_InputComobo()
    {
        ////Debug.Log($"Time Check zero input : {Time.time} / {lastComboEnd} / ");

        //// 마지막 콤보가 끝난 후 해당 시간이 경과했는지 확인
        //if (Time.time - lastComboEnd >= comboCheckTime)
        //{
        //    //Debug.Log($"Time Check first input : {Time.time - lastComboEnd} / " +
        //    //    $"{comboMaintainTime}");

        //    // 콤보 타이머 체크를 중단
        //    if (comboMaintainCoroutine != null)
        //        StopCoroutine(comboMaintainCoroutine);

        //    // 마지막 입력 후 해당 시간 만큼 지났는지 
        //    if (Time.time - lastInputTime >= lastInputCheckTime)
        //    {
        //        //   Debug.Log($"Time Check second input : {Time.time - lastInputTime} / " +
        //        //$"{lastInputCheckTime}");

        //        // 다음 콤보 실행 
        //        // 공격 실행 
        //        float currentTime = Time.time;
        //        var inputElement = new InputElement
        //        {
        //            InputType = keycode.ToString(),
        //            TimeStamp = currentTime,
        //            comboCount = this.comboCount
        //        };
        //        ExecuteAttack(ref inputElement);
        //        comboCount++;

        //        lastInputTime = Time.time; // 값 최신화 
        //    }
        //}
    }


    public void InputCombo(KeyCode keycode)
    {
        // 마지막 콤보가 끝난 후 해당 시간이 경과했는지 확인
        if (Time.time - lastComboEnd >= comboCheckTime)
        {
            // 콤보 타이머 체크를 중단
            if(comboMaintainCoroutine != null)
                StopCoroutine(comboMaintainCoroutine);

            // 마지막 입력 후 해당 시간 만큼 지났는지 
            if (Time.time - lastInputTime >= lastInputCheckTime)
            {
                // 다음 콤보 실행 
                float currentTime = Time.time;
                var inputElement = new InputElement
                {
                    InputType = keycode.ToString(),
                    TimeStamp = currentTime,
                    comboCount = this.comboCount
                };

                ExecuteAttack(ref inputElement);
                comboCount++;

                lastInputTime = Time.time; // 값 최신화 
            }
        }
    }


    // 입력 제한 시간 안에 입력 받았는지 검사한다. 
    private IEnumerator ComboMaintainCoroutine()
    {
        curr_MaintainTime = comboMaintainTime;
        while (curr_MaintainTime > 0)
        {
            curr_MaintainTime -= Time.deltaTime;
            yield return null;
        }

        ResetCombo();
    }

    public void OnBeginDoAction()
    {

    }

    public void OnEndDoAction()
    {
        // 진행했던 애니메이션이 끝나고 이곳을 호출하게 되면 종료자를 호출한다. 
        comboMaintainCoroutine = StartCoroutine(ComboMaintainCoroutine());
    }


    private void ResetCombo()
    {
        comboCount = 0;
        var data = currComboObj?.GetComboDataByRewind(0);
        ResetTimerValue(data);


        lastComboEnd = Time.time;
        comboMaintainCoroutine = null;

        inputQueue.Clear();
        currComboObj?.ResetComboIndex();

        DestroyComboUIObjs();
    }


}


