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
    Reticle_Center,
    ItemUI_Bars,
}

public enum UICounterDesign
{
    Smooth, // a smoothly interpolating bar
    Segments, // discrete counter values
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
    public UICounterDesign CounterDesign = UICounterDesign.Smooth;
    public UIPlacement PlacementDirection = UIPlacement.Reticle_Right; // where will this be placed on the UI (i just used a built in enum)
    public TextDisplay DisplayTest = TextDisplay.None;
    private GameObject _resourceObject = null;

    public GameObject LoadUI(bool ForceLoadResource = false)
    {
        // force the object to load from resources. used at the beginning of the game
        if (ForceLoadResource)
            _resourceObject = null;
        // if the object is already loaded, find it
        else if (_resourceObject)
        {
            return _resourceObject;
        }

        string placement = "Horizontal";
        switch (PlacementDirection)
        {
            case UIPlacement.Reticle_Left:
                placement = "Vertical";
                break;
            case UIPlacement.Reticle_Right: 
                placement = "Vertical";
                break;
            case UIPlacement.Reticle_Up:
                placement = "Horizontal";
                break;
            case UIPlacement.Reticle_Down:
                placement = "Horizontal";
                break;
        }
        
        string design = "";
        switch (CounterDesign)
        {
            case UICounterDesign.Smooth:
                design = "SmoothBar";
                break;
            case UICounterDesign.Segments: 
                design = "SegmentBar";
                break;
        }
        

        // save the loaded object to prevent calling again
        if (_resourceObject == null)
        {
            _resourceObject = Resources.Load<GameObject>("StackBars/" + placement + design);
        }
        return _resourceObject;
    }
}