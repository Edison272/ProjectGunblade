using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public enum PathFindingType
{
    NONE,
    TRACK, // moves directly towards target, or their last seen location
    SMART, // use A* to find a path to the target
}

/// <summary>
/// A helper class used by BehaviorController to manage character ai movement
/// Contains several different pathfinding solutions for different situations
///  - A* pathfinding
///  - Tile-based steering
/// </summary>
public class PathfinderModule
{
    // behavior controller data
    private readonly BehaviorController _behaviorController;
    private Character _character => _behaviorController.ThisCharacter;

    public readonly Stack<Vector2> path = new Stack<Vector2>();
    private Vector2 _targetPosition;
    private PathFindingType _pathfindingType = PathFindingType.NONE;
    private int _avoidRange = 1;

    public Vector2 MoveDir {get; private set;} = Vector2.zero;

    public PathfinderModule(BehaviorController behaviorController)
    {
        _behaviorController = behaviorController;
    }

    public void UpdatePathfinding(Vector2 targetPos)
    {
        // don't calculate a new path if the character is already at the targetPos
        if (Vector2Int.FloorToInt(targetPos) == _character.TilePosition)
        {
            MoveDir = Vector2.zero;
            return;
        }
        
        // get new path if no current path, or if a clear LOS to the target is possible
        RaycastHit2D hit = Physics2D.Linecast(_character.Position, targetPos, 1 << 6);
        if (path.Count == 0 || !hit)
        {
            _character.characterRelay.Invoke(CharacterEvent.MoveEnd);
            if (!SetNewPath(targetPos, !hit))
                return;
        }
        MoveDir = _targetPosition - _character.Position;

        Vector2 prev = _targetPosition;
        Debug.DrawLine(prev, _character.Position, Color.yellow);
        foreach(Vector2 node in path)
        {
            MapManager.DrawTile(Vector2Int.FloorToInt(node), Color.black, 0);
            Debug.DrawLine(prev, node, Color.green);
            prev = node;
        }


        if (MoveDir.sqrMagnitude < 0.025f)
        {
            _targetPosition = path.Pop();
        }
        else
        {
            //MoveDir = SmartSteering(MoveDir.normalized);
            _character.characterRelay.Invoke(CharacterEvent.MoveStart, MoveDir.normalized);
        
        }
    }

    public bool SetNewPath(Vector2 targetPos, bool directLine)
    {
        path.Clear();
        if(directLine)
        {
            _targetPosition = targetPos;
            return true;
        }
        else
        {
            bool pathfound = MapManager.FindPath(Vector2Int.FloorToInt(_character.Position), Vector2Int.FloorToInt(targetPos), path);

            if (pathfound) {
                _targetPosition = path.Pop();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// given a target direction, this function finds other obscales/obstructions in the surrounding area, allowing the entity to steer around them and maintain pursuit
    /// Copiles Vectors together, weighs them based on distance to entity and dot product with target direction
    /// </summary>
    /// <param name="targDir">
    /// takes in a normalized vector
    /// </param>
    /// <returns></returns>
    public Vector2 SmartSteering(Vector2 targDir)
    {
        Vector2 steeringVector = Vector2.zero;
        foreach(Vector2Int offsetVec in Directions2D.GetDirectionArray(_avoidRange, true))
        {
            if (!MapManager.IsTileOccupied(offsetVec + _character.TilePosition)) {
                MapManager.DrawTile(offsetVec + _character.TilePosition, Color.gray);
                continue;
            }
            
            float angleScalar = -Mathf.Max(0, Vector2.Dot(targDir, ((Vector2)offsetVec).normalized));
            float distScalar = _avoidRange / offsetVec.magnitude;
            Debug.DrawLine(_character.Position, _character.Position + (Vector2)offsetVec * angleScalar * distScalar, Color.white);
            steeringVector += (Vector2)offsetVec * angleScalar * distScalar;

        }
        Debug.DrawLine(_character.Position, _character.Position + targDir + steeringVector.normalized, Color.green);
        return (targDir + steeringVector.normalized).normalized;
    }
}