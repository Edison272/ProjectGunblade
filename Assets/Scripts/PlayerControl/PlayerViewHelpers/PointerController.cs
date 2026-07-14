using System;
using UnityEngine;

[Serializable]
public class PointerController
{
    [Header("Look Data")]
    private Vector2 _pixelPos;  // the pointer's position in a pixels, based on the updates from the player controller.
    private Vector2 _worldPos; // the in-game world position of the player's pointer. Transated from pixelPos
    private Vector2 _viewportPos; //the viewport/screen position of the player's pointer. Translated from pixelPos
    private Vector2 _sourcePos;
    private Vector2 _sourceToWorldVec; // directional vector from the sourcePos to the _worldPos

    // used to save/hold a look position
    public Vector2 hold__pixelPos {get; private set;}
    public Vector2 hold_viewport_pos {get; private set;}

    [Header("Camera & Rect")]
    private Camera _cam;
    private Rect _camRect;

    public PointerController(Camera camera)
    {
        _cam = camera;
        _camRect = _cam.pixelRect;
        ResetView(_cam.transform.position);
    }
    public void ResetView(Vector2 ResetLookPos)
    {
        _pixelPos = new Vector2(_camRect.width/2, _camRect.height/2);
        _worldPos = _cam.ScreenToWorldPoint(_pixelPos);
        _viewportPos = _cam.ViewportToWorldPoint(_pixelPos);
    }
    // update's the pointer's pixel position based on the pointer delta. returns the in-gam
    public void UpdateDelta(Vector2 pointerDelta, Vector2 sourcePos)
    {
        // source pos is a character position
        this._sourcePos = sourcePos;
        _pixelPos += pointerDelta;
        _pixelPos = new Vector2(
            Mathf.Clamp(_pixelPos.x, _camRect.xMin, _camRect.xMax),
            Mathf.Clamp(_pixelPos.y, _camRect.yMin, _camRect.yMax)
        );
        _worldPos = _cam.ScreenToWorldPoint(_pixelPos);
        _viewportPos = _cam.ViewportToWorldPoint(_pixelPos);
        _sourceToWorldVec = _worldPos - _sourcePos;
    }
    public Vector2 GetSourceTo_worldPos(Vector2 sourcePos)
    {
        return sourcePos + _sourceToWorldVec;
    }
}