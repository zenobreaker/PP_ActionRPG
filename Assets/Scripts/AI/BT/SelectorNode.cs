using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// SelectorNode 는 실패하면 다음 자식을 확인한다. 
public class SelectorNode : BTNode
{
    private  List<BTNode> children;
    private int currentRunningNodeIndex = -1;  // 현재 실행 중인 자식 추적하기 위한 변수

    public SelectorNode(List<BTNode> children)
    {
        this.children = children;
    }

    public  override NodeState Evaluate()
    {
        if(currentRunningNodeIndex !=  -1) 
        { 
            // 이전에 실행 중이던 노드 평가하기
            NodeState result = children[currentRunningNodeIndex].Evaluate();
            if(result  == NodeState.Running)
            {
                return NodeState.Running;
            }
            else
            {
                currentRunningNodeIndex = -1;
                if (result == NodeState.Success)
                    return NodeState.Success;
            }
        }

        foreach (BTNode node in children)
        {
            switch (node.Evaluate())
            {
                case NodeState.Running:
                return NodeState.Running;   
                case NodeState.Success:
                return NodeState.Success;
            }
        }
        return NodeState.Failure;
    }

}
