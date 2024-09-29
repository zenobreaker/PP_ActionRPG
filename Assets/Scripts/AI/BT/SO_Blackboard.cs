using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public interface IComparisonStrategy
{
    bool IsEqual(object a, object b);
    bool IsGreaterthan(object a, object b);
    bool IsLessthan(object a, object b);

    IComparisonStrategy DeepCopy(); // 깊은 복사 메서드 추가
}

public class Vector3ComparisonStrategy : IComparisonStrategy
{
    public bool IsEqual(object a, object b)
    {
        if (a is Vector3 vectorA && b is Vector3 vectorB)
        {
            return vectorA == vectorB;
        }
        return false;
    }


    public bool IsGreaterthan(object a, object b)
    {
        if (a is Vector3 vectorA && b is Vector3 vectorB)
        {
            return vectorA.magnitude > vectorB.magnitude;
        }
        return false;
    }

    public bool IsLessthan(object a, object b)
    {
        if (a is Vector3 vectorA && b is Vector3 vectorB)
        {
            return vectorA.magnitude < vectorB.magnitude;
        }
        return false;
    }

    public IComparisonStrategy DeepCopy()
    {
        return new Vector3ComparisonStrategy();
    }
}
public class GameObjectComparisonStrategy : IComparisonStrategy
{
    public bool IsEqual(object a, object b)
    {
        if (a is GameObject objA && b is GameObject objB)
        {
            return ReferenceEquals(objA, objB); // 또는 이름 등 다른 기준으로 비교 가능
        }
        return false;
    }

    public bool IsGreaterthan(object a, object b)
    {
        if (a is GameObject objA && b is GameObject objB)
        {
            int hashA = objA.GetHashCode();
            int hashB = objB.GetHashCode();

            return hashA > hashB; // 또는 이름 등 다른 기준으로 비교 가능
        }
        return false;
    }

    public bool IsLessthan(object a, object b)
    {
        if (a is GameObject objA && b is GameObject objB)
        {
            int hashA = objA.GetHashCode();
            int hashB = objB.GetHashCode();

            return hashA < hashB; // 또는 이름 등 다른 기준으로 비교 가능
        }
        return false;
    }

    public IComparisonStrategy DeepCopy()
    {
        return new GameObjectComparisonStrategy();
    }
}
public class NumericComparisonStrategy<T> : IComparisonStrategy where T : struct, IComparable
{
    public IComparisonStrategy DeepCopy()
    {
        return new NumericComparisonStrategy<T>();
    }

    public bool IsEqual(object a, object b)
    {
        if (a is T valueA && b is T valueB)
            return valueA.CompareTo(valueB) == 0;
        return false;
    }

    public bool IsGreaterthan(object a, object b)
    {
        if (a is T valueA && b is T valueB)
            return valueA.CompareTo(valueB) > 0;
        return false;
    }

    public bool IsLessthan(object a, object b)
    {
        if (a is T valueA && b is T valueB)
            return valueA.CompareTo(valueB) < 0;
        return false;
    }


}
public class StringComparisonStrategy : IComparisonStrategy
{
    public IComparisonStrategy DeepCopy()
    {
        return new StringComparisonStrategy();
    }

    public bool IsEqual(object a, object b)
    {
        if (a is string valueA && b is string valueB)
        {
            return valueA.Equals(valueB);
        }
        return false;
    }

    public bool IsGreaterthan(object a, object b)
    {
        if (a is string valueA && b is string valueB)
        {
            return valueA.CompareTo(valueB) > 0;
        }

        return false;
    }

    public bool IsLessthan(object a, object b)
    {
        if (a is string valueA && b is string valueB)
        {
            return valueA.CompareTo(valueB) < 0;
        }

        return false;
    }
}

public class EnumComparisonStrategy<T> : IComparisonStrategy where T : struct, Enum
{
    public IComparisonStrategy DeepCopy()
    {
        return new EnumComparisonStrategy<T>();
    }

    public bool IsEqual(object a, object b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        if (a.GetType().IsEnum && b.GetType().IsEnum)
        {
            // 두 값이 동일한 Enum 타입인지 확인
            if (a.GetType() != b.GetType())
            {
                Debug.LogWarning($"Enum types do not match: {a.GetType()} != {b.GetType()}");
                return false;
            }

            return a.Equals(b);
        }
        else
        {
            Debug.LogWarning("Both values must be enums.");
            return false;
        }
    }

    public bool IsGreaterthan(object a, object b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        if (a.GetType().IsEnum && b.GetType().IsEnum)
        {
            if (a.GetType() != b.GetType())
            {
                Debug.LogWarning($"Enum types do not match: {a.GetType()} != {b.GetType()}");
                return false;
            }

            return (int)a > (int)b;
        }
        else
        {
            Debug.LogWarning("Both values must be enums.");
            return false;
        }
    }

    public bool IsLessthan(object a, object b)
    {
        if (a == null || b == null)
        {
            return false;
        }

        if (a.GetType().IsEnum && b.GetType().IsEnum)
        {
            if (a.GetType() != b.GetType())
            {
                Debug.LogWarning($"Enum types do not match: {a.GetType()} != {b.GetType()}");
                return false;
            }

            return (int)a < (int)b;
        }
        else
        {
            Debug.LogWarning("Both values must be enums.");
            return false;
        }
    }


}

// 블랙보드에서 사용할 데이터의 기본 인터페이스 정의 
public interface IBlackboardKey
{
    string KeyName { get; } // 키의 이름
    object GetValue(); // 값을 반환할 땐 object 타입으로 반환
    T GetValue<T>(); // 제네릭 메서드 추가
    void SetValue(object value);  // 값을 설정할 때도 object 타입으로 받기
    Type GetValueType();    // 값의 타입 반환

    IBlackboardKey DeepCopy();
}


// 제네릭을 활용하여 다양한 타입의 블랙보드 키 구현
public class BlackboardKey<T>
    : IBlackboardKey
{
    public string KeyName { get; }
    private T value;

    public BlackboardKey(string KeyName)
    {
        this.KeyName = KeyName;
    }

    public object GetValue() => value;
    // 제네릭 메서드 구현 
    public U GetValue<U>()
    {
        return (U)(object)value; // T로 캐스팅
    }

    public void SetValue(object newValue)
    {
        if (newValue is T typedValue)
        {
            value = typedValue;

            return;
        }

        value = default(T);
    }

    public Type GetValueType() => typeof(T);

    public IBlackboardKey DeepCopy()
    {
        BlackboardKey<T> copy = new BlackboardKey<T>(this.KeyName);
        copy.SetValue(this.GetValue()); // 깊은 복사: 값 복사
        return copy;
    }
}


[CreateAssetMenu(fileName = "NewBlackboard", menuName = "AI/Blackboard")]
public class SO_Blackboard : ScriptableObject
{
    private Dictionary<string, IBlackboardKey> keys = new Dictionary<string, IBlackboardKey>();
    private Dictionary<Type, IComparisonStrategy> comparisonStrategies = new Dictionary<Type, IComparisonStrategy>();

    // 블랙보드의 특정 키가 변경될 때 발생
    public event Action<string> OnValueChanged;

    public void Initialize()
    {
        // 비교 전략 등록
        comparisonStrategies[typeof(Vector3)] = new Vector3ComparisonStrategy();
        comparisonStrategies[typeof(GameObject)] = new GameObjectComparisonStrategy();
        comparisonStrategies[typeof(string)] = new StringComparisonStrategy();
        comparisonStrategies[typeof(int)] = new NumericComparisonStrategy<int>();
        comparisonStrategies[typeof(float)] = new NumericComparisonStrategy<float>();
        comparisonStrategies[typeof(double)] = new NumericComparisonStrategy<double>();
    }

    public void AddEnumComparisonStrategy<T>() where T : struct, Enum
    {
        comparisonStrategies[typeof(T)] = new EnumComparisonStrategy<T>();
    }

    // 새로운 키 추가
    public void AddKey<T>(string keyName)
    {
        if (!keys.ContainsKey(keyName))
            keys[keyName] = new BlackboardKey<T>(keyName);
    }

    // 데이터 설정 메서드
    public void SetValue(string keyName, object value)
    {
        Type valueType = value.GetType();

        if (keys.ContainsKey(keyName) && keys[keyName].GetValueType() == valueType)
        {
            // 이미 존재하는 키는 캐스팅 후 값 설정
            IBlackboardKey typedKey = keys[keyName];
            typedKey.SetValue(value);
        }
        else
        {
            // 존재하지 않은 경우 새롭게 생성
            Type keyType = typeof(BlackboardKey<>).MakeGenericType(valueType);
            IBlackboardKey newKey = (IBlackboardKey)Activator.CreateInstance(keyType, keyName);

            // 값 설정
            if (value is IConvertible convertible)
            {
                // IConvertible 인터페이스를 구현하는 경우
                newKey.SetValue(convertible); // 여기서도 동적 설정
            }
            else
            {
                // 직접 캐스팅하여 값을 설정
                newKey.SetValue(value);
            }

            keys[keyName] = newKey;
        }
    }

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
            //Debug.Log($"Key {keyName} not found in Blackobard");
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
                return typedKey.GetValue<T>();
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

    public bool CompareValue<T>(string keyName1, string keyName2)
    {

        if (keys.TryGetValue(keyName1, out IBlackboardKey key))
        {
            if (key.GetValueType() == typeof(T))
            {
                IComparisonStrategy strategy = GetComparisonStrategy(typeof(T));
                if (strategy != null)
                {
                    object storedValue = key.GetValue();
                    return strategy.IsEqual(storedValue, keyName2);
                }

            }
        }

        return false;
    }

    public bool CompareValue<T>(string keyName, T value)
    {
        if (keys.TryGetValue(keyName, out IBlackboardKey key))
        {
            if (key.GetValueType() == typeof(T))
            {
                IComparisonStrategy strategy = GetComparisonStrategy(typeof(T));
                if (strategy != null)
                {
                    object storedValue = key.GetValue();
                    return strategy.IsEqual(storedValue, value);
                }
            }
        }
        return false;
    }

    public bool GreaterThanValue<T>(string keyName1, string keyName2)
    {
        if (keys.TryGetValue(keyName1, out IBlackboardKey key))
        {
            if (key.GetValueType() == typeof(T))
            {
                IComparisonStrategy strategy = GetComparisonStrategy(typeof(T));
                if (strategy != null)
                {
                    object storedValue = key.GetValue();
                    return strategy.IsGreaterthan(storedValue, keyName2);
                }
            }
        }

        return false;
    }

    public bool GreaterThanValue<T>(string keyName, T value)
    {
        if (keys.TryGetValue(keyName, out IBlackboardKey key))
        {
            if (key.GetValueType() == typeof(T))
            {
                IComparisonStrategy strategy = GetComparisonStrategy(typeof(T));
                if (strategy != null)
                {
                    object storedValue = key.GetValue();
                    return strategy.IsGreaterthan(storedValue, value);
                }
            }
        }
        return false;
    }

    public bool LessThanValue<T>(string keyName1, string keyName2)
    {
        if (keys.TryGetValue(keyName1, out IBlackboardKey key))
        {
            if (key.GetValueType() == typeof(T))
            {
                IComparisonStrategy strategy = GetComparisonStrategy(typeof(T));
                if (strategy != null)
                {
                    object storedValue = key.GetValue();
                    return strategy.IsLessthan(storedValue, keyName2);
                }
            }
        }

        return false;
    }

    public bool LessThanValue<T>(string keyName, T value)
    {
        if (keys.TryGetValue(keyName, out IBlackboardKey key))
        {
            if (key.GetValueType() == typeof(T))
            {
                IComparisonStrategy strategy = GetComparisonStrategy(typeof(T));
                if (strategy != null)
                {
                    object storedValue = key.GetValue();
                    return strategy.IsLessthan(storedValue, value);
                }
            }
        }
        return false;
    }

    private IComparisonStrategy GetComparisonStrategy(Type type)
    {
        comparisonStrategies.TryGetValue(type, out IComparisonStrategy strategy);
        return strategy;
    }

    public SO_Blackboard Clone()
    {
        SO_Blackboard soBB = ScriptableObject.CreateInstance<SO_Blackboard>();
        soBB.keys = new Dictionary<string, IBlackboardKey>();
        soBB.comparisonStrategies = new Dictionary<Type, IComparisonStrategy>();

        foreach (KeyValuePair<string, IBlackboardKey> pair in this.keys)
        {
            IBlackboardKey value = pair.Value.DeepCopy();
            soBB.keys[pair.Key] = value;
        }

        foreach (KeyValuePair<Type, IComparisonStrategy> pair in this.comparisonStrategies)
        {
            IComparisonStrategy value = pair.Value.DeepCopy();
            soBB.comparisonStrategies[pair.Key] = value;
        }

        return soBB;
    }
}
