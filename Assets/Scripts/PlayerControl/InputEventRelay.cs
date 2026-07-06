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
    private EventSlot[] _inputEvents {get; set;} = new EventSlot[(int)InputEvent.Size];
    private Dictionary<Type, EventSlot[]> _customInputEvents = new Dictionary<Type, EventSlot[]>();
    private Type[] expected_types = new Type[(int)InputEvent.Size];

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
    /// Delegate should have an array length same as InputEvent.Size
    /// No input present = no reference (empty)
    public InputEventRelay(Type[] set_events)
    {
        for (int i = 0; i < _inputEvents.Length; i++)
        {
            Type set_event = set_events[i];
            if (typeof(Delegate).IsAssignableFrom(set_events[i]))
            {
                expected_types[i] = set_event;
                _inputEvents[i] = new EventSlot(set_event);
            }
            else
            {
                Debug.LogError(
                    $"'{expected_types[i].Name}' is not a delegate type.");
            }
        }
    }
    // constructor that converts dictionary into an array
    public InputEventRelay(Dictionary<InputEvent, Type> set_events)
    {
        foreach(InputEvent input_type in set_events.Keys)
        {
            Type set_event = set_events[input_type];
            if (typeof(Delegate).IsAssignableFrom(set_event))
            {
                expected_types[(int)input_type] = set_event;
                _inputEvents[(int)input_type] = new EventSlot(set_event);
            }
            else
            {
                Debug.LogError(
                    $"'{set_event.Name}' is not a delegate type.");
            }
        }
    }

    // directly set one type
    public void SetInputEvent(InputEvent input_type, Type callback_type)
    {
        expected_types[(int)input_type] = callback_type;
    }
    #endregion

    #region Connect Events
    /// Connects an event to the relay
    /// Will return T/F depending on whether or not connection was successful
    public bool ConnectEvent(InputEvent inputType, Delegate callback, Type callback_type)
    {
        if (callback == null)
        {
            Debug.LogError("Cannot connect a null delegate.");
            return false;
        }

        Type expected = expected_types[(int)inputType];

        // Prevent mixing delegate types.
        if (expected != callback_type)
        {
            Debug.LogError(
                $"Cannot connect '{callback.GetType().Name}' to '{inputType}'. " +
                $"Expected '{expected.Name}'.");
            return false;
        }

        _inputEvents[(int)inputType].Subscribers.Add(callback);
        return true;
    }
    public bool ConnectEvent(InputEvent inputType, Action callback){return ConnectEvent(inputType, (Delegate)callback, typeof(Action));}
    public bool ConnectEvent<T>(InputEvent inputType, Action<T> callback){return ConnectEvent(inputType, (Delegate)callback, typeof(Action<T>));}
    public bool ConnectEvent<T1, T2>(InputEvent inputType, Action<T1, T2> callback){return ConnectEvent(inputType, (Delegate)callback, typeof(Action<T1, T2>));}

    /// Disconnects an event from the relay
    /// Will return T/F depending on whether or not disconnection was successful
    public bool DisconnectEvent<TDelegate>(InputEvent inputType, Delegate callback)
    {
        if (callback == null)
        {
            Debug.LogError("Cannot disconnect a null delegate.");
            return false;
        }

        Type expected = expected_types[(int)inputType];
        if (expected != typeof(TDelegate))
        {
            Debug.LogError(
                $"Cannot disconnect '{callback.GetType().Name}' from '{inputType}'. " +
                $"Expected '{expected}'.");
            return false;
        }

        bool removed = _inputEvents[(int)inputType].Subscribers.Remove(callback);
        if (!removed)
        {
            Debug.LogWarning($"callback could not be found for removal");
        }

        return true;
    }
    #endregion

    #region Invoke
    public void Invoke(InputEvent inputType)
    {
        EventSlot slot = _inputEvents[(int)inputType];
        foreach (Delegate slot_sub in slot.Subscribers) {
            Type d_type = slot.DelegateType;
            ((Action)slot_sub)();
        }
    }
    public void Invoke<T>(InputEvent inputType, T arg)
    {
        EventSlot slot = _inputEvents[(int)inputType];
        foreach (Delegate slot_sub in slot.Subscribers) {
            Type d_type = slot.DelegateType;
            ((Action<T>)slot_sub)(arg);
        }
    }
    public void Invoke<T1, T2>(InputEvent inputType, T1 arg1, T2 arg2)
    {
        EventSlot slot = _inputEvents[(int)inputType];
        foreach (Delegate slot_sub in slot.Subscribers) {
            Type d_type = slot.DelegateType;
            ((Action<T1, T2>)slot_sub)(arg1, arg2);
        }
    }

    #endregion

    #region Helpers
    public Type GetDelegateType(InputEvent inputType)
    {
        return expected_types[(int)inputType];
    }

    // determines if any methods are connected to an input
    public bool IsConnected(InputEvent inputType)
    {
        return _inputEvents[(int)inputType] != null;
    }
    #endregion
}
