using System.Collections;
using UnityEngine;

namespace Assets.Scripts.AI.BT
{
    public class ConditionNode : BTNode
    {
        public ConditionNode(GameObject owner) : base(owner)
        {
        }

        public override NodeState Evaluate()
        {
            throw new System.NotImplementedException();
        }
    }
}