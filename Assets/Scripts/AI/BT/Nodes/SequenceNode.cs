
using UnityEngine;

namespace AI.BT.Nodes
{

    // SequenceNode 자식이 성공하면 다음 자식을 확인한다.
    public class SequenceNode : CompositeNode
    {
        public SequenceNode()
        {
            nodeName = "Sequence";
        }

        private int currentRunningNodeIndex = -1; // 현재 실행 중인 자식 추적하기 위한 변수 


        public override NodeState Evaluate()
        {
            OnStart();

            if (currentRunningNodeIndex != -1)
            {
                NodeState result = children[currentRunningNodeIndex].Evaluate();
                //Debug.Log($"Sequence = Previous Node Evaluate {currentRunningNodeIndex} /" +
                //   $"{children[currentRunningNodeIndex].NodeName}");
                if (result == NodeState.Running)
                    return NodeState.Running;
                else if (result == NodeState.Failure)
                {
                   // Debug.Log($"Sequence = Previous Node Evaluate Fail {currentRunningNodeIndex} /" +
                   //$"{children[currentRunningNodeIndex].NodeName}");
                    currentRunningNodeIndex = -1;
                    OnEnd();
                    return NodeState.Failure;
                }
                else if (result == NodeState.Abort)
                {
                    // Debug.Log($"Sequence = Previous Node Evaluate Fail {currentRunningNodeIndex} /" +
                    //$"{children[currentRunningNodeIndex].NodeName}");
                    //TODO: 중단시 처음부터 자식 노드들을 검사시킬지 의문이다.
                    currentRunningNodeIndex = -1;
                    OnEnd();
                    return NodeState.Abort;
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
                    
                    case NodeState.Failure:
                    currentRunningNodeIndex = -1;
                    OnEnd();
                    return NodeState.Failure;

                    case NodeState.Abort:
                    currentRunningNodeIndex = -1;
                    OnEnd();
                    return NodeState.Abort;
                }
            }

            currentRunningNodeIndex = -1;
            OnEnd();
            return NodeState.Success;

        }

        public override void StopEvaluate()
        {
            foreach(var child in children)
            {
                child.StopEvaluate();
            }

            currentRunningNodeIndex = -1;
        }
    }
}