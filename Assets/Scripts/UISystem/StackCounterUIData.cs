using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public enum UIPlacement
{
    Reticle_Left,
    Reticle_Right,
    Reticle_Up,
    Reticle_Down,
    Reticle_Center
}

public enum TextDisplay
{
    None,
    Percentage, // 0-100%
    Count, // one number
    Capacity, // number + cap
}

[Serializable]
public class StackCounterUISetting
{
    public Image.FillMethod ImageFillType = Image.FillMethod.Horizontal;
    public UIPlacement PlacementDirection = UIPlacement.Reticle_Right; // where will this be placed on the UI (i just used a built in enum)
    public TextDisplay DisplayTest = TextDisplay.None;
    

    private GameObject _resourceObject = null;

    public GameObject LoadUI()
    {
        // if the object is already loaded, find it
        if (_resourceObject)
        {
            return _resourceObject;
        }
        
        
        string filename = "HorizontalStatBar";
        switch (PlacementDirection)
        {
            case UIPlacement.Reticle_Left:
                filename = "VerticalStatBar";
                break;
            case UIPlacement.Reticle_Right: 
                filename = "VerticalStatBar";
                break;
            case UIPlacement.Reticle_Up:
                filename = "HorizontalStatBar";
                break;
            case UIPlacement.Reticle_Down:
                filename = "HorizontalStatBar";
                break;
        }
        

        // save the loaded object to prevent calling again
        if (_resourceObject == null)
        {
            _resourceObject = Resources.Load<GameObject>("StackBars/" + filename);
        }
        return _resourceObject;
    }
}