using AI.BT.Helpers;
using AI.BT.Nodes;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

namespace AI.BT
{
    public class BehaviorTreeRunner
    {
        public BehaviorTree tree; 

        private GameObject owner;
        private MonoBehaviour ownerMBH; 
        private RootNode rootNode;
        private SO_Blackboard blackboard;
        private Coroutine btRunCoroutine;
        //private bool isRunning = false; 
        public BehaviorTreeRunner(GameObject owner, SO_Blackboard blackboard, RootNode rootNode)
        {
            this.owner = owner;
            this.blackboard = blackboard;
            if(owner != null)
            {
                ownerMBH = owner.GetComponent<MonoBehaviour>();
            }

            this.rootNode = rootNode;
        }

        public void RunBehaviorTree(float interval = -1.0f, bool debugMode = false)
        {
            if (ownerMBH == null)
                return;

            if(blackboard != null)
            {
                blackboard.Initialize();
            }

            //isRunning = true;
            if (btRunCoroutine == null)
            {
                Debug.Log("Run Behaivor !!");
                btRunCoroutine = ownerMBH.StartCoroutine(StartBTRunCoroutine(debugMode, interval));
            }
        }

        public void StopBehaviorTree()
        {
            if (ownerMBH == null)
                return;

            CoroutineHelper.Instance.StopAllCoroutines();
            if(btRunCoroutine != null)
                ownerMBH.StopCoroutine(btRunCoroutine);
            StopEvaluate();
        }

        public void StopEvaluate()
        {
            rootNode.StopEvaluate();
        }

        public void OperateNode()
        {
            rootNode.Evaluate();
        }

        public void OperateNode(bool debugMode)
        {
            OperateNode();
        }

        public IEnumerator StartBTRunCoroutine(bool debugMode = false, float interval = -1.0f)
        {
            while(true)
            {
                if(interval > 0.0f)
                    yield return new WaitForSeconds(interval);

                OperateNode(debugMode);

                yield return null;
            }
        }
    }

}