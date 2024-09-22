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

        public void AbortTask()
        {
            foreach(BTNode node in children)
            {
                if (node is TaskNode taskNode)
                    taskNode.AbortTask();
                else if (node is DecoratorNode decoratorNode)
                    decoratorNode.AbortTask();
            }
        }
    }

}