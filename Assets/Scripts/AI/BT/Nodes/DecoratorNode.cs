using UnityEngine;

namespace AI.BT.Nodes
{
    public abstract class DecoratorNode : BTNode
    {
        public string nodeName;
        private GameObject owner;
        private BTNode childNode;
        public BTNode ChildNode => childNode;

        public DecoratorNode(string nodeName, BTNode childNode, GameObject owner = null)
        {
            this.nodeName = nodeName;
            this.owner = owner;
            this.childNode = childNode;
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
    }
}
