using UnityEngine;
using System;


/// <summary>
/// class contains data carried by each individual tile
/// </summary>
public class TileProperties
{
    public Vector2Int Position;
    public bool OccupiedByCharacter;

    public TileProperties(Vector2Int setPos)
    {
        Position = setPos;
    }
}