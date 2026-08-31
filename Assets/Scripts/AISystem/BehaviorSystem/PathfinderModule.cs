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

    // internal
    public readonly Stack<Vector2> path = new Stack<Vector2>();
    private Vector2 _targetPosition;
    private PathFindingType _pathfindingType = PathFindingType.NONE;
    private bool _hasLOS; 
    private int _avoidRange = 1;
    public Vector2 MoveDir {get; private set;} = Vector2.zero;

    // state data
    public bool IsPathing => MoveDir.sqrMagnitude > 0;


    // important

    

    public PathfinderModule(BehaviorController behaviorController)
    {
        _behaviorController = behaviorController;
        _targetPosition = _character.Position;
    }

    public void UpdatePathfinding()
    {
        // don't calculate a new path if the character is already at the targetPos
        if (Vector2Int.FloorToInt(_targetPosition) == _character.TilePosition)
        {
            if (path.Count == 0)
            {
                MoveDir = Vector2.zero;
                _character.StopMove();
                return;
            }
            else
            {
                _targetPosition = path.Pop();
            }
        }
        
        MoveDir = _targetPosition - _character.Position;
        // add new path when LOS is broken
        // RaycastHit2D hit = Physics2D.Linecast(_character.TilePosition, _targetPosition, 1 << 6);
        // if (hit) // add a new node to avoid obstacles in the path
        // {
        //     // if (_hasLOS)
        //     // {
        //     //     Vector2 pushPoint = hit.point + Vector2.Perpendicular(MoveDir.normalized)*0.25f;
        //     //     if (!MapManager.HasWallAt(pushPoint))
        //     //         path.Push(Vector2Int.FloorToInt(pushPoint));
        //     //         _hasLOS = false;
        //     // }
        // }
        // else // if path is large, use raycasts to see if all nodes of path need to be followed
        // {
        //     _hasLOS = true;
        //     // if (path.Count > 1)
        //     // {
        //     //     // skip tiles the character can clearly walk to, but also won't get stuck on a wall
        //     //     if (!MapManager.TileAdjacentsBlocked(path.Peek(), _character.TilePosition-path.Peek()))
        //     //     {
        //     //         MapManager.DrawTile(Vector2Int.FloorToInt(path.Peek()), Color.red, 2);
        //     //         _targetPosition = path.Pop();
        //     //     }
        //     // }
        // }

        Vector2 prev = _targetPosition;
        Debug.DrawLine(prev, _character.Position, Color.yellow);
        foreach(Vector2 node in path)
        {
            MapManager.DrawTile(Vector2Int.FloorToInt(node), Color.black, 0);
            Debug.DrawLine(prev, node, Color.green);
            prev = node;
        }

        //MoveDir = SmartSteering(MoveDir.normalized);
        Debug.DrawLine(_character.Position, _character.Position + MoveDir, Color.green);
        _character.StartMove(MoveDir.normalized);
    }

    public bool FindPath(Vector2 targetPos)
    {
        return SetNewPath(targetPos, !Physics2D.Linecast(_character.Position, targetPos, 1 << 6));
    }

    public bool SetNewPath(Vector2 targetPos, bool directLine)
    {
        Reset();
        if(directLine)
        {
            _targetPosition = Vector2Int.FloorToInt(targetPos) + MapManager.TILE_CENTER_OFFSET;
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
    public void Reset()
    {
        MoveDir = Vector2.zero;
        _targetPosition = _character.Position;
        path.Clear();
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
        Vector2 netVec = Vector2.zero;
        foreach(Vector2Int offsetVec in Directions2D.GetDirectionArray(_avoidRange, true))
        {
            Vector2 steerVec = ((Vector2)offsetVec).normalized * Mathf.Clamp01(Vector2.Dot(targDir, ((Vector2)offsetVec).normalized));
            if (MapManager.IsTileOccupied(offsetVec + _character.TilePosition)) {
                float distScalar = Mathf.Clamp01((_avoidRange * _avoidRange) / offsetVec.sqrMagnitude);
                netVec += steerVec * -distScalar;
            }
        }
        return (targDir + netVec.normalized).normalized;
    }
}