using UnityEngine;

public abstract class AnimState : MonoBehaviour
{
    [SerializeField] protected float stateBeginTime;
    [SerializeField] protected float stateEndTime;

    protected virtual void Start()
    {
        
    }

    protected virtual void StateBegin()
    {

    }

    protected virtual void StateEnd()
    {

    }
}
