using AI.BT.Helpers;
using AI.BT.Nodes;
using UnityEngine;


namespace AI.BT.CustomBTNodes
{
    public class TaskNode_ChangeAttackRange : TaskNode
    {
        BTAIController controller;
        private float range;
        public TaskNode_ChangeAttackRange(GameObject ownerObject, float range)
            : base(ownerObject)
        {
            this.nodeName = "ChangeAttackRange";
            this.range = range;

            controller = owner.GetComponent<BTAIController>();

            onBegin = OnBegin;
        }


        protected override NodeState OnBegin()
        {
            if (controller == null)
                return NodeState.Failure;

            controller.ChangeAttackRange(range);

            return NodeState.Success;
        }
    }
}