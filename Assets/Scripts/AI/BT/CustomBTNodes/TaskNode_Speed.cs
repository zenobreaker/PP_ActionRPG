using AI.BT.Nodes;
using UnityEngine;


namespace AI.BT.CustomBTNodes
{
    public class TaskNode_Speed : TaskNode
    {
        private SpeedType speedType;
        private MovementComponent movement;

        public TaskNode_Speed(GameObject ownerObject, SO_Blackboard blackboard, SpeedType speedType)
            : base(ownerObject, blackboard)
        {

            nodeName = "Speed";

            this.speedType = speedType;
            movement = owner?.GetComponent<MovementComponent>();

            onBegin = OnBegin;

        }

        protected override NodeState OnBegin()
        {
            if (blackboard == null || movement == null)
                return NodeState.Failure;

            //Debug.Log($"속도 세팅 {speedType}");

            movement.SetSpeed(speedType);

            return NodeState.Success;
        }
    }

}