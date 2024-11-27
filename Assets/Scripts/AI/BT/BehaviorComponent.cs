using BT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviorComponent : MonoBehaviour
{
    [SerializeField] private BehaviorTree tree; 

    private void Start()
    {
        tree = tree.Clone();
    }

    private void Update()
    {
        tree?.Evaluate();
    }
}
