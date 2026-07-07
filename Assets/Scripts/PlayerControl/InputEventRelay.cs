using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public enum InputEvent {
    Passive, 
    MoveStart,
    MoveEnd,
    MainStart, 
    MainEnd, 
    AltStart, 
    AltEnd, 
    Reset, 
    // always keep this here to get this thing's size
    Size,
    // when a custom activator is triggered, this will be called.
    // this is not initialized into the event relay by default
    CustomEvent, 
}

/// <summary>
/// Contains an array of all existing input events
/// Allows other scripts to easily & safely access input events, so they can subscribe to them
/// Stores an internal array of referneces to input events, in their base class of delegates
/// </summary>

[Serializable]
public class InputEventRelay
{    
    private Dictionary<Type, EventSlot[]> _customInputEvents = new Dictionary<Type, EventSlot[]>();

    // simple container of type of subscribers
    private sealed class EventSlot
    {
        public readonly Type DelegateType;
        public List<Delegate> Subscribers;
        public EventSlot(Type delegateType)
        {
            DelegateType = delegateType;
            Subscribers = new List<Delegate>();
        }

        // placeholder function for a faster removal algorithm
        public bool Remove(Delegate removeEvent)
        {
            for (int i = 0; i < Subscribers.Count; i++)
            {
                Delegate subscriber = Subscribers[i];
                if (subscriber == removeEvent)
                {
                    Subscribers[i] = Subscribers[Subscribers.Count-1];
                    Subscribers[Subscribers.Count-1] = subscriber;
                    return true;
                }
            }
            return false;
        }
    }

    #region Initializers
    // simple helper function to get a preinitialized array for the input event
    public static Delegate[] NewRelayArray()
    {
        return new Delegate[(int)InputEvent.Size];
    }
    /// basic constructor
    public InputEventRelay((Enum, Type)[] set_events)
    {
        foreach((Enum, Type) set_event in set_events)
        {
            Type enum_type = set_event.Item1.GetType();
            int index = Convert.ToInt32(set_event.Item1);

            if (!_customInputEvents.ContainsKey(enum_type))
            {
                _customInputEvents[enum_type] = new EventSlot[Enum.GetNames(enum_type).Length];
            }
            if (_customInputEvents[enum_type][index] == null)
            {
                Type delg_type = set_event.Item2;            
                if (typeof(Delegate).IsAssignableFrom(delg_type))
                {
                    _customInputEvents[enum_type][index] = new EventSlot(delg_type);
                }
                else
                {
                    Debug.LogError(
                        $"'{enum_type.Name}' is not a delegate type.");
                }
            }
        }
    }
    #endregion

    #region Connect Events
    /// Connects an event to the relay
    /// If the event does not exist, it will create a new event. actually
    /// Will return T/F depending on whether or not connection was successful
    public bool ConnectEvent<E>(E inputType, Delegate callback, Type callback_type) where E : Enum 
    {
        if (callback == null)
        {
            Debug.LogError("Cannot connect a null delegate.");
            return false;
        }
        
        Type enum_type = typeof(E);
        int index = Convert.ToInt32(inputType);
    
        if (!_customInputEvents.ContainsKey(enum_type) || _customInputEvents[enum_type][index] == null)
        {
            Debug.LogError("No relay exists to connect to");
            return false;
        }
        else
        {
            // Prevent mixing delegate types.
            Type expected = _customInputEvents[enum_type][index].DelegateType;
            if (expected != callback_type)
            {
                Debug.LogError(
                    $"Cannot connect '{callback.GetType().Name}' to '{inputType}'. " +
                    $"Expected '{expected.Name}'.");
                return false;
            }
        }

        _customInputEvents[enum_type][index].Subscribers.Add(callback);
        return true;
    }
    public bool ConnectEvent<E>(E inputType, Action callback) where E : Enum {return ConnectEvent(inputType, (Delegate)callback, typeof(Action));}
    public bool ConnectEvent<E, T>(E inputType, Action<T> callback) where E : Enum {return ConnectEvent(inputType, (Delegate)callback, typeof(Action<T>));}
    public bool ConnectEvent<E, T1, T2>(E inputType, Action<T1, T2> callback) where E : Enum {return ConnectEvent(inputType, (Delegate)callback, typeof(Action<T1, T2>));}

    /// Disconnects an event from the relay
    /// Will return T/F depending on whether or not disconnection was successful
    public bool DisconnectEvent<E>(E inputType, Delegate callback) where E : Enum
    {
        if (callback == null)
        {
            Debug.LogError("Cannot disconnect a null delegate.");
            return false;
        }

        Type enum_type = typeof(E);
        int index = Convert.ToInt32(inputType);

        Type expected = _customInputEvents[enum_type][index].DelegateType;
        if (expected != enum_type)
        {
            Debug.LogError(
                $"Cannot disconnect '{callback.GetType().Name}' from '{inputType}'. " +
                $"Expected '{expected}'.");
            return false;
        }

        bool removed = _customInputEvents[enum_type][index].Subscribers.Remove(callback);
        if (!removed)
        {
            Debug.LogWarning($"callback could not be found for removal");
        }

        return true;
    }
    #endregion

    #region Invoke
    public void Invoke<E>(E inputType) where E : Enum
    {
        Type enum_type = typeof(E);
        int index = Convert.ToInt32(inputType);
        EventSlot slot = _customInputEvents[enum_type][index];
        foreach (Delegate slot_sub in slot.Subscribers) {
            Type d_type = slot.DelegateType;
            ((Action)slot_sub)();
        }
    }
    public void Invoke<E, T>(E inputType, T arg) where E : Enum
    {
        Type enum_type = typeof(E);
        int index = Convert.ToInt32(inputType);
        EventSlot slot = _customInputEvents[enum_type][index];
        foreach (Delegate slot_sub in slot.Subscribers) {
            Type d_type = slot.DelegateType;
            ((Action<T>)slot_sub)(arg);
        }
    }
    public void Invoke<E, T1, T2>(E inputType, T1 arg1, T2 arg2) where E : Enum
    {
        Type enum_type = typeof(E);
        int index = Convert.ToInt32(inputType);
        EventSlot slot = _customInputEvents[enum_type][index];
        foreach (Delegate slot_sub in slot.Subscribers) {
            Type d_type = slot.DelegateType;
            ((Action<T1, T2>)slot_sub)(arg1, arg2);
        }
    }

    #endregion

    #region Helpers
    public Type GetDelegateType<E>(E inputType) where E : Enum
    {
        Type enum_type = typeof(E);
        int index = Convert.ToInt32(inputType);
        return _customInputEvents[enum_type][index].DelegateType;
    }

    // determines if any methods are connected to an input
    public bool IsConnected<E>(E inputType) where E : Enum
    {
        Type enum_type = typeof(E);
        int index = Convert.ToInt32(inputType);
        if (!_customInputEvents.ContainsKey(enum_type))
        {
            return false;
        }
        return _customInputEvents[enum_type][index]  != null;
    }
    #endregion
}
