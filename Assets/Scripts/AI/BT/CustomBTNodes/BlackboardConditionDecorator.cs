using AI.BT.Nodes;
using System;
using UnityEngine;

namespace AI.BT.CustomBTNodes
{
    /// <summary>
    /// 데코레이터를 상속받는 클래스이다 
    /// 블랙보드와 연동을 주로 처리한다.
    /// </summary>
    public class BlackboardConditionDecorator<T> : DecoratorNode
    {
        //public enum BB_KeyQuery
        //{
        //    Equals = 0, NotEquals, LessThan, LessThanOrEqual, GreaterThan, GreateThanOrEqual,
        //}

        //private BB_KeyQuery keyQuery;
        //private T value;

        
        private SO_Blackboard blackboard;
        private string key;
        private Func<T, bool> condition;

        public BlackboardConditionDecorator(string nodeName, BTNode childNode,
            GameObject owner = null,
            SO_Blackboard blackboard = null,
            string key = null,
            Func<T, bool> condition = null) :
            base(nodeName, childNode, owner)
        {
            
            this.blackboard = blackboard;
            this.key = key;
            this.condition = condition;
        }

        //public BlackboardConditionDecorator(BTNode childNode,
        //   GameObject owner = null,
        //   SO_Blackboard blackboard = null,
        //   string key = null,
        //   T value) :
        //   base(childNode, owner)
        //{
        //    this.blackboard = blackboard;
        //    this.key = key;
        //    this.value = value;
        //}


        protected override bool ShouldExecute()
        {
            T value = blackboard.GetValue<T>(key);

            Debug.Log($"{nodeName} What this that {key} / {value}");
            return condition(value);
        }

    
    }
}