using AI.BT.Nodes;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace AI.BT.CustomBTNodes
{
    public class TaskNode_Action : TaskNode
    {

        BTAIController controller;
        StateComponent state;
        IActionComponent action;

        public TaskNode_Action(GameObject owner, SO_Blackboard blackboard)
            : base(owner, blackboard)
        {
            this.nodeName = "Action";

            action = owner.GetComponent<IActionComponent>();
            controller = owner.GetComponent<BTAIController>();
            state = owner.GetComponent<StateComponent>();
            onBegin = OnBegin;
            onUpdate = OnUpdate;
            onEnd = OnEnd;
            onAbort = OnAbort;
        }


        protected override NodeState OnBegin()
        {
            if (action == null)
            {
                return NodeState.Failure;
            }
            Debug.Log($"{nodeName} Action Node Begin ");
            if (state.IdleMode == false)
                return NodeState.Failure;

            action.DoAction();

            return NodeState.Running;
        }


        protected override NodeState OnUpdate()
        {
            if (state == null || controller == null)
            {
                return NodeState.Failure;
            }

            Debug.Log($"{nodeName} action update");
            bool bCheck = true;
            bCheck &= (state.ActionMode);
            bCheck &= controller.ActionMode;

            if (bCheck)
                return NodeState.Running;

            Debug.Log($"{nodeName} action end {state.IdleMode} / {controller.ActionMode}");
            //controller.SetWaitMode(bCheck);

            return NodeState.Success;
        }

        protected override NodeState OnEnd()
        {
            return base.OnEnd();
        }


        protected override NodeState OnAbort()
        {
            if(currActionState == ActionState.End)
                return NodeState.Abort;

                Debug.Log($"{nodeName} action abort {state.IdleMode} / {controller.ActionMode}");

            return base.OnAbort();
        }
    }
}
