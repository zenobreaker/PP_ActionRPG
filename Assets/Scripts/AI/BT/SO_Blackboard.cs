using System;
using System.Collections.Generic;
using UnityEngine;


// 블랙보드에서 사용할 데이터의 기본 인터페이스 정의 
public interface IBlackboardKey
{
    string KeyName { get; } // 키의 이름
    Type GetValueType();    // 값의 타입 반환
}

// 제네릭을 활용하여 다양한 타입의 블랙보드 키 구현
public class BlackboardKey<T> : IBlackboardKey
{
    public string KeyName { get; }
    private T value; 

    public BlackboardKey(string KeyName)
    {
        this.KeyName = KeyName;
    }

    public T GetValue() => value;

    public void SetValue(T newValue) => value = newValue;

    public Type GetValueType() => typeof(T);
}


[CreateAssetMenu(fileName = "NewBlackboard", menuName = "AI/Blackboard")]
public class SO_Blackboard : ScriptableObject
{
    private Dictionary<string, IBlackboardKey> keys = new Dictionary<string, IBlackboardKey>();

    // 블랙보드의 특정 키가 변경될 때 발생
    public event Action<string> OnValueChanged;

    // 새로운 키 추가
    public void AddKey<T>(string keyName)
    {
        if(!keys.ContainsKey(keyName))
            keys[keyName] = new BlackboardKey<T>(keyName);
    }
    // 데이터 설정 메서드
    public void SetValue<T>(string keyName, T value)
    {
        if (keys.TryGetValue(keyName, out IBlackboardKey key))
        {
            if (key is BlackboardKey<T> typedKey)
            {
                typedKey.SetValue(value);
                OnValueChanged?.Invoke(keyName); // 값 변경 시 트리거되게
            }
            else
            {
                Debug.Log($"Key {keyName} is not of Type {typeof(T)}");
            }
        }
        else
        {
            Debug.Log($"Key {keyName} not found in Blackobard");
            BlackboardKey<T> newKey = new BlackboardKey<T>(keyName);
            newKey.SetValue(value);
            keys.Add(keyName, newKey);

            //OnValueChanged?.Invoke(keyName); // 새로 추가된 것도 트리거 
        }
    }

    // 키의 값 가져오기 
    public T GetValue<T>(string keyName)
    {
        if (keys.TryGetValue(keyName, out IBlackboardKey key))
        {
            if (key is BlackboardKey<T> typedKey)
            {
                return typedKey.GetValue();
            }
            else
            {
                Debug.Log($"Key {keyName} is not of type {typeof(T)}");
            }
        }
        else
        {
            Debug.Log($"Key {keyName} not found in Blackboard");
        }

        return default(T);
    }

   
    public SO_Blackboard Clone()
    {
        SO_Blackboard soBB = ScriptableObject.CreateInstance<SO_Blackboard>();
        soBB.keys = new Dictionary<string, IBlackboardKey>();

        foreach(KeyValuePair<string, IBlackboardKey> pair in this.keys)
        {
            IBlackboardKey value = pair.Value;
            soBB.keys[pair.Key] = value;
        }

        return soBB;
    }
}
