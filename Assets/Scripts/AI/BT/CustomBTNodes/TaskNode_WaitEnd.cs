using AI.BT.Nodes;
using UnityEngine;
using static BTAIController;

namespace AI.BT.CustomBTNodes
{
    public class TaskNode_WaitEnd: TaskNode
    {
        BTAIController controller;
        public TaskNode_WaitEnd(GameObject owner, SO_Blackboard blackboard)
            :base(owner, blackboard)
        {
            this.nodeName = "WaitEnd";

            controller = owner.GetComponent<BTAIController>();

            onBegin = OnBegin;
        }


        protected override NodeState OnBegin()
        {
            Debug.Log("Wait end call");

            controller.SetWaitState_NoneCondition();

            return NodeState.Success;
        }
    }
}
