using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 다음 큐로 확정 짓는 시간이 입력 제한 크다면 입력 제한 시간 안에 아무리 입력해도
/// 다음 콤보로 이어지지 않는 효과가 있다. 
/// 입력 제한 시간이 남아 있는 상태에서 입력하면 큐에 등록하는 시간이다.
/// 다음 큐로 확정 짓는 시간이 남아 있는 상태에서 입력 받으면 다음 콤보로 이어질 플래그가 켜진다.
/// </summary>
[System.Serializable]
public class ComboData 
{
    public string ComboName;
    public float comboInputLimitTime;       // 입력 제한 시간 
    public float comboNextInputLimitTime;    // 다음 콤보큐로 확정 짓는 제한 시간 
}

[CreateAssetMenu(fileName = "ComboObject", menuName = "ScriptableObjects/Combo", order = 1)]
public class SO_Combo : ScriptableObject
{
    public List<ComboData> comboDatas = new List<ComboData>();

    public event Action OnFinishCombo;

    public void SetOnFinishCombo(Action onFinishCombo)
    {
        if (OnFinishCombo != null)
        {
            foreach (Action action in OnFinishCombo.GetInvocationList())
            {
                if (action == OnFinishCombo)
                    return;
            }
        }

        OnFinishCombo = onFinishCombo;
    }

    public ComboData GetComboData(int index)
    {
        return comboDatas[index];
    }

    public ComboData GetComboDataByRewind(int index)
    {
        index %= (comboDatas.Count);

        return GetComboData(index);
    }

    public void OnChangeCombo (int combo)
    {
        if (combo == (comboDatas.Count))
        {
            OnFinishCombo?.Invoke();
        }
    }
}
