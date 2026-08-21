using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;

using Random = UnityEngine.Random;

public static class Directions2D 
{
    #region Basic Cardinal & Diagonal directions
    public enum DirArray {FOUR, EIGHT, HORZ_WEIGHT_FOUR, HORZ_WEIGHT_EIGHT};
    public static readonly Vector2Int[] FourDirections = {
        new Vector2Int(1, 0), 
        new Vector2Int(0, 1), 
        new Vector2Int(-1, 0), 
        new Vector2Int(0, -1)
    };

    public static readonly Vector2Int[] EightDirections = {
        new Vector2Int(1, 0), 
        new Vector2Int(1, 1), 
        new Vector2Int(0, 1), 
        new Vector2Int(-1, 1), 
        new Vector2Int(-1, 0), 
        new Vector2Int(-1, -1), 
        new Vector2Int(0, -1),
        new Vector2Int(1, -1), 
    };
    public static readonly Vector2Int[] horz_weight_four_dir = {
        new Vector2Int(1, 0), 
        new Vector2Int(1, 0), 
        new Vector2Int(0, 1), 
        new Vector2Int(-1, 0), 
        new Vector2Int(-1, 0), 
        new Vector2Int(0, -1)
    };
    public static readonly Vector2Int[] horz_weight_eight_dir = { // heavily favor horizontal movement, then favor vertical movement, and lastly diagonal movement
        new Vector2Int(1, 0), 
        new Vector2Int(1, 0), 
        new Vector2Int(1, 0), 
        new Vector2Int(1, 1), 
        new Vector2Int(0, 1), 
        new Vector2Int(0, 1), 
        new Vector2Int(-1, 1), 
        new Vector2Int(-1, 0), 
        new Vector2Int(-1, 0), 
        new Vector2Int(-1, 0), 
        new Vector2Int(-1, -1), 
        new Vector2Int(0, -1),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1), 
    };

    public static Vector2Int GetRandomDirection(DirArray direction_array)
    {
        Vector2Int direction = Vector2Int.zero;
        switch(direction_array)
        {
            case DirArray.FOUR:
                direction = FourDirections[Random.Range(0, FourDirections.Length)];
                break;
            case DirArray.EIGHT:
                direction = EightDirections[Random.Range(0, EightDirections.Length)];
                break;
            case DirArray.HORZ_WEIGHT_FOUR:
                direction = horz_weight_four_dir[Random.Range(0, horz_weight_four_dir.Length)];
                break;
            case DirArray.HORZ_WEIGHT_EIGHT:
                direction = horz_weight_eight_dir[Random.Range(0, horz_weight_eight_dir.Length)];
                break;
        }
        return direction;
    }


    public static void DirectionsFromPoint(List<Vector2Int> list_pointer, DirArray direction_array, bool randomize = false)
    {
        Vector2Int[] dir_pointer = FourDirections;
        switch(direction_array)
        {
            case DirArray.FOUR:
                dir_pointer = FourDirections;
                break;
            case DirArray.EIGHT:
                dir_pointer = EightDirections;
                break;
            case DirArray.HORZ_WEIGHT_FOUR:
                dir_pointer = horz_weight_four_dir;
                break;
            case DirArray.HORZ_WEIGHT_EIGHT:
                dir_pointer = horz_weight_eight_dir;
                break;
        }

        list_pointer.Clear();
        foreach(Vector2Int dir in dir_pointer)
        {
            list_pointer.Add(dir);
        }

        if (randomize) // switch around the list values to randomize it
        {
            for (int i = 0; i < list_pointer.Count; i++)
            {
                Vector2Int temp = list_pointer[i];
                int randint = Random.Range(0, list_pointer.Count);
                list_pointer[i] = list_pointer[randint];
                list_pointer[randint] = temp;
            }
        }
    }
    #endregion

    #region Specific width direction array

    private static Dictionary<(int, bool), Vector2Int[]> _allDirectionArays = new Dictionary<(int, bool), Vector2Int[]>();

    // given size, returns a radius x radius array of Vectir2Int offsets from 0,0
    // radius MUST be atleast 1. 1 is the basic eight tiles around a single cemter tile. each radius represents one "ring"
    // generates an array if DNE in static dict. adds it to dict for retrieval
    public static Vector2Int[] GetDirectionArray(int radius, bool circular = false) // square radius
    {
        if (_allDirectionArays.ContainsKey((radius, circular)))
            return _allDirectionArays[(radius, circular)];
        
        
        List<Vector2Int> returnVec = new List<Vector2Int>();
        radius = Mathf.Max(1, radius);
        for (int r = 1; r <= radius; r++)
        {
            Vector2Int addVec = new Vector2Int(-r, -r); // the vec that will be added to the array
            for (int i = 0; i < r * 8; i++)
            {
                addVec += FourDirections[(int)(i/(r*2))];
                if (circular && addVec.sqrMagnitude > radius*radius)
                    continue;
                returnVec.Add(addVec);
            }
        }
        _allDirectionArays.Add((radius, circular), returnVec.ToArray());

        return _allDirectionArays[(radius, circular)];
    }

    #endregion

    
}