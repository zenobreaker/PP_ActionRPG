using BT.Nodes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BT.CustomBTNodes
{
    public class TaskNode_Unequip : TaskNode
    {

        private WeaponComponent weapon;
        private StateComponent stateComponent; 

        public TaskNode_Unequip(GameObject ownerObject, SO_Blackboard blackboard)
            : base(ownerObject, blackboard)
        {
            this.nodeName = "Unequip";


            weapon = ownerObject.GetComponent<WeaponComponent>();
            stateComponent = ownerObject.GetComponent<StateComponent>();

            onBegin = OnBegin;
            onUpdate = OnUpdate;
            onEnd = OnEnd;
            onAbort = OnAbort;
        }


        protected override NodeState OnBegin()
        {
            if (weapon == null)
            {
                return NodeState.Failure;
            }
            
            // 이미 해제된 상태면 성공 처리  
            if (weapon.Type == WeaponType.Unarmed)
                return NodeState.Success;

            weapon.SetUnarmedMode();

            ChangeActionState(ActionState.Update);
            return NodeState.Running;
        }

        protected override NodeState OnUpdate()
        {
            if (weapon == null)
            {
                return NodeState.Failure;
            }

            if (stateComponent == null)
            {
                return NodeState.Failure;
            }

            bool bCheck = true; 
            bool bEquippd = weapon.IsEquipped();
            bool bIdle = stateComponent.IdleMode;
            bCheck = bEquippd == false && bIdle;
            

            if (bCheck)
            {
                //ChangeActionState (ActionState.Begin);
                return NodeState.Success;
            }

            return NodeState.Running;
        }


        protected override NodeState OnAbort()
        {
            //if (weapon == null)
            //{
            //    ChangeActionState(ActionState.End);
            //    return NodeState.Abort;
            //}

            //bool bEquippd = weapon.IsEquipped();
            //// 장착이 완료가 된 상태가 아니라면
            //if (bEquippd )
            //    weapon.SetUnarmedMode();

            return base.OnAbort();
        }
    }
}