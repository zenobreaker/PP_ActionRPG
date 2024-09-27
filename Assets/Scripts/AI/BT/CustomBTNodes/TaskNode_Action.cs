using AI.BT.Nodes;
using UnityEngine;

namespace AI.BT.CustomBTNodes
{
    public class TaskNode_Action : TaskNode
    {

        StateComponent state;
        IActionComponent action; 

        public TaskNode_Action(GameObject owner, SO_Blackboard blackboard)
            :base(owner, blackboard)
        {
            this.nodeName = "Action";

            action = owner.GetComponent<IActionComponent>();
            state = owner.GetComponent<StateComponent>();
            onBegin = OnBegin;
            onUpdate = OnUpdate;
            onEnd = OnEnd;
            onAbort = OnAbort;
        }


        protected override NodeState OnBegin()
        {
            if(action == null)
            {
                return NodeState.Failure;
            }
            Debug.Log($"{nodeName} Action Node Begin ");

            ChangeActionState(ActionState.Update);
            action.DoAction();

            return NodeState.Running;
        }


        protected override NodeState OnUpdate()
        {
            if(state == null)
            {
                ChangeActionState(ActionState.Begin);
                return NodeState.Failure;
            }

            //if (!owner.TryGetComponent<WeaponComponent>(out WeaponComponent weapon))
            //{
            //    ChangeActionState(ActionState.Begin);
            //    return NodeState.Failure;
            //}
            
            //Debug.Log("Action Node Running");

            bool bCheck = true;
            bCheck &= state.IdleMode;

            if (bCheck == false)
                return NodeState.Running;

            return base.OnUpdate();
        }

        protected override NodeState OnEnd()
        {
            return base.OnEnd();
        }


        protected override NodeState OnAbort()
        {


            return base.OnAbort();
        }
    }
}
