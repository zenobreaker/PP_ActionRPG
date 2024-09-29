using UnityEngine;


namespace AI.BT
{
    public abstract class BTNode
    {
        public enum NodeState { Running, Success, Failure, Abort, }
        protected NodeState nodeState;

        protected string nodeName;
        public string NodeName { get => nodeName; set => nodeName = value; }
        protected GameObject owner;
     
        public abstract NodeState Evaluate();

        public abstract void StopEvaluate();

    }
}