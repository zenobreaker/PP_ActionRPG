using AI.BT.Nodes;
using System;
using UnityEngine;
using static BTAIController;

namespace AI.BT.CustomBTNodes
{
    public class TaskNode_PatternDecider : TaskNode
    {
        BTAIController controller;
        Action action; 
        public TaskNode_PatternDecider(GameObject owner, Action action)
            :base(owner)
        {
            this.nodeName = "TaskNode_PatternDecider";

            controller = owner.GetComponent<BTAIController>();
            this.action= action; 
            onBegin = OnBegin;
        }


        protected override NodeState OnBegin()
        {
            if (controller == null)
                return NodeState.Failure;
            if(action == null)
                return NodeState.Failure;

            action?.Invoke();

            return NodeState.Success;
        }
    }
}
