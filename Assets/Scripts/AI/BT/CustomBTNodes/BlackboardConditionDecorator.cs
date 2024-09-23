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
        protected Predicate<T> condition;

        public BlackboardConditionDecorator(string nodeName, BTNode childNode,
            GameObject owner = null,
            SO_Blackboard blackboard = null,
            string boardKey = null,
            string key = null,
            Predicate<T> condition = null) :
            base(nodeName, childNode, owner, blackboard, boardKey, key)
        {
            this.condition = condition;
        }

        //TODO: 아래 이벤트용 함수는 정리가 필요하다.
        protected override void OnValueChanged(string changedKey)
        {
            // 여기에 등록된 키값이랑 값이 변경되는 키값이랑 같은 경우 
            if (changedKey == boardKey)
            {
                T value = blackboard.GetValue<T>(boardKey);
                //Debug.Log($"here to condition check previous : {value} / {key}");
                //T myValue = blackboard.GetValue<T>(key); 
                //TODO: 제네릭에 대한 정리가 안되어 잇는 상태 
                //if (!value.Equals(myValue))
                if(condition?.Invoke(value) == false)
                {
                    //Debug.Log($"here to condition after : {myValue} / {key}");
                    AbortTask();
                }
            }
        }

        protected override bool ShouldExecute()
        {
            T value = blackboard.GetValue<T>(key);
           
            return condition(value);
        }

    }
}