using AI.BT.Nodes;
using UnityEngine;

namespace AI.BT.CustomBTNodes
{
    public class TaskNode_ActionEnd : TaskNode
    {
        BTAIController controller;
        public TaskNode_ActionEnd(GameObject owner, SO_Blackboard blackboard)
            :base(owner, blackboard)
        {
            this.nodeName = "ActionEnd";

            controller = owner.GetComponent<BTAIController>();

            onBegin = OnBegin;
        }


        protected override NodeState OnBegin()
        {
            controller.SetWaitMode();
            
            return NodeState.Failure;
        }
    }
}
