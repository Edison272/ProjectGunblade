using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

// simple unused container of type of subscribers
public abstract class EventSlot
{
    public abstract Type delegateType { get; }
    protected readonly InputEventRelay inputRelay;
    protected Delegate relayCallback; // other delegates can connect to this
    protected readonly List<Delegate> _subscribers = new List<Delegate>();

    // slot control
    public bool IsActive = true; // used to individually toggle an event slot. used by the inputEventRelay


    public EventSlot(InputEventRelay newInputRelay)
    {
        inputRelay = newInputRelay;
    }

    // creat the generic type using the delegate typeId li
    public static EventSlot CreateSlot(Type delegateType, InputEventRelay newInputRelay)
    {
        EventSlot new_event = null;
        Type[] args = delegateType.GetGenericArguments();
        if (delegateType == typeof(Action))
        {
            new_event = new ActionSlot(newInputRelay);
        }
        else if (delegateType.GetGenericTypeDefinition() == typeof(Action<>))
        {
            Type slotType = typeof(ActionSlot<>).MakeGenericType(args);
            new_event = (EventSlot)Activator.CreateInstance(slotType, newInputRelay);
        }
        else if (delegateType.GetGenericTypeDefinition() == typeof(Action<,>))
        {
            Type slotType = typeof(ActionSlot<>).MakeGenericType(args);
            new_event = (EventSlot)Activator.CreateInstance(slotType, newInputRelay);
        }
        return new_event;
    }
    public virtual bool Add(Delegate callback, Type callbackType = null)
    {
        if (callbackType == null)
            callbackType = callback.GetType();
        if (callbackType != delegateType)
            return false;
        
        _subscribers.Add(callback);
        return true;
    }

    // placeholder function for a faster removal algorithm
    public virtual bool Remove(Delegate callback)
    {
        for (int i = 0; i < _subscribers.Count; i++)
        {
            Delegate subscriber = _subscribers[i];
            if (subscriber == callback)
            {
                _subscribers[i] = _subscribers[_subscribers.Count-1];
                _subscribers[_subscribers.Count-1] = callback;
                _subscribers.RemoveAt(_subscribers.Count-1);
                return true;
            }
        }
        return false;
    }

    // overload invokes
    public virtual void Invoke() {}
    public virtual void Invoke<T1>(T1 arg1) {}
    public virtual void Invoke<T1, T2>(T1 arg1, T2 arg2) {}
    
    // helpers
    public virtual Delegate GetRelayCallback()
    {
        return relayCallback;
    }
}
#region ActionSlot
public class ActionSlot : EventSlot
{
    public override Type delegateType => typeof(Action);
    public ActionSlot(InputEventRelay newInputRelay) : base(newInputRelay)
    {
        relayCallback =(Action)(() => Invoke());
    }
    // Invoke overloads
    public override void Invoke()
    {
        if (!inputRelay.IsActive || !IsActive)
            return;
        for (int i = 0; i < _subscribers.Count; i++) {
            Type d_type = delegateType;
            ((Action)_subscribers[i])();
        }
    }
    public override void Invoke<I1>(I1 arg1) {Invoke();}
    public override void Invoke<I1, I2>(I1 arg1, I2 arg2) {Invoke();}
}
#endregion

#region ActionSlot<>
public class ActionSlot<T> : EventSlot
{
    public override Type delegateType => typeof(Action<T>);
    public ActionSlot(InputEventRelay newInputRelay) : base(newInputRelay)
    {
        relayCallback = (Action<T>)((arg1) => Invoke(arg1));
    }
    // Invoke overloads
    public override void Invoke<I1>(I1 arg1)
    {
        if (!inputRelay.IsActive || !IsActive)
            return;
        for (int i = 0; i < _subscribers.Count; i++) {
            Type d_type = delegateType;
            ((Action<I1>)_subscribers[i])(arg1);
        }
    }
    public override void Invoke<I1, I2>(I1 arg1, I2 arg2) {Invoke(arg1);}
}
#endregion
#region ActionSlot<,>
public class ActionSlot<T1, T2> : EventSlot
{
    public override Type delegateType => typeof(Action<T1, T2>);
    public ActionSlot(InputEventRelay newInputRelay) : base(newInputRelay)
    {
        relayCallback = (Action<T1, T2>)((arg1, arg2) => Invoke(arg1, arg2));
    }
    // Invoke overloads
    public override void Invoke<I1, I2>(I1 arg1, I2 arg2)
    {
        if (!inputRelay.IsActive || !IsActive)
            return;
        for (int i = 0; i < _subscribers.Count; i++) {
            Type d_type = delegateType;
            ((Action<I1, I2>)_subscribers[i])(arg1, arg2);
        }
    }
}
#endregion