using System;


namespace BT
{

    [AttributeUsage(AttributeTargets.Enum, Inherited = false)]
    public class KeyQueryAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Enum, Inherited = false)]
    public class ObserveAbortAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Enum, Inherited = false)]
    public class NotifyObserverAttribute : Attribute { }
}