using System.Collections;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AI.BT.Nodes
{
    // SelectorNode 는 실패하면 다음 자식을 확인한다. 
    public class SelectorNode : CompositeNode
    {
        private int currentRunningNodeIndex = -1;  // 현재 실행 중인 자식 추적하기 위한 변수

        public override NodeState Evaluate()
        {

            if (currentRunningNodeIndex != -1)
            {
                // 이전에 실행 중이던 노드 평가하기
                NodeState result = children[currentRunningNodeIndex].Evaluate();
                if (result == NodeState.Running)
                {
                    return NodeState.Running;
                }
                else if (result == NodeState.Success)
                {
                    currentRunningNodeIndex = -1;
                    return NodeState.Success;
                }

            }

            for (int i = currentRunningNodeIndex + 1; i < children.Count; i++)
            {
                currentRunningNodeIndex = i;
                NodeState result = children[i].Evaluate();

                switch (result)
                {
                    case NodeState.Running:
                    return NodeState.Running;
                    case NodeState.Success:
                    currentRunningNodeIndex = -1;
                    return NodeState.Success;
                }
            }

            currentRunningNodeIndex = -1;
            return NodeState.Failure;
        }

    }
}