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
            if (state.IdleMode == false)
                return NodeState.Failure;

            if (bRunning == false)
            {
                Debug.Log($"{nodeName} Action Node Begin ");
                bRunning = true;
                action.DoAction();
            }

            return NodeState.Running;
        }


        protected override NodeState OnUpdate()
        {
            if (state == null || controller == null)
            {
                return NodeState.Failure;
            }


            //Debug.Log($"{nodeName} action update");
            bool bCheck = true;
            bCheck &= (state.ActionMode);
            //bCheck &= controller.ActionMode;

            if (bCheck)
            {
                ChangeActionState(ActionState.End);
                return NodeState.Running;
            }

            bRunning = false;
            Debug.Log($"{nodeName} action end {state.IdleMode}");
            //controller.SetWaitMode(bCheck);

            return NodeState.Success;
        }

        protected override NodeState OnEnd()
        {
            // 노드 확인했는데 진행중이였다면?
            // 한싸이클을 돌리게 한다. 
            if(bRunning == true)
            {
                bRunning = false; 
                return NodeState.Running;
            }

            return base.OnEnd();
        }


        protected override NodeState OnAbort()
        {
            if(currActionState == ActionState.End)
                return NodeState.Abort;

            ChangeActionState(ActionState.End);
            bRunning = false; 
            action.End_DoAction();
            Debug.Log($"{nodeName} action abort {state.IdleMode}");

            return base.OnAbort();
        }
    }
}
