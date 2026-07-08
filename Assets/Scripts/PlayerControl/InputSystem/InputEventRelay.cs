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
    Usable_Reset, 
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
    private Dictionary<Type, EventSlot[]> _customInputEvents = new Dictionary<Type, EventSlot[]>();
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
        Type enum_type = inputType.GetType();
        int index = Convert.ToInt32(inputType);

        if (!_customInputEvents.ContainsKey(enum_type))
        {
            _customInputEvents[enum_type] = new EventSlot[Enum.GetNames(enum_type).Length];
        }
        if (_customInputEvents[enum_type][index] == null)
        {         
            Debug.Log($"{inputType}, {callbackType}");
            EventSlot new_event = EventSlot.CreateSlot(callbackType);
            if (new_event != null)
            {
                _customInputEvents[enum_type][index] = new_event;
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
        Debug.Log(string.Join(", ",  _customInputEvents.Keys));
        Debug.Log("parent: "+string.Join(", ",  parent._customInputEvents.Keys));
        foreach (Type inputType in _customInputEvents.Keys)
        {
            if (_customInputEvents[inputType] == null)
            {
                Debug.LogWarning($"no event slot array here at {inputType}");
                continue;
            }
            for (int i = 0; i < _customInputEvents[inputType].Length; i++)
            {
                EventSlot slot = _customInputEvents[inputType][i];
                if (slot == null)
                {
                    continue;
                }
                
                Enum enum_type = (Enum)Enum.ToObject(inputType, i);
                Type delegateType = slot.delegateType;
                Delegate invoke_method = _customInputEvents[inputType][i].GetLambda();
                Debug.Log($"Link {enum_type}, parent has key? {parent._customInputEvents.ContainsKey(inputType)}");
                bool connected = parent.ConnectEvent(enum_type, invoke_method);
                if (connected)
                {
                    Debug.Log($"{slot.delegateType.Name} for {enum_type} has been connected");
                }
                else
                {
                    Debug.Log($"{slot.delegateType.Name} for {enum_type} could not connect");
                }
                
            }
        }
    }
    public void UnlinkRelay(InputEventRelay parent)
    {
        
    }

    #endregion

    #region Connect Events
    /// Connects an event to the relay
    /// If the event does not exist, it will create a new event. actually
    /// Will return T/F depending on whether or not connection was successful
    public bool ConnectEvent<E>(E inputType, Delegate callback, Type callbackType) where E : Enum 
    {
        if (callback == null)
        {
            Debug.LogError("Cannot connect a null delegate.");
            return false;
        }
        
        Type enum_type = inputType.GetType();
        Debug.Log($"checking {typeof(E)}, {inputType.GetType().Name}");
        int index = Convert.ToInt32(inputType);
    
        if (!_customInputEvents.ContainsKey(enum_type))
        {
            Debug.LogError($"No relay array at {enum_type} to connect to");
            return false;
        }
        if ( _customInputEvents[enum_type][index] == null)
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
        if (!_customInputEvents[enum_type][index].Add(callback, callbackType))
        {
            Debug.LogError(
                $"Cannot connect '{callback.GetType().Name}' to '{inputType}'. " +
                $"Expected '{_customInputEvents[enum_type][index].delegateType.Name}'.");
            return false;
        }

        ;
        return true;
    }
    public bool ConnectEvent<E>(E inputType, Delegate callback) where E : Enum {return ConnectEvent(inputType, callback, null);}
    public bool ConnectEvent<E>(E inputType, Action callback) where E : Enum {return ConnectEvent(inputType, callback, typeof(Action));}
    public bool ConnectEvent<E, T>(E inputType, Action<T> callback) where E : Enum {return ConnectEvent(inputType, callback, typeof(Action<T>));}
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

        Type expected = _customInputEvents[enum_type][index].delegateType;
        if (expected != enum_type)
        {
            Debug.LogError(
                $"Cannot disconnect '{callback.GetType().Name}' from '{inputType}'. " +
                $"Expected '{expected}'.");
            return false;
        }

        bool removed = _customInputEvents[enum_type][index].Remove(callback);
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
        EventSlot slot = _customInputEvents[typeof(E)][Convert.ToInt32(inputType)];
        slot.Invoke();
    }
    public void Invoke<E, T>(E inputType, T arg) where E : Enum
    {
        EventSlot slot = _customInputEvents[typeof(E)][Convert.ToInt32(inputType)];
        slot.Invoke(arg);
    }
    public void Invoke<E, T1, T2>(E inputType, T1 arg1, T2 arg2) where E : Enum
    {
        EventSlot slot = _customInputEvents[typeof(E)][Convert.ToInt32(inputType)];
        slot.Invoke(arg1, arg2);
    }

    #endregion

    #region Helpers
    public Type GetDelegateType<E>(E inputType) where E : Enum
    {
        Type enum_type = typeof(E);
        int index = Convert.ToInt32(inputType);
        return _customInputEvents[enum_type][index].delegateType;
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
        return _customInputEvents[enum_type][index] != null;
    }

    // creates a list of all active events in this input relay and their event type
    public List<(Enum, Type)> GetEventData()
    {
        List<(Enum, Type)> set_events_list = new List<(Enum, Type)>();
        foreach(Type key in _customInputEvents.Keys)
        {
            for(int i = 0; i < _customInputEvents[key].Length; i++)
            {
                Enum enum_type = (Enum)Enum.ToObject(key, i);
                EventSlot event_slot = _customInputEvents[key][i];
                if (event_slot != null)
                {
                    set_events_list.Add((enum_type, event_slot.delegateType));
                }
            }
        }
        return set_events_list;
    }
    #endregion
}
