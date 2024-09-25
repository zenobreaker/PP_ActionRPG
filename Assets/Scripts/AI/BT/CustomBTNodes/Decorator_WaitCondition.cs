using AI.BT.Nodes;
using UnityEngine;

namespace AI.BT.CustomBTNodes
{
    /// <summary>
    /// WaitCondition의 관련한 처리를 하는 Decorator
    /// WaitCondtion의 종속적이기 때문에 해당 enum을 쓰지 않으면 사용할 순 없다.
    /// </summary>
    public class Decorator_WaitCondition : DecoratorNode
    {
        BTAIController.WaitCondition waitCondition;
        BTAIController controller; 

        public Decorator_WaitCondition(string nodeName,
            BTNode childNode,
            GameObject owner = null, 
            SO_Blackboard blackboard = null, 
            string boardKey = null,
            string key = null, 
            BTAIController.WaitCondition waitCondition = BTAIController.WaitCondition.None) 
            : base(nodeName, childNode, owner, blackboard, boardKey, key)
        {
            controller = owner.GetComponent<BTAIController>();

            this.waitCondition = waitCondition;
        }

        public Decorator_WaitCondition(BTNode childNode, GameObject owner, BTAIController.WaitCondition waitCondition = BTAIController.WaitCondition.None)
            :base(null, childNode, owner)
        {
            controller = owner.GetComponent<BTAIController>();

            this.waitCondition = waitCondition;
        }

        protected override void OnEnd()
        {
            if (controller == null)
                return;

            // 컨디션이 같은 노드로 한해서만 처리하기
            if(controller.MyWaitCondition == waitCondition)
            {
                controller.SetWaitState_NoneCondition();
            }
        }

        protected override bool ShouldExecute()
        {
            if (controller == null)
                return false;

            return controller.MyWaitCondition == waitCondition;
        }
    }
}
