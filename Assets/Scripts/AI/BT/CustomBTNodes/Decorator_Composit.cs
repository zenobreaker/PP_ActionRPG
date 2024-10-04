using AI.BT.Nodes;
using System;
using UnityEngine;

namespace AI.BT.CustomBTNodes
{
    /// <summary>
    /// TODO: 관찰자를 추가해서 해당 관찰자 상태에 따라 로직이 틀리면 바로 끝낼지 말지 추가해야할 듯
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

            bool result = blackboard.CompareValue(boardKey, key);
            return result;
        }

        public override void StopEvaluate()
        {
            isRunning = false;
            childNode.StopEvaluate();
        }
    }
}
