using AI.BT;
using System.Collections.Generic;


namespace AI.BT.Nodes
{
    public abstract class CompositeNode : BTNode
    {
        protected List<BTNode> children = new List<BTNode>();

        public List<BTNode> Children => children;

        protected bool hasFirstStart = true; 

        public void AddChild(BTNode node)
        {
            children.Add(node);
        }

        protected abstract void OnStart();
        
        protected abstract void OnEnd();

        // 하위 노드들의 Abort 명령 실행
        public void AbortTask()
        {
            foreach(BTNode node in children)
            {
                if (node is TaskNode taskNode)
                    taskNode.AbortTask();
                else if (node is DecoratorNode decoratorNode)
                    decoratorNode.AbortTask();
                else if( node is CompositeNode compositeNode)
                    compositeNode.AbortTask();
            }
        }

    }

}