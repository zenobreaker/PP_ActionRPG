using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApproachNode : ActionNode
{
    public ApproachNode(Func<NodeState> onUpdate) : base(onUpdate)
    {
    }
}
