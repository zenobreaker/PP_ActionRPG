using UnityEngine;


namespace AI.BT
{
    public abstract class BTNode : ScriptableObject
    {
        public enum NodeState { Running, Success, Failure, Abort, }
        protected NodeState nodeState;

        protected string nodeName;
        public string NodeName { get => nodeName; set => nodeName = value; }

        #region Editor
        public string guid;
        public Vector2 position;


        #endregion
        protected GameObject owner;
     

        public virtual BTNode Clone()
        { 
            return Instantiate(this);
        }

        public abstract NodeState Evaluate();

        public abstract void StopEvaluate();

    }
}