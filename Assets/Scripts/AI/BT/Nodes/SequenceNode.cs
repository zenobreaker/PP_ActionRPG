
namespace AI.BT.Nodes
{

    // SequenceNode 자식이 성공하면 다음 자식을 확인한다.
    public class SequenceNode : CompositeNode
    {

        private int currentRunningNodeIndex = -1; // 현재 실행 중인 자식 추적하기 위한 변수 

        public override NodeState Evaluate()
        {
            if (currentRunningNodeIndex != -1)
            {
                NodeState result = children[currentRunningNodeIndex].Evaluate();
                if (result == NodeState.Running)
                    return NodeState.Running;
                else
                {
                    currentRunningNodeIndex = -1;
                    if (result == NodeState.Failure)
                        return NodeState.Failure;
                }
            }

            for (int i = 0; i < children.Count; i++)
            {
                if (i <= currentRunningNodeIndex)
                    continue;
                
                currentRunningNodeIndex = i;

                NodeState result = children[i].Evaluate();
                switch (result)
                {
                    case NodeState.Running:
                    return NodeState.Running;
                    case NodeState.Failure:
                    return NodeState.Failure;
                }
            }

            return NodeState.Success;

        }
    }
}