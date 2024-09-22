using AI.BT.Nodes;
using UnityEngine;

namespace AI.BT
{
    public abstract class BehaviorTree : MonoBehaviour
    {
        [SerializeField] protected  SO_Blackboard so_blackboard;
        protected SO_Blackboard blackboard;
        public SO_Blackboard Blackboard { get => blackboard; }

        public abstract void CreateBlackboardKey();
        public abstract RootNode CreateBehaviorTree();
    }
}
