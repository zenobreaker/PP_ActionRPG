using AI.BT.Nodes;
using UnityEngine;

namespace AI.BT.CustomBTNodes
{
    /// <summary>
    /// 데코레이터를 상속받는 클래스이다 
    /// 블랙보드와 연동을 주로 처리한다.
    /// </summary>
    public class BlackboardConditionDecorator<T> : DecoratorNode
    {

        public BlackboardConditionDecorator(string nodeName, BTNode childNode,
            GameObject owner = null,
            SO_Blackboard blackboard = null,
            string boardKey = null,
            string key = null,
            BB_KeyQuery keyQuery = BB_KeyQuery.Equals
           ) :
            base(nodeName, childNode, owner, blackboard, boardKey, key, keyQuery)
        {
            
        }

        protected override void OnEnd()
        {
           base.OnEnd();
        }

        //TODO: 아래 이벤트용 함수는 정리가 필요하다.
        protected override void OnValueChanged(string changedKey)
        {
            // 여기에 등록된 키값이랑 값이 변경되는 키값이랑 같은 경우 
            if (changedKey == boardKey)
            {
                // 비교 했을 때 다르다면?
                if(CompareValueToQuery<T>(changedKey) == false)
                {
                    AbortTask();
                }
                
            }
        }

        protected override bool ShouldExecute()
        {
            //T value = blackboard.GetValue<T>(key);

            bool result = CompareValueToQuery<T>(boardKey);

            return result;
        }

    }
}