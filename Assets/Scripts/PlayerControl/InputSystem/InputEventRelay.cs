using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public enum InputEvent {
    Character_MoveStart,
    Character_MoveEnd,
    Character_MainStart, 
    Character_MainEnd, 
    Character_AltStart, 
    Character_AltEnd,
    General_Passive, 
    General_StackActivation,
    Usable_Equip,
    Usable_Unequip,
    Usable_Used, 
    Usable_ResetStart, 
    Usable_ResetEnd, 
    Size
}

/// <summary>
/// Contains an array of all existing input events
/// Allows other scripts to easily & safely access input events, so they can subscribe to them
/// Stores an internal array of referneces to input events, in their base class of delegates
/// </summary>

[Serializable]
public class InputEventRelay
{    
    private EventSlot[] _inputEvents = new EventSlot[(int)InputEvent.Size];
    public bool isActive = true;

    #region Initializers
    /// basic constructors
    public InputEventRelay()
    {}

    public InputEventRelay((InputEvent, Type)[] set_events)
    {
        foreach((InputEvent, Type) set_event in set_events)
        {
            AddEvent(set_event.Item1, set_event.Item2);
        }
    }
    public InputEventRelay(List<(InputEvent, Type)> set_events)
    {
        foreach((InputEvent, Type) set_event in set_events)
        {
            AddEvent(set_event.Item1, set_event.Item2);
        }
    }

    // Add events during runtime or initialization. Only meant to be performed by the relay holder.
    // Also used by the constructor
    public bool AddEvent(InputEvent inputType, Type callbackType)
    {
        int index = (int)inputType;

        if (_inputEvents[index] == null)
        {         
            EventSlot new_event = EventSlot.CreateSlot(callbackType, this);
            if (new_event != null)
            {
                Debug.Log($" created relay for {inputType}, {callbackType}");
                _inputEvents[index] = new_event;
            }
            else
            {
                Debug.LogError(
                    $"'{callbackType.Name}' is not a delegate type.");
                return false;
            }
        }
        return true;
    }
    #endregion

    #region Copy Data

    // copies the active events in an inputted relay, and creates new, empty events in this relay
    public void CopyEvents(InputEventRelay copied)
    {
        foreach((InputEvent, Type) set_event in copied.GetEventData())
        {
            AddEvent(set_event.Item1, set_event.Item2);
        }
    }

    #endregion

    #region Linking Relays
    // links this relay to a parent relay, connecting this relay's invoke methods to its parent's calls if the parent has them
    public void LinkRelay(InputEventRelay parent)
    {
        for (int i = 0; i < _inputEvents.Length; i++)
        {
            // Add a new event if the parent has the event and this one doesn't
            if (parent._inputEvents[i] == null)
            {
                Debug.Log("nothing to link");
                continue;
            }
            else if (_inputEvents[i] == null)
            {
                AddEvent((InputEvent)i, parent.GetDelegateType((InputEvent)i));
            }
            
            EventSlot slot = _inputEvents[i];
            Delegate invoke_method = slot.GetRelayCallback();
            bool connected = parent.ConnectEvent((InputEvent)i, invoke_method);
            if (connected)
            {
                Debug.Log($"{slot.delegateType.Name} for {(InputEvent)i} has been connected");
            }
            else
            {
                Debug.LogWarning($"{slot.delegateType.Name} for {(InputEvent)i} could not connect");
            }
        }
    }
    public void UnlinkRelay(InputEventRelay parent)
    {
        for (int i = 0; i < _inputEvents.Length; i++)
        {
            // Add a new event if the parent has the event and this one doesn't
            if (parent._inputEvents[i] == null || _inputEvents[i] == null)
            {
                Debug.Log("nothing to unlink");
                continue;
            }
            
            EventSlot slot = _inputEvents[i];
            Delegate invoke_method = slot.GetRelayCallback();
            bool connected = parent.DisconnectEvent((InputEvent)i, invoke_method);
            if (connected)
            {
                Debug.Log($"{slot.delegateType.Name} for {(InputEvent)i} has been disconnected");
            }
            else
            {
                Debug.LogWarning($"{slot.delegateType.Name} for {(InputEvent)i} could not disconnect");
            }
        }
    }

    #endregion

    #region Connect Events
    /// Connects an event to the relay
    /// If the event does not exist, it will create a new event. actually
    /// Will return T/F depending on whether or not connection was successful
    public bool ConnectEvent(InputEvent inputType, Delegate callback, Type callbackType)
    {
        if (callback == null)
        {
            Debug.LogError("Cannot connect a null delegate.");
            return false;
        }
        
        int index = (int)inputType;
        if ( _inputEvents[index] == null)
        {
            Debug.LogError($"No relay at {inputType} to connect to");
            return false;
        }

        // callback type is nul when a delegate is inputted (from linking). use reflection
        if (callbackType == null)
        {
            callbackType = callback.GetType();
        }
        // Prevent mixing delegate types.
        if (!_inputEvents[index].Add(callback, callbackType))
        {
            Debug.LogError(
                $"Cannot connect '{callback.GetType().Name}' to '{inputType}'. " +
                $"Expected '{_inputEvents[index].delegateType.Name}'.");
            return false;
        }

        ;
        return true;
    }
    public bool ConnectEvent(InputEvent inputType, Delegate callback) {return ConnectEvent(inputType, callback, null);}
    public bool ConnectEvent(InputEvent inputType, Action callback) {return ConnectEvent(inputType, callback, typeof(Action));}
    public bool ConnectEvent<T>(InputEvent inputType, Action<T> callback) {return ConnectEvent(inputType, callback, typeof(Action<T>));}
    public bool ConnectEvent<T1, T2>(InputEvent inputType, Action<T1, T2> callback) {return ConnectEvent(inputType, callback, typeof(Action<T1, T2>));}

    /// Disconnects an event from the relay
    /// Will return T/F depending on whether or not disconnection was successful
    public bool DisconnectEvent(InputEvent inputType, Delegate callback)
    {
        if (callback == null)
        {
            Debug.LogError("Cannot disconnect a null delegate.");
            return false;
        }

        int index = (int)inputType;

    
        Type expected = _inputEvents[index].delegateType;
        if (expected != callback.GetType())
        {
            Debug.LogError(
                $"Cannot disconnect '{callback.GetType().Name}' from '{inputType}'. " +
                $"Expected '{expected}'.");
            return false;
        }

        bool removed = _inputEvents[index].Remove(callback);
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
        slot.Invoke();
    }
    public void Invoke<T>(InputEvent inputType, T arg)
    {
        EventSlot slot = _inputEvents[(int)inputType];
        slot.Invoke(arg);
    }
    public void Invoke<T1, T2>(InputEvent inputType, T1 arg1, T2 arg2)
    {
        EventSlot slot = _inputEvents[(int)inputType];
        slot.Invoke(arg1, arg2);
    }

    #endregion

    #region Helpers
    public Type GetDelegateType(InputEvent inputType)
    {
        return _inputEvents[(int)inputType].delegateType;
    }

    // determines if any methods are connected to an input
    public bool IsConnected(InputEvent inputType)
    {
        return _inputEvents[(int)inputType] != null;
    }

    // creates a list of all active events in this input relay and their event type
    public (InputEvent, Type)[] GetEventData()
    {
        (InputEvent, Type)[] set_events_array = new (InputEvent, Type)[(int)InputEvent.Size];
        for(int i = 0; i < set_events_array.Length; i++)
        {
            InputEvent enum_type = (InputEvent)i;
            EventSlot event_slot = _inputEvents[i];
            if (event_slot != null)
            {
                set_events_array[i] = (enum_type, event_slot.delegateType);
            }
        }
        return set_events_array;
    }

    public void DebugEvents()
    {
        string join_string = "";
        for(int i = 0; i < _inputEvents.Length; i++)
        {
            join_string += $"{(InputEvent)i} has event? {_inputEvents[i] != null}, \n";
        }
        Debug.Log(join_string);
    }
    #endregion
}
