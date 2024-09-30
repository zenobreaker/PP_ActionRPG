using AI.BT.Nodes;
using System;
using UnityEngine;

namespace AI.BT.CustomBTNodes
{
    /// <summary>
    /// WaitCondition의 관련한 처리를 하는 Decorator
    /// WaitCondtion의 종속적이기 때문에 해당 enum을 쓰지 않으면 사용할 순 없다.
    /// </summary>
    public class Decorator_Composit<T> : DecoratorNode where T : IComparable
    {
        BTAIController controller;

        T key;

        public Decorator_Composit(string nodeName,
            BTNode childNode,
            GameObject owner = null, 
            SO_Blackboard blackboard = null,
            string boardKey = null, 
            T key = default(T)) 
            : base(nodeName, childNode, owner, blackboard, boardKey)
        {
            controller = owner.GetComponent<BTAIController>();
            this.key = key;
        }


        protected override bool ShouldExecute()
        {
            if (controller == null)
                return false;
            return blackboard.CompareValue(boardKey, key);
        }

        public override void StopEvaluate()
        {
            isRunning = false;
            childNode.StopEvaluate();
        }
    }
}
