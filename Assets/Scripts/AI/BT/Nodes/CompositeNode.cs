using AI.BT;
using System.Collections.Generic;


namespace AI.BT.Nodes
{
    public abstract class CompositeNode : BTNode
    {
        protected List<BTNode> children = new List<BTNode>();

        public List<BTNode> Children => children;

        public void AddChild(BTNode node)
        {
            children.Add(node);
        }
    }

}