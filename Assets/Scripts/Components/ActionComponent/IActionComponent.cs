using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActionComponent 
{
    public event Action OnBeginDoAction;
    public event Action OnEndDoAction;

    public void DoAction();

    public void End_DoAction();
}
