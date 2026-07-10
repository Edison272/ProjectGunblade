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
    public Delegate relayCallback; // other delegates can connect to this
    protected readonly List<Delegate> _subscribers = new List<Delegate>();

    // creat the generic type using the delegate typeId li
    public static EventSlot CreateSlot(Type delegateType)
    {
        EventSlot new_event = null;
        
        Type[] args = delegateType.GetGenericArguments();
        Debug.Log($"{delegateType}, {typeof(Action<>)}, {typeof(Action<Vector2>)}");
        if (delegateType == typeof(Action))
        {
            new_event = new ActionSlot();
        }
        else if (delegateType.GetGenericTypeDefinition() == typeof(Action<>))
        {
            Type slotType = typeof(ActionSlot<>).MakeGenericType(args);
            new_event = (EventSlot)Activator.CreateInstance(slotType);
        }
        else if (delegateType.GetGenericTypeDefinition() == typeof(Action<,>))
        {
            Type slotType = typeof(ActionSlot<>).MakeGenericType(args);
            new_event = (EventSlot)Activator.CreateInstance(slotType);
        }
        return new_event;
    }
    public abstract bool Add(Delegate callback, Type callbackType = null);
    public abstract bool Remove(Delegate callback);

    // overload invokes
    public abstract void Invoke();
    public abstract void Invoke<T1>(T1 arg1);
    public abstract void Invoke<T1, T2>(T1 arg1, T2 arg2);
    
    //
    public abstract Delegate GetRelayCallback();
}
#region ActionSlot
public class ActionSlot : EventSlot
{
    public override Type delegateType => typeof(Action);
    public ActionSlot()
    {
        relayCallback = (Action)(() => Invoke());
    }
    public override bool Add(Delegate callback, Type callbackType = null)
    {
        if (callbackType == null)
            callbackType = callback.GetType();
        if (callbackType != typeof(Action))
            return false;
        
        _subscribers.Add((Action)callback);
        return true;
    }

    // placeholder function for a faster removal algorithm
    public override bool Remove(Delegate callback)
    {
        for (int i = 0; i < _subscribers.Count; i++)
        {
            Delegate subscriber = _subscribers[i];
            if (subscriber == callback)
            {
                _subscribers[i] = _subscribers[_subscribers.Count-1];
                _subscribers[_subscribers.Count-1] = (Action)callback;
                _subscribers.RemoveAt(_subscribers.Count-1);
                return true;
            }
        }
        return false;
    }

    // Invoke overloads
    public override void Invoke()
    {
        foreach (Delegate slot_sub in _subscribers) {
            Type d_type = delegateType;
            ((Action)slot_sub)();
        }
    }
    public override void Invoke<I1>(I1 arg1)
    {
        Invoke();
    }
    public override void Invoke<I1, I2>(I1 arg1, I2 arg2)
    {
        Invoke();
    }

    public override Delegate GetRelayCallback()
    {
        return relayCallback;
    }
}
#endregion

#region ActionSlot<>
public class ActionSlot<T> : EventSlot
{
    public override Type delegateType => typeof(Action<T>);
    public ActionSlot()
    {
        relayCallback = (Action<T>)((arg1) => Invoke(arg1));
    }
    public override bool Add(Delegate callback, Type callbackType = null)
    {
        if (callbackType == null)
            callbackType = callback.GetType();
        if (callbackType != typeof(Action<T>))
            return false;
        
        _subscribers.Add((Action<T>)callback);
        return true;
    }

    // placeholder function for a faster removal algorithm
    public override bool Remove(Delegate callback)
    {
        for (int i = 0; i < _subscribers.Count; i++)
        {
            Delegate subscriber = _subscribers[i];
            if (subscriber == callback)
            {
                _subscribers[i] = _subscribers[_subscribers.Count-1];
                _subscribers[_subscribers.Count-1] = (Action<T>)callback;
                _subscribers.RemoveAt(_subscribers.Count-1);
                return true;
            }
        }
        return false;
    }

    // Invoke overloads
    public override void Invoke()
    {
        
    }
    public override void Invoke<I1>(I1 arg1)
    {
        foreach (Delegate slot_sub in _subscribers) {
            Type d_type = delegateType;
            ((Action<I1>)slot_sub)(arg1);
        }
    }
    public override void Invoke<I1, I2>(I1 arg1, I2 arg2)
    {
        Invoke(arg1);
    }

    public override Delegate GetRelayCallback()
    {
        return relayCallback;
    }
}
#endregion
#region ActionSlot<,>
public class ActionSlot<T1, T2> : EventSlot
{
    public override Type delegateType => typeof(Action<T1, T2>);
    public ActionSlot()
    {
        relayCallback = (Action<T1, T2>)((arg1, arg2) => Invoke(arg1, arg2));
    }
    public override bool Add(Delegate callback, Type callbackType = null)
    {
        if (callbackType == null)
            callbackType = callback.GetType();
        if (callbackType != typeof(Action<T1, T2>))
            return false;
        
        _subscribers.Add((Action<T1, T2>)callback);
        return true;
    }

    // placeholder function for a faster removal algorithm
    public override bool Remove(Delegate callback)
    {
        for (int i = 0; i < _subscribers.Count; i++)
        {
            Delegate subscriber = _subscribers[i];
            if (subscriber == callback)
            {
                _subscribers[i] = _subscribers[_subscribers.Count-1];
                _subscribers[_subscribers.Count-1] = (Action<T1, T2>)callback;
                _subscribers.RemoveAt(_subscribers.Count-1);
                return true;
            }
        }
        return false;
    }

    // Invoke overloads
    public override void Invoke()
    {
        
    }
    public override void Invoke<I1>(I1 arg1)
    {

    }
    public override void Invoke<I1, I2>(I1 arg1, I2 arg2)
    {
        foreach (Delegate slot_sub in _subscribers) {
            Type d_type = delegateType;
            ((Action<I1, I2>)slot_sub)(arg1, arg2);
        }
    }

    public override Delegate GetRelayCallback()
    {
        return relayCallback;
    }
}
#endregion