using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AI.BT.Nodes
{
    public class ParallelNode : CompositeNode
    {
        public enum FinishCondition
        {
            All, // 모든 자식이 성공해야 성공
            One, // 하나라도 성공하면 성공
        }

        private FinishCondition finishCondition;

        private NodeState mainNodeState;
        private NodeState backgroundNodeState;

        public ParallelNode(FinishCondition finishCondition)
            : base()
        {
            this.finishCondition = finishCondition;
        }

        protected override void OnStart()
        {
      
        }

        public override NodeState Evaluate()
        {

           if(children.Count > 0)
                mainNodeState = children[0].Evaluate();

           if(children.Count > 1)
                backgroundNodeState = children[1].Evaluate();

           if(mainNodeState == NodeState.Success || mainNodeState == NodeState.Failure)
                return mainNodeState;

            // 서브 노드가 완료 되면 패러렐 노드는 계속 실행
            return NodeState.Running;
        }

        protected override void OnEnd()
        {
      
            hasFirstStart = true;
        }
    }

}