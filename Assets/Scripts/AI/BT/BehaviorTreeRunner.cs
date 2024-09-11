using UnityEditor.Rendering.Universal;
using UnityEngine;

public class BehaviorTreeRunner 
{
    private BTNode rootNode;

    public BehaviorTreeRunner( BTNode rootNode)
    {
        this.rootNode = rootNode;
    }

    public void OperateNode()
    {
        rootNode.Evaluate();
    }   
    public void OperateNode(bool debugMode)
    {
        OperateNode();
    }
}
