using System.Runtime.InteropServices;
using UnityEngine;

public interface IInteractable
{       
    const int InteractableMask = (1 << 3);
    public void Interact(Character character); // characters can call the interact function when near an interactable
    public void ToggleInteractPrompt(bool enable);
    public string GetPromptText();
}