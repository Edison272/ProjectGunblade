using UnityEngine;
using UnityEngine.UI;


/*
Display the stats of a stack counter using bars
*/
public abstract class StackCounterUI : MonoBehaviour
{
    protected StackCounter _stackCounter;
    public abstract StackCounter StackCounter {get; set;}
}
