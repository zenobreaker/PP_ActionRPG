using System;
using UnityEngine;

namespace AI.BT.Nodes
{
    // 최하위 작업 노드 
    public class TaskNode : BTNode
    {
        protected enum ActionState
        {
            Begin, Update, End
        }

        protected ActionState currActionState;

        protected Func<BTNode.NodeState> onBegin = null;
        protected Func<BTNode.NodeState> onUpdate = null;
        protected Func<BTNode.NodeState> onEnd = null;
        protected Func<BTNode.NodeState> onAbort = null;

        protected SO_Blackboard blackboard;

        #region Constructor

        public TaskNode(GameObject owner = null, SO_Blackboard blackboard = null,
            Func<BTNode.NodeState> onBegin = null, Func<BTNode.NodeState> onUpdate = null,
            Func<BTNode.NodeState> onEnd = null)
        {
            this.owner = owner;
            this.blackboard = blackboard;

            currActionState = ActionState.Begin;

            this.onBegin = onBegin;
            this.onUpdate = onUpdate;
            this.onEnd = onEnd;
            this.onAbort = OnAbort;
        }


        public TaskNode(Func<BTNode.NodeState> onBegin = null,
            Func<BTNode.NodeState> onUpdate = null,
            Func<BTNode.NodeState> onEnd = null)
        {
            currActionState = ActionState.Begin;

            this.onBegin = onBegin;
            this.onUpdate = onUpdate;
            this.onEnd = onEnd;
            this.onAbort = OnAbort;
        }

        #endregion


        public void SetOwnerObject(GameObject owner)
        {
            this.owner = owner;
        }

        public void SetBlackboard(SO_Blackboard blackboard)
        {
            this.blackboard = blackboard;
        }

        //////////////////////////////////////////////////////////////////////////////////// 


        /// <summary>
        /// 액션 노드의 ActionState 값을 변경 시키는 함수 
        /// </summary>
        /// <param name="newState"></param>
        protected void ChangeActionState(ActionState newState) => currActionState = newState;

        public override NodeState Evaluate()
        {
            //Debug.Log($"Current Node Evaluate {nodeName} / {currActionState}");

            if (currActionState == ActionState.Begin)
                return onBegin?.Invoke() ?? BTNode.NodeState.Failure;
            else if (currActionState == ActionState.Update)
                return onUpdate?.Invoke() ?? BTNode.NodeState.Failure;
            else if (currActionState == ActionState.End)
                return onEnd?.Invoke() ?? BTNode.NodeState.Failure;

            return NodeState.Failure;
        }


        protected virtual BTNode.NodeState OnBegin()
        {

            ChangeActionState(ActionState.Update);
            return BTNode.NodeState.Running;
        }


        protected virtual BTNode.NodeState OnUpdate()
        {

            ChangeActionState(ActionState.End);
            return BTNode.NodeState.Running;
        }


        protected virtual BTNode.NodeState OnEnd()
        {
            ChangeActionState(ActionState.Begin);
            return BTNode.NodeState.Success;
        }

        protected virtual BTNode.NodeState OnAbort()
        {
            return BTNode.NodeState.Abort;
        }

        public void AbortTask()
        {
            onAbort?.Invoke();  
        }

        public override string ToString()
        {
            return onUpdate.Method.Name;
        }
    }

}