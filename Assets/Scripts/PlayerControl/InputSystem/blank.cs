using UnityEngine;
using System;
using System.Collections;
using UnityEngine.InputSystem.LowLevel;



public static class InputEventUtils
{


    // public EventType()


    // use this function to get the event type in compile time
    public static Type GetEventType<E>() where E : Enum
    {
        return typeof(InputEvent);
    }
}

