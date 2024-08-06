using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.MaterialProperty;

public class WaitNode: BTNode 
{

    private float delayTime = 2.0f;
    private float delayRandom = 0.5f;
    private float maxWaitTime = 0.0f; 
    private float currentTime = 0.0f;

    private bool bRandom = false; 
    private bool bInit = false; 

    private BTAIController controller;

    //public WaitNode(Func<NodeState> onUpdate) : base(onUpdate)
    //{
    //}


    private event Action OnEndWaitAction;
    public WaitNode(BTAIController controller, float waitTime, bool bRandom = false, Action action = null)
    {
        this.controller = controller;
        this.delayTime = waitTime;
        this.bRandom = bRandom;

        if (action != null)
            OnEndWaitAction += action;
    }


    public override BTNode.NodeState Evaluate()
    {
        if (controller == null)
            return BTNode.NodeState.Failure;

        ResetTime();

        currentTime += Time.deltaTime;
        if (currentTime >= maxWaitTime)
        {
            bInit = false;
            maxWaitTime = 0.0f;
            OnEndWaitAction?.Invoke();
            return BTNode.NodeState.Success; // 
        }

        return BTNode.NodeState.Running;
    }

    private void ResetTime()
    {
        if (bInit)
            return;

        controller.SetWaitMode();

        bInit = true;
        currentTime = 0.0f;
        maxWaitTime += delayTime;
        if (bRandom)
            maxWaitTime += UnityEngine.Random.Range(-delayRandom, delayRandom);
    }
}
