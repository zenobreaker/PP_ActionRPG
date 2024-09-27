using AI.BT.Nodes;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace AI.BT.CustomBTNodes
{
    /// <summary>
    /// Pattern에 따라 일정한 수치들을 결정 해주는 노드 
    /// </summary>
    public class TaskNode_SetupPattern : TaskNode
    {
        private int pattern;
        private IPatternHandler patternHandler;

        public TaskNode_SetupPattern(GameObject ownerObject, int pattern)
            : base(ownerObject)
        {
            this.nodeName = "SetupPattern";
            this.pattern = pattern;
            patternHandler = owner.GetComponent<IPatternHandler>(); 

            onBegin = OnBegin;
            onUpdate = OnUpdate;
            onEnd = OnEnd;
            onAbort = OnAbort;
        }


        protected override NodeState OnBegin()
        {
            if (patternHandler == null)
                return NodeState.Failure;

            Debug.Log($"Pattern Set  {pattern} ");
            patternHandler.SetPattern(pattern);

            return NodeState.Success;
        }

        protected override NodeState OnAbort()
        {
            //Debug.Log("Equip Abort !!");

            return base.OnAbort();
        }
    }
}