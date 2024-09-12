using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum BlackboardKey
{
    TargetPosition,
    TargetHealth,
    IsAlerted,
    EnemyCount,
}

[CreateAssetMenu(fileName = "NewBlackboard", menuName = "AI/Blackboard")]
public class SO_Blackboard : ScriptableObject
{
    private Dictionary<BlackboardKey, object> data = new Dictionary<BlackboardKey, object>();

    // 데이터 설정 메서드
    public void SetValue<T>(BlackboardKey key, T value)
    {
        if(data.ContainsKey(key))
            data[key] = value;
        else 
            data.Add(key, value);
    }

    // 데이터 가져오기 메서드
    public T GetValue<T>(BlackboardKey key)
    {
        if (data.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        else
        {
            return default(T);
        }
    }

    // 데이터 존재 여부 확인
    public bool HasKey(BlackboardKey key)
    {
        return data.ContainsKey(key);
    }

    public SO_Blackboard Clone()
    {
        SO_Blackboard soBB = ScriptableObject.CreateInstance<SO_Blackboard>();
        soBB.data = new Dictionary<BlackboardKey, object>();

        foreach(KeyValuePair<BlackboardKey, object> pair in this.data)
        {
            //TODO: object 복사에 대한 정의 필요
            object value = pair.Value;
            soBB.data[pair.Key] = value;
        }

        return soBB;
    }
}
