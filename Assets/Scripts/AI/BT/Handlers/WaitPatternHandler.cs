using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitPatternHandler 
    : MonoBehaviour
    , IPatternHandler
{
    public int decidedPattern;

    public int GetPattern()
    {
        return decidedPattern;
    }

    public void SetPattern(int pattern)
    {
        decidedPattern = pattern;   
    }

}
