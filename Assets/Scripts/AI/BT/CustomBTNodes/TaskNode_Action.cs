using AI.BT.Nodes;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace AI.BT.CustomBTNodes
{
    public class TaskNode_Action : TaskNode
    {
        public TaskNode_Action(GameObject owner, SO_Blackboard blackboard)
            :base(owner, blackboard)
        {
            this.nodeName = "Action";

            onBegin = OnBegin;
            onUpdate = OnUpdate;
            onEnd = OnEnd;
            onAbort = OnAbort;
        }


        protected override NodeState OnBegin()
        {
            if(!owner.TryGetComponent<WeaponComponent>(out WeaponComponent weapon))
            {
                return NodeState.Failure;
            }
            //Debug.Log("Action Node Begin ");

            ChangeActionState(ActionState.Update);
            weapon.DoAction();

            return NodeState.Running;
        }


        protected override NodeState OnUpdate()
        {
            if(!owner.TryGetComponent<StateComponent>(out StateComponent state))
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
