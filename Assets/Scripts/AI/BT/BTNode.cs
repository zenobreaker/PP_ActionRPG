using UnityEngine;


namespace AI.BT
{
    public abstract class BTNode
    {
        public enum NodeState { Running, Success, Failure, Abort, }
        protected NodeState state;

        public string nodeName;
        protected GameObject owner;
        

        public abstract NodeState Evaluate();

    }
}