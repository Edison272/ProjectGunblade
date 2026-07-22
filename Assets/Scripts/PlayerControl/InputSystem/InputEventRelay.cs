using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Contains an array of all existing input events
/// Allows other scripts to easily & safely access input events, so they can subscribe to them
/// Stores an internal array of referneces to input events, in their base class of delegates
/// </summary>

[Serializable]
public class InputEventRelay
{    
    private EventSlot[][] _inputEvents = new EventSlot[Enum.GetValues(typeof(InputEventType)).Length-1][];
    public bool IsActive = true; // control whether the ENTIRE relay is active/inactive

    #region Initializers
    /// basic constructors
    public InputEventRelay()
    {}

    public InputEventRelay((Enum, Type)[] set_events)
    {
        foreach((Enum, Type) set_event in set_events)
        {
            AddEvent(set_event.Item1, set_event.Item2);
        }
    }
    public InputEventRelay(List<(Enum, Type)> set_events)
    {
        foreach((Enum, Type) set_event in set_events)
        {
            AddEvent(set_event.Item1, set_event.Item2);
        }
    }

    // Add events during runtime or initialization. Only meant to be performed by the relay holder.
    // Also used by the constructor
    public bool AddEvent<E>(E inputType, Type callbackType) where E : Enum
    {
        // secure spot in outer array
        InputEventType eventType = InputEventSelector.GetInputEventTypeFromEnum(inputType);
        if (eventType == InputEventType.None)
        {
            Debug.LogError($"{inputType} is not a valid Input Event Type!");
            return false;
        }
        int eventIndex = Convert.ToInt32(eventType);
        if (_inputEvents[eventIndex] == null)
        {
            _inputEvents[eventIndex] = new EventSlot[InputEventSelector.GetEnumLength(inputType)];
        }
        
        // secure spot in nested array
        int enumIndex =  Convert.ToInt32(inputType);

        if (_inputEvents[eventIndex][enumIndex] == null)
        {         
            EventSlot new_event = EventSlot.CreateSlot(callbackType, this);
            if (new_event != null)
            {
                Debug.Log($" created relay for {inputType}, {callbackType}");
                _inputEvents[eventIndex][enumIndex] = new_event;
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
        foreach((Enum, Type) set_event in copied.GetEventData())
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
            if (parent._inputEvents[i] == null)
            {
                Debug.Log($" {(InputEventType)i} does not exist in parent");
                continue;
            }
                    
            for (int j = 0; j < parent._inputEvents[i].Length; j++)
            {
                // Add a new event if the parent has the event and this one doesn't
                Enum newEnum = InputEventSelector.GetEnumFromTypeInt((InputEventType)i,j);
                if (parent._inputEvents[i][j] == null)
                {
                    Debug.Log($" {newEnum} does not exist in parent");
                    continue;
                }
                else if (_inputEvents[i] == null || _inputEvents[i][j] == null)
                {
                    AddEvent(newEnum, parent.GetDelegateType(newEnum));
                }
                
                EventSlot slot = _inputEvents[i][j];
                Delegate invoke_method = slot.GetRelayCallback();
                bool connected = parent.ConnectEvent(newEnum, invoke_method);
                if (connected)
                {
                    Debug.Log($"{slot.delegateType.Name} for {newEnum} has been linked");
                }
                else
                {
                    Debug.LogWarning($"{slot.delegateType.Name} for {newEnum} could not link");
                }
            }
        }
    }
    public void UnlinkRelay(InputEventRelay parent)
    {
        for (int i = 0; i < _inputEvents.Length; i++)
        {
            if (parent._inputEvents[i] == null)
            {
                Debug.Log($" {(InputEventType)i} does not exist in parent");
                continue;
            }
            
            EventSlot[] slotArr = _inputEvents[i];
            if (slotArr == null)
            {
                continue;
            }
            
            for (int j = 0; j < slotArr.Length; i++)
            {
                if (parent._inputEvents[i][j] == null || slotArr[j] == null)
                {
                    Debug.Log("nothing to unlink");
                    continue;
                }
                Enum newEnum = InputEventSelector.GetEnumFromTypeInt((InputEventType)i,j);
                EventSlot slot = slotArr[j];
                Delegate invoke_method = slot.GetRelayCallback();
                bool connected = parent.DisconnectEvent(newEnum, invoke_method);
                if (connected)
                {
                    Debug.Log($"{slot.delegateType.Name} for {newEnum} has been disconnected");
                }
                else
                {
                    Debug.LogWarning($"{slot.delegateType.Name} for {newEnum} could not disconnect");
                }
            }
        }
    }

    #endregion

    #region Connect Events
    /// Connects an event to the relay
    /// If the event does not exist, it will create a new event. actually
    /// Will return T/F depending on whether or not connection was successful
    public bool ConnectEvent(Enum inputType, Delegate callback, Type callbackType)
    {
        if (callback == null)
        {
            Debug.LogError("Cannot connect a null delegate.");
            return false;
        }

        // find index in 2d array
        InputEventType eventType = InputEventSelector.GetInputEventTypeFromEnum(inputType);
        int eventIndex = Convert.ToInt32(eventType);
        if (_inputEvents[eventIndex] == null)
        {
           Debug.LogError($"No relay at {eventType} to connect to");
           return false;
        }
        int enumIndex =  Convert.ToInt32(inputType);
        if ( _inputEvents[eventIndex][enumIndex] == null)
        {
            Debug.LogError($"No relay at {eventType}, at {inputType} to connect to");
            return false;
        }

        // callback type is nul when a delegate is inputted (from linking). use reflection
        if (callbackType == null)
        {
            callbackType = callback.GetType();
        }
        // Prevent mixing delegate types.
        if (!_inputEvents[eventIndex][enumIndex].Add(callback, callbackType))
        {
            Debug.LogError(
                $"Cannot connect '{callback.GetType().Name}' to '{inputType}'. " +
                $"Expected '{_inputEvents[eventIndex][enumIndex].delegateType.Name}'.");
            return false;
        }

        ;
        return true;
    }
    public bool ConnectEvent<E>(E inputType, Delegate callback) where E : Enum {return ConnectEvent(inputType, callback, null);}
    public bool ConnectEvent<E>(E inputType, Action callback) where E : Enum {return ConnectEvent(inputType, callback, typeof(Action));}
    public bool ConnectEvent<E, T>(E inputType, Action<T> callback) where E : Enum {return ConnectEvent(inputType, callback, typeof(Action<T>));}
    public bool ConnectEvent<E,T1, T2>(E inputType, Action<T1, T2> callback) where E : Enum {return ConnectEvent(inputType, callback, typeof(Action<T1, T2>));}

    /// Disconnects an event from the relay
    /// Will return T/F depending on whether or not disconnection was successful
    public bool DisconnectEvent(Enum inputType, Delegate callback)
    {
        if (callback == null)
        {
            Debug.LogError("Cannot disconnect a null delegate.");
            return false;
        }

        // find index in 2d array
        InputEventType eventType = InputEventSelector.GetInputEventTypeFromEnum(inputType);
        int eventIndex = Convert.ToInt32(eventType);
        if (_inputEvents[eventIndex] == null)
        {
            Debug.LogError($"No relay at {inputType} to disconnect from");
            return false;
        }

        int enumIndex =  Convert.ToInt32(inputType);
        if ( _inputEvents[eventIndex][enumIndex] == null)
        {
            Debug.LogError($"No relay at {eventType}, at {inputType} to disconnect from");
            return false;
        }

    
        Type expected = _inputEvents[eventIndex][enumIndex].delegateType;
        if (expected != callback.GetType())
        {
            Debug.LogError(
                $"Cannot disconnect '{callback.GetType().Name}' from '{inputType}'. " +
                $"Expected '{expected}'.");
            return false;
        }

        bool removed = _inputEvents[eventIndex][enumIndex].Remove(callback);
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
        if (!IsActive || !IsConnected(inputType)) {return;}
        GetSlot(inputType)?.Invoke();
    }
    public void Invoke<E, T>(E inputType, T arg) where E : Enum
    {
        if (!IsActive || !IsConnected(inputType)) {return;}
        GetSlot(inputType)?.Invoke(arg);
    }
    public void Invoke<E, T1, T2>(E inputType, T1 arg1, T2 arg2) where E : Enum
    {
        if (!IsActive || !IsConnected(inputType)) {return;}
        GetSlot(inputType)?.Invoke(arg1, arg2);
    }

    #endregion

    #region Event Slot Control

    // control whether or not an individual event is active/inactive
    public void SetEventActive(Enum inputType, bool is_active)
    {
        EventSlot slot = GetSlot(inputType);
        slot.IsActive = is_active;
    }

    #endregion

    #region Helpers
    
    public EventSlot GetSlot(Enum inputType)
    {
        InputEventType eventType = InputEventSelector.GetInputEventTypeFromEnum(inputType);
        int eventIndex = Convert.ToInt32(eventType);
        if (_inputEvents[eventIndex] == null)
            return null;

        int enumIndex =  Convert.ToInt32(inputType);
        EventSlot slot = _inputEvents[eventIndex][enumIndex];
        if (slot == null)
            return null;

        return slot;
    }
    public Type GetDelegateType(Enum inputType)
    {
        EventSlot slot = GetSlot(inputType);
        return slot != null ? slot.delegateType : null;
    }

    // determines if any methods are connected to an input
    public bool IsConnected(Enum inputType)
    {
        EventSlot slot = GetSlot(inputType);
        return slot != null;
    }

    // creates a list of all active events in this input relay and their event type
    public (Enum, Type)[] GetEventData()
    {
        int total_size = 0;
        for(int i = 0; i < _inputEvents.Length; i++)
        {
            total_size += InputEventSelector.GetEnumLength(InputEventSelector.GetEnumFromTypeInt((InputEventType)i, i));
        }
        (Enum, Type)[] set_events_array = new (Enum, Type)[total_size];
        int top = 0;
        for(int i = 0; i < _inputEvents.Length; i++)
        {
            if (_inputEvents[i] == null)
            {
                top += InputEventSelector.GetEnumLength(InputEventSelector.GetEnumFromTypeInt((InputEventType)i, i));
                continue;
            }
            for (int j = 0; j < InputEventSelector.GetEnumLength(InputEventSelector.GetEnumFromTypeInt((InputEventType)i, i)); j++)
            {
                Enum enum_type = InputEventSelector.GetEnumFromTypeInt((InputEventType)i, j);
                EventSlot event_slot = _inputEvents[i][j];
                if (event_slot != null)
                {
                    set_events_array[top+j] = (enum_type, event_slot.delegateType);
                }
                top++;
            }
        }
        return set_events_array;
    }
    #endregion
}
