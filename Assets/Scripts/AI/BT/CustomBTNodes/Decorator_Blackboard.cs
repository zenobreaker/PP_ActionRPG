using AI.BT.Nodes;
using UnityEngine;

namespace AI.BT.CustomBTNodes
{
    /// <summary>
    /// 데코레이터를 상속받는 클래스이다 
    /// 블랙보드와 연동을 주로 처리한다.
    /// </summary>
    public class Decorator_Blackboard<T> : DecoratorNode
    {
        public enum BB_KeyQuery
        {
            Equals = 0, NotEquals, LessThan, LessThanOrEqual, GreaterThan, GreaterThanOrEqual,
        }
        protected BB_KeyQuery keyQuery;

        public Decorator_Blackboard(string nodeName, BTNode childNode,
            GameObject owner = null,
            SO_Blackboard blackboard = null,
            string boardKey = null,
            string key = null,
            BB_KeyQuery keyQuery = BB_KeyQuery.Equals
           ) :
            base(nodeName, childNode, owner, blackboard, boardKey, key/*, keyQuery*/)
        {
            this.keyQuery = keyQuery;
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

         protected virtual bool CompareValueToQuery<T>(string changedKey)
        {
            if (blackboard == null)
                return false; 

            T value = blackboard.GetValue<T>(key);

            switch (keyQuery)
            {
                case BB_KeyQuery.Equals:
                return blackboard.CompareValue(boardKey, value);
                
                case BB_KeyQuery.NotEquals:
                return blackboard.CompareValue(boardKey, value) == false;
                
                case BB_KeyQuery.GreaterThan:
                return blackboard.GreaterThanValue(boardKey, value);
                
                case BB_KeyQuery.LessThan:
                return blackboard.LessThanValue(boardKey, value);

                case BB_KeyQuery.LessThanOrEqual:
                {
                    bool bCheck = false;
                    bCheck |= blackboard.LessThanValue(boardKey, value);
                    bCheck |= blackboard.CompareValue(boardKey, value);
                    return bCheck;
                }
                case BB_KeyQuery.GreaterThanOrEqual:
                {
                    bool bCheck = false;
                    bCheck |= blackboard.GreaterThanValue(boardKey, value);
                    bCheck |= blackboard.CompareValue(boardKey, value);
                    return bCheck;
                }
            }

            return false;
        }


    }
}