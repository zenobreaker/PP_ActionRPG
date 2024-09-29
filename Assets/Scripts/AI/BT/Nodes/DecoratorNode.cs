using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace AI.BT.Nodes
{
    public abstract class DecoratorNode
        : BTNode
    {

        protected bool isRunning;
        protected string boardKey;
        //protected string keyValue;

        protected SO_Blackboard blackboard;

        protected BTNode childNode;
        public BTNode ChildNode => childNode;


        public DecoratorNode(string nodeName,
            BTNode childNode,
            GameObject owner = null,
            SO_Blackboard blackboard = null,
            string boardKey = null)
            //B_KeyQuery keyQuery = BB_KeyQuery.Equals)
        //string key = null, string keyValue = default)
        {
            this.nodeName = nodeName;
            this.owner = owner;
            this.childNode = childNode;
            this.blackboard = blackboard;
            this.boardKey = boardKey;
            //this.keyQuery = keyQuery;
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

       

        //TODO: Task들이 변화할 때 호출될 이벤트
        //protected virtual void OnResultChanged()

        protected virtual void OnValueChanged(string changedKey)
        {

        }

        public void AbortTask()
        {
            //Debug.Log($"Task Aboarted  {nodeName}");
            if (isRunning == false)
                return;
            // 자식들을 순회하면서 AbortTask 함수 실행
            if (childNode is CompositeNode composite)
            {
                composite.AbortTask();
            }
            else if(childNode is TaskNode taskNode)
            {
                taskNode.AbortTask();
            }
        }

        // 삭제시 구독 해제 
        ~DecoratorNode()
        {
            blackboard.OnValueChanged -= OnValueChanged;
        }
    }
}
