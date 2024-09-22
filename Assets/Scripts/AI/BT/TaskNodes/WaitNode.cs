using AI.BT.Nodes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AI.BT.TaskNodes
{
    public class WaitNode : TaskNode
    {
        [SerializeField] private float waitTime = 1.0f;
        public float WaitTime { set => waitTime = value; }
        [SerializeField] private float waitRandDelay = 0;
        public float WaitRandDelay { set => waitRandDelay = value; }

        private float currentWaitTime;
        public float CurrentWaitTime { get => currentWaitTime; }

        public WaitNode()
            : base(null, null, null)
        {
            onBegin = OnBegin;
            onUpdate = OnUpdate;
            onEnd = OnEnd;
        }

        public WaitNode(float waitTime, float waitRandDelay)
            : base(null, null, null)
        {
            nodeName = "Wait";

            this.waitTime = waitTime;
            this.waitRandDelay = waitRandDelay;

            onBegin = OnBegin;
            onUpdate = OnUpdate;
            onEnd = OnEnd;
        }



        protected override BTNode.NodeState OnBegin()
        {
            //Debug.Log("Wait Node Begin");
            currentWaitTime = Random.Range(waitTime + (-1.0f * waitRandDelay),
                waitTime + (+1.0f * waitRandDelay));

            return base.OnBegin();
        }


        protected override BTNode.NodeState OnUpdate()
        {
            currentWaitTime -= Time.deltaTime;

            if (currentWaitTime > 0)
            {
                //Debug.Log($"{nodeName} Update ");
                return BTNode.NodeState.Running;
            }

            return base.OnUpdate();
        }


        protected override BTNode.NodeState OnEnd()
        {
            //Debug.Log("Wait Node End");
             
            return base.OnEnd();
        }
    }
}