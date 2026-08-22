using UnityEngine;
using System;


/// <summary>
/// class contains data carried by each individual tile
/// </summary>
public class TileProperties
{
    public Vector2Int Position;
    public bool OccupiedByCharacter;    

    // Checks if there is an adjacent wall between this tile and the tareget direction 
    // returns true if there is an adjacent wall in the way of the parameter direction
    public bool HasInterferingAdjacent(Vector2Int dir)
    {
        // Horizontal neighbor
        Vector2Int horiz = new Vector2Int(dir.x, 0);
        if (MapManager.HasWallAt(Position + horiz))
            return true;

        // Vertical neighbor
        Vector2Int vert = new Vector2Int(0, dir.y);
        if (MapManager.HasWallAt(Position + vert))
            return true;
        return false;


        // really cool thing. could use in a lotta places instead of using Atan
        
        // // rely on ordering of Direction2d.FourDirections to get indexes based on dot product of dir
        // float Dot = Vector2.Dot(dir, Vector2.right);
        // float Cross = Vector2.Dot(dir, Vector2.up);
        // float angle = 0;

        // // this method is inefficient, but it works for ANY ANGLE, NOT JUST CARDINAL

        // if (Cross >= 0) angle = (1f - Dot) * 90f; // 0–180
        // else angle = 180f + (1f + Dot) * 90f; // 180–360
        // // Normalize angle to 0–360
        // angle -= 360 / (4 * 2);
        // angle = (angle % 360f + 360f) % 360f;

        // return HasWallAdjacent[(int)(angle / (360f/4))];
    }

    public TileProperties(Vector2Int setPos)
    {
        Position = setPos;
    }
}