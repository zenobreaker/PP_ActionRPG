using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ComboData 
{
    public string ComboName;
    public float comboInputLimitTime;
    public float comboQueueLimitTime;
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
