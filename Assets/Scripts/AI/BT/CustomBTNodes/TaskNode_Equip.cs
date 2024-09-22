using AI.BT.Nodes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AI.BT.CustomBTNodes
{
    public class TaskNode_Equip : TaskNode
    {
        private WeaponType weaponType = WeaponType.Unarmed; 

        public TaskNode_Equip(GameObject ownerObject, SO_Blackboard blackboard, 
            WeaponType weaponType)
            : base(ownerObject, blackboard)
        {
            this.nodeName = "Equip";

            this.weaponType = weaponType;

            onBegin = OnBegin;
            onUpdate = OnUpdate;
            onEnd = OnEnd;
            onAbort = OnAbort;
        }


        protected override NodeState OnBegin()
        {
            if (!owner.TryGetComponent<WeaponComponent>(out var weapon))
            {
                ChangeActionState(ActionState.End);
                return NodeState.Failure;
            }

            if (weaponType == WeaponType.Unarmed)
            {
                ChangeActionState(ActionState.End);
                return NodeState.Failure;
            }

            // 이미 장착되어 있으면 넘김 
            if (weaponType == weapon.Type)
                return NodeState.Success;

            // 장착 시도 
            switch(weaponType)
            {
                case WeaponType.Fist:
                weapon.SetFistMode();
                break;
                case WeaponType.Sword:
                weapon.SetSwordMode();
                break;
                case WeaponType.Hammer:
                weapon.SetHammerMode();
                break;
                case WeaponType.FireBall:
                weapon.SetFireBallMode();
                break;
            }

            ChangeActionState(ActionState.Update);
            
            return NodeState.Running;
        }

        protected override NodeState OnUpdate()
        {
            if (!owner.TryGetComponent<WeaponComponent>(out var weapon))
            {
                ChangeActionState(ActionState.End);
                return NodeState.Failure;
            }

            if (!owner.TryGetComponent<StateComponent>(out var state))
            {
                ChangeActionState(ActionState.End);
                return NodeState.Failure;
            }

            bool bCheck = true; 
            bool bEquippd = weapon.IsEquipped();
            bool bIdle = state.IdleMode;
            bCheck = bEquippd && bIdle;

            ChangeActionState(ActionState.Begin);

            if (bCheck)
            {
                return NodeState.Success;
            }

            return NodeState.Failure;
        }


        protected override NodeState OnAbort()
        {
            //Debug.Log("Equip Abort !!");

            if (!owner.TryGetComponent<WeaponComponent>(out var weapon))
            {
                ChangeActionState(ActionState.End);
                return NodeState.Abort;
            }

            bool bEquippd = weapon.IsEquipped();
            // 장착이 완료가 된 상태가 아니라면
            if (bEquippd == false)
                weapon.Begin_Equip();

            weapon.End_Equip();


            return base.OnAbort();
        }
    }
}