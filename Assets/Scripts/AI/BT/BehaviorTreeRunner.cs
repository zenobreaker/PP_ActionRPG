using AI.BT.Nodes;
using UnityEditor.Rendering.Universal;
using UnityEngine;

namespace AI.BT
{
    public class BehaviorTreeRunner
    {
        private RootNode rootNode;

        public BehaviorTreeRunner(RootNode rootNode)
        {
            this.rootNode = rootNode;
        }

        public void OperateNode()
        {
            rootNode.Evaluate();
        }
        public void OperateNode(bool debugMode)
        {
            OperateNode();
        }
    }

}