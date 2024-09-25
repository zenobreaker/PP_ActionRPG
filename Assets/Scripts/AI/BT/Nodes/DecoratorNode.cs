using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace AI.BT.Nodes
{
    public abstract class DecoratorNode
        : BTNode
    {
        public enum BB_KeyQuery
        {
            Equals = 0, NotEquals, LessThan, LessThanOrEqual, GreaterThan, GreaterThanOrEqual,
        }

        protected bool isRunning;
        protected BB_KeyQuery keyQuery;
        protected string boardKey;
        protected string key;
        //protected string keyValue;

        protected SO_Blackboard blackboard;

        protected BTNode childNode;
        public BTNode ChildNode => childNode;


        public DecoratorNode(string nodeName,
            BTNode childNode,
            GameObject owner = null,
            SO_Blackboard blackboard = null,
            string boardKey = null,
            string key = null,
            BB_KeyQuery keyQuery = BB_KeyQuery.Equals)
        //string key = null, string keyValue = default)
        {
            this.nodeName = nodeName;
            this.owner = owner;
            this.childNode = childNode;
            this.blackboard = blackboard;
            this.boardKey = boardKey;
            this.key = key;
            this.keyQuery = keyQuery;
            if (blackboard != null)
                blackboard.OnValueChanged += OnValueChanged;


            //this.keyQuery = keyQuery;
            //this.keyValue = keyValue;
        }

        public void SetOwnerObject(GameObject owner)
        {
            this.owner = owner;
        }

        public override NodeState Evaluate()
        {
            OnStart();

            NodeState result = NodeState.Failure;
            if (ShouldExecute())
            {
                result = childNode.Evaluate();
            }

            if (result != NodeState.Running)
                OnEnd();

            return result;
        }

        protected abstract bool ShouldExecute();

        protected virtual void OnStart()
        {
            isRunning = true;
        }
        protected virtual void OnEnd()
        {
            isRunning = false;
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


        //TODO: Task들이 변화할 때 호출될 이벤트
        //protected virtual void OnResultChanged()

        protected virtual void OnValueChanged(string changedKey)
        {

        }



        //protected virtual bool IsEquals(string keyValue)
        //{
        //    if (keyValue == null) return false;


        //    T originKeyValue = blackboard.GetValue<T>(this.keyValue);
        //    T targetKeyValue = blackboard.GetValue<T>(keyValue);

        //    if (originKeyValue.Equals(targetKeyValue))
        //        return true;
        //    else
        //        return false;
        //}

        //protected virtual bool IsNotEquals(string keyValue)
        //{
        //    T originKeyValue = blackboard.GetValue<T>(this.keyValue);
        //    T targetKeyValue = blackboard.GetValue<T>(keyValue);

        //    if (!originKeyValue.Equals(targetKeyValue))
        //        return true;
        //    else
        //        return false;
        //}


        //protected virtual bool IsLessThan(string keyValue)
        //{
        //    return true;
        //}

        //protected virtual bool IsLessThanOrEquals(T keyValue)
        //{
        //    return true;
        //}

        //protected virtual bool IsGreaterThan(T keyValue)
        //{
        //    return true;
        //}

        //protected virtual bool IsGreaterThanOrEquals(T keyValue)
        //{
        //    return true;
        //}

        public void AbortTask()
        {
            //Debug.Log($"Task Aboarted  {nodeName}");
            if (isRunning == false)
                return;
            Debug.Log($"{nodeName} Abort Call : {key}");
            // 자식들을 순회하면서 AbortTask 함수 실행
            if (childNode is CompositeNode composite)
            {
                composite.AbortTask();
            }
        }



        // 삭제시 구독 해제 
        ~DecoratorNode()
        {
            blackboard.OnValueChanged -= OnValueChanged;
        }
    }
}
