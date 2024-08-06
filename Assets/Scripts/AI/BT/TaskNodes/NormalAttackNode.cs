using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalAttackNode : BTNode
{

    public NormalAttackNode()
    {

    }


    public override NodeState Evaluate()
    {
        return NodeState.Failure;
    }

}
