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

        private T key;
        public Decorator_Blackboard(string nodeName, BTNode childNode,
            GameObject owner = null,
            SO_Blackboard blackboard = null,
            string boardKey = null,
            T key = default(T),
            BB_KeyQuery keyQuery = BB_KeyQuery.Equals
           ) :
            base(nodeName, childNode, owner, blackboard, boardKey /*key, keyQuery*/)
        {
            this.keyQuery = keyQuery;
            this.key = key;
        }

        protected override void OnEnd()
        {
           base.OnEnd();
        }


        protected virtual bool CompareValueToQuery<U>(string changedKey)
        {
            if (blackboard == null)
                return false;

            //Debug.Log($"{nodeName} == Compare {key}");
            switch (keyQuery)
            {
                case BB_KeyQuery.Equals:
                return blackboard.CompareValue(changedKey, key);

                case BB_KeyQuery.NotEquals:
                return blackboard.CompareValue(changedKey, key) == false;

                case BB_KeyQuery.GreaterThan:
                return blackboard.GreaterThanValue(changedKey, key);

                case BB_KeyQuery.LessThan:
                return blackboard.LessThanValue(changedKey, key);

                case BB_KeyQuery.LessThanOrEqual:
                {
                    bool bCheck = false;
                    bCheck |= blackboard.LessThanValue(changedKey, key);
                    bCheck |= blackboard.CompareValue(changedKey, key);
                    return bCheck;
                }
                case BB_KeyQuery.GreaterThanOrEqual:
                {
                    bool bCheck = false;
                    bCheck |= blackboard.GreaterThanValue(changedKey, key);
                    bCheck |= blackboard.CompareValue(changedKey, key);
                    return bCheck;
                }
            }

            return false;
        }

        //TODO: 아래 이벤트용 함수는 정리가 필요하다.
        protected override void OnValueChanged(string changedKey)
        {
            if (isRunning == false)
                return; 

            // 여기에 등록된 키값이랑 값이 변경되는 키값이랑 같은 경우 
            if (changedKey == boardKey)
            {
                //Debug.Log($"{nodeName} +  + Examine {changedKey}");
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

        public override void StopEvaluate()
        {
            isRunning = false;
            childNode.StopEvaluate();
        }
    }
}