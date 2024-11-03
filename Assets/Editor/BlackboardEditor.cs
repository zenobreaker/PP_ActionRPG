#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SO_Blackboard))]
public class BlackboardEditor : Editor
{

    // 사용할 데이터 타입들을 Enum으로 정의
    private enum BlackboardType
    {
        Int,
        Float,
        String
    }

    private string newKey;
    BlackboardType selectedType = BlackboardType.Int;
    object newValue;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SO_Blackboard blackboard = (SO_Blackboard)target;

        // 새로운 키 추가 섹션
        GUILayout.Label("Add New Key", EditorStyles.boldLabel);
        newKey = EditorGUILayout.TextField("Key Name", newKey);

        // 타입 선택 (여기서는 간단하게 int, float, string만 선택 가능)
        selectedType = (BlackboardType)EditorGUILayout.EnumPopup("Type", selectedType);



    }
}


#endif