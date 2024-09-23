using UnityEngine;

namespace AI.BT.Nodes
{
    public abstract class DecoratorNode
        : BTNode
    {
        public enum BB_KeyQuery
        {
            Equals = 0, NotEquals, LessThan, LessThanOrEqual, GreaterThan, GreateThanOrEqual,
        }

        //protected BB_KeyQuery keyQuery;
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
            string key = null)
            //BB_KeyQuery keyQuery = BB_KeyQuery.Equals,
            //string key = null, string keyValue = default)
        {
            this.nodeName = nodeName;
            this.owner = owner;
            this.childNode = childNode;
            this.blackboard = blackboard;
            this.boardKey = boardKey;
            this.key = key;

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
            if (ShouldExecute())
            {
                return childNode.Evaluate();
            }

            return NodeState.Failure;
        }

        protected abstract bool ShouldExecute();


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

            // 자식들을 순회하면서 AbortTask 함수 실행
            if(childNode is CompositeNode composite)
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
