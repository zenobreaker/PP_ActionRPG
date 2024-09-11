using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaitNode : ActionNode
{
    [SerializeField] private float waitTime = 1.0f;
    [SerializeField] private float waitRandDelay = 1.0f;

    private float currentWaitTime; 

    public WaitNode() 
        : base(null)
    {
        onBegin = OnBegin;
        onUpdate = OnUpdate;
        onEnd = OnEnd;
    }



    protected override BTNode.NodeState OnBegin()
    {
        Debug.Log("Wait Node Begin");
        currentWaitTime = Random.Range(waitTime + (-1.0f * waitRandDelay), 
            waitTime + (+1.0f * waitRandDelay));

        return base.OnBegin(); 
    }


    protected override BTNode.NodeState OnUpdate()
    {
        currentWaitTime -= Time.deltaTime;

        if (currentWaitTime > 0)
        {
            Debug.Log("Wait Node Update");
            return BTNode.NodeState.Running;
        }

        return base.OnUpdate();
    }


    protected override BTNode.NodeState OnEnd()
    {
        Debug.Log("Wait Node End");

        return base.OnEnd();
    }
}
