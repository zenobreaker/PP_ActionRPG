using AI.BT.Helpers;
using AI.BT.Nodes;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace AI.BT.CustomBTNodes
{
    /// <summary>
    /// </summary>
    public class Decorator_CoolDown : DecoratorNode
    {
        BTAIController controller;

        private float maxCoolDown;
        private float coolDown;
        private Coroutine coolDownCoroutine;
        private bool isCoolDown = false;
        public Decorator_CoolDown(string nodeName,
            BTNode childNode,
            GameObject owner = null,
            SO_Blackboard blackboard = null,
            float maxCoolDown = 0)
            : base(nodeName, childNode, owner, blackboard)
        {
            controller = owner.GetComponent<BTAIController>();
            this.maxCoolDown = maxCoolDown;
            coolDown = -this.maxCoolDown;
        }

        public Decorator_CoolDown(BTNode childNode, GameObject owner,
            float maxCoolDown)
            : base(null, childNode, owner)
        {
            controller = owner.GetComponent<BTAIController>();
            this.maxCoolDown = maxCoolDown;
        }

        protected override void OnStart()
        {
            Debug.Log($"{nodeName} Decorator_CoolDown");
            if (isRunning == false)
            {
                if (coolDownCoroutine == null && isCoolDown == false)
                {
                    isCoolDown = true;
                    coolDownCoroutine = CoroutineHelper.Instance.StartHelperCoroutine(CoolDownCoroutine());
                }
            }

            base.OnStart();
        }


        protected override void OnEnd()
        {
            if (controller == null)
                return;

            isRunning = false;
            if (coolDownCoroutine != null && isCoolDown == true)
            {
                coolDownCoroutine = null;
            }
        }

        protected override bool ShouldExecute()
        {
            if (controller == null)
                return false;

            Debug.Log($"{nodeName} Decorator_CoolDown Complete");
            isCoolDown = false;
            return coolDown <= 0;
        }

        private IEnumerator CoolDownCoroutine()
        {
            coolDown = maxCoolDown;
            yield return new WaitForSeconds(maxCoolDown);
            coolDown = 0.0f;
        }

    }
}
