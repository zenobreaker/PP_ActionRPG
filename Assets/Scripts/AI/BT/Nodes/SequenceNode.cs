
using UnityEngine;

namespace AI.BT.Nodes
{

    // SequenceNode 자식이 성공하면 다음 자식을 확인한다.
    public class SequenceNode : CompositeNode
    {

        private int currentRunningNodeIndex = -1; // 현재 실행 중인 자식 추적하기 위한 변수 

        public override NodeState Evaluate()
        {
            //Debug.Log($"{nodeName} Sequence first = {currentRunningNodeIndex}");

            if (currentRunningNodeIndex != -1)
            {
                NodeState result = children[currentRunningNodeIndex].Evaluate();

                if (result == NodeState.Running)
                    return NodeState.Running;
                else if (result == NodeState.Failure)
                {
                    currentRunningNodeIndex = -1;
                    return NodeState.Failure;
                }

            }

            //Debug.Log($"{nodeName} Sequence second = {currentRunningNodeIndex}");

            for (int i = currentRunningNodeIndex + 1; i < children.Count; i++)
            {
                currentRunningNodeIndex = i;
                NodeState result = children[i].Evaluate();

                switch (result)
                {
                    case NodeState.Running:
                    return NodeState.Running;
                    case NodeState.Failure:
                    currentRunningNodeIndex = -1;
                    return NodeState.Failure;
                }
            }

            currentRunningNodeIndex = -1;
            return NodeState.Success;

        }
    }
}