using BT;

namespace Assets.Scripts.AI.BT.Blackboard
{
    [NotifyObserver]
    public enum NotifyObserver
    {
        OnResultChange = 0, OnValueChange
    }

    [ObserveAbort]
    public enum ObserveAborts
    {
        None, Selft, Lower_Priority, Both
    }

    [KeyQuery]
    public enum BB_KeyQuery
    {
        Equals = 0, NotEquals, LessThan, LessThanOrEqual, GreaterThan, GreaterThanOrEqual,
    }

    internal class Blackboard_Enum_Group
    {
    }
}
