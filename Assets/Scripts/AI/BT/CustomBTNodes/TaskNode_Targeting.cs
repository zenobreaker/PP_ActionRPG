using AI.BT.Helpers;
using AI.BT.Nodes;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UIElements;


namespace AI.BT.CustomBTNodes
{
    public class TaskNode_Targeting : TaskNode
    {

        private Coroutine lookAtTargetCoroutine;
        public TaskNode_Targeting(GameObject ownerObject, SO_Blackboard blackboard)
            : base(ownerObject, blackboard)
        {
            this.nodeName = "Targeting";

            onBegin = OnBegin;
            onUpdate = OnUpdate;
            onEnd = OnEnd;
            onAbort = OnAbort;
        }


        protected override NodeState OnBegin()
        {
            GameObject target = blackboard.GetValue<GameObject>("Target");
            if (target == null)
            {
                return NodeState.Failure;
            }

            Debug.Log($"{NodeName} already running ");
            // 적을 향해 회전 
            lookAtTargetCoroutine = CoroutineHelper.Instance.StartHelperCoroutine(LootAtTarget(target));
            return NodeState.Success;
        }


        private IEnumerator LootAtTarget(GameObject target)
        {
            if (target == null)
            {
                yield break;
            }


            Vector3 direction = target.transform.position - owner.transform.position;
            direction.Normalize();

            Vector3 forward = owner.transform.forward;

            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float rotateSpeed = 8.0f;
            float angle = Quaternion.Angle(owner.transform.rotation, targetRotation);

            while (angle < 0.2f)
            {
                owner.transform.rotation = Quaternion.Lerp(owner.transform.rotation,
                    targetRotation, Time.deltaTime * rotateSpeed);

                yield return null;
            }

            owner.transform.rotation = targetRotation;

            yield return null;
        }


        protected override NodeState OnAbort()
        {
            CoroutineHelper.Instance.StopHelperCoroutine(lookAtTargetCoroutine);

            return base.OnAbort();
        }

    }
}