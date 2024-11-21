using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class BehaviorTreeView : GraphView
{
    public new class UxmlFactory : UxmlFactory<BehaviorTreeView, GraphView.UxmlTraits> { }
    public BehaviorTreeView()
    {
        // 배경 그리드 추가 
        //GridBackground grid = new GridBackground();
        Insert(0, new GridBackground());
        //grid.StretchToParentSize();

        // 줌 및 이동 지원 
        this.AddManipulator(new ContentZoomer());
        this.AddManipulator(new ContentDragger());
        this.AddManipulator(new SelectionDragger());
        this.AddManipulator(new RectangleSelector());

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Editor/BehaviorTreeEditor.uss");
        styleSheets.Add(styleSheet);
    }    
}
