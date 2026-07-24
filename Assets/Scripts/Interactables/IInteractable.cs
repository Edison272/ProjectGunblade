using System.Runtime.InteropServices;
using UnityEngine;

public interface IInteractable
{       
    public static readonly LayerMask find_interactable_mask = 1 << 9;
    public void Interact(Character character);
    public void ToggleInteractPrompt(bool enable);
    public string GetPromptText();
}