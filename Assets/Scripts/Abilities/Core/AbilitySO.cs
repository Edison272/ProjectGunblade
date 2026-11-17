using UnityEngine;
using System;
using System.Collections.Generic;

public enum AbilityActiveType {STATS, ATTACK, EQUIP}
public enum AbilityDurationType {Time, ItemUses, DamageTaken, Endless}
public enum AbilityRecoveryType {Time, Kills, DamageTaken}
[CreateAssetMenu(fileName = "Item", menuName = "ScriptableObjects/Abilities", order = 1)]
public class AbilitySO : ScriptableObject
{
    // [Header("Ability Active Type")] // What the ability does
    
    // [Header("Ability Duration Type")] // When the ability ends
    

    // [Header("Ability Recovery Type")] // How the ability recovers



    #region Creating the Item
    // public Item GenerateItem(Vector3 pos) // summon an item on the ground
    // {
    //     return GenerateItem(pos, Quaternion.identity);
    // }
    // public Item GenerateItem(Vector3 pos, Quaternion rotation) // summon an item on the ground
    // {
    //     GameObject item_object = MonoBehaviour.Instantiate(item_prefab, pos, rotation);
    //     Item new_item = item_object.GetComponent<Item>();
    //     new_item.Setup(this);
    //     return new_item;
    // }
    // public Item GenerateItem(Transform holder) // summon an item on a holder
    // {
    //     GameObject item_object = MonoBehaviour.Instantiate(item_prefab, holder);
    //     Item new_item = item_object.GetComponent<Item>();
    //     new_item.Setup(this);
    //     return new_item;
    // }

    // public Item SetupNewItem(Item newItem)
    // {
    //     return
    // }
    #endregion

    public void OnValidate()
    {
        // for(int i = 0; i < ItemOutputs.Length; i++)
        // {
        //     ItemOutput itemOutput = ItemOutputs[i];
        //     if (itemOutput == null)
        //         itemOutput = new GunOutput();
        //     if (itemOutput.itemOutputType != itemOutput.GetExpectedItemOutputType())
        //         itemOutput = itemOutput.SmartRecast();
        //     ItemOutputs[i] = itemOutput;
        // }
        // AttackObjectController.UpdateSerialization();
        // for(int i = 0; i < StackCounters.Length; i++)
        // {
        //     StackCounter stackCounter = StackCounters[i];
        //     if (stackCounter == null)
        //         stackCounter = new SimpleCounter();
        //     if (stackCounter.stackCounterType != stackCounter.GetExpectedStackCountType())
        //         stackCounter = stackCounter.SmartRecast();
        //     if (stackCounter.UISetting != null)
        //         stackCounter.UISetting.LoadUI(true);
        //     StackCounters[i] = stackCounter;
        // }
    }

    #region GUI support

    #endregion
}