using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;



/// <summary>
/// A helper class used by BehaviorController to manage character ai movement
/// Contains several different pathfinding solutions for different situations
///  - A* pathfinding
///  - Tile-based steering
/// </summary>
public class PathfinderModule
{
    // behavior controller data
    private BehaviorController _behaviorController;
    private Character _character => _behaviorController.ThisCharacter;

    public Stack<Vector2> path = new Stack<Vector2>();
    private Vector2 _targetPosition;

    public Vector2 MoveDir {get; private set;} = Vector2.zero;

    public PathfinderModule(BehaviorController behaviorController)
    {
        _behaviorController = behaviorController;
    }
    public void UpdatePathfinding(Vector2 targetPos)
    {
        // don't calculate a new path if the character is already at the targetPos
        if (Vector2Int.FloorToInt(targetPos) == _character.TilePosition) return;
        
        // get new path if no path
        if (path.Count == 0) GetNewPath(targetPos);


        Vector2 prev = path.Peek();
        foreach(Vector2 node in path)
        {
            Debug.DrawLine(prev, node, Color.green);
            prev = node;
        }

        MoveDir = _targetPosition - _character.Position;

        // skip tiles the character can clearly walk to, but also won't get stuck on a wall
        RaycastHit2D hit = Physics2D.Linecast(_character.Position, path.Peek(), 1 << 6);
        bool skipTile = hit.collider == null && path.Count > 1;
        if (skipTile)
        {
            foreach (Vector2Int dirVec in Directions2D.FourDirections)
            {
                if (MapManager.HasWallAt(dirVec+path.Peek()) && Vector2.Dot(dirVec, -MoveDir.normalized) > 0)
                {
                    Debug.DrawLine(path.Peek(), path.Peek() + dirVec, Color.red, 2);
                    skipTile = false;
                    break;
                }
            }
        }


        if (skipTile || MoveDir.sqrMagnitude < 0.025f)
        {
            Debug.DrawLine(_character.Position, path.Peek(), Color.black, 2);
            _targetPosition = path.Pop();
            if (path.Count == 0)
                _character.characterRelay.Invoke(CharacterEvent.MoveEnd);
        }
        else
        {
            //MoveDir = SmartSteering(MoveDir);
            _character.characterRelay.Invoke(CharacterEvent.MoveStart, MoveDir.normalized);
        
        }
    }

    public void GetNewPath(Vector2 targetPos)
    {
        if(!Physics2D.Linecast(_character.Position, targetPos, 1 << 6))
        {
            path.Push(targetPos);
        }
        else
        {
            bool pathfound = MapManager.FindPath(Vector2Int.FloorToInt(_character.Position), Vector2Int.FloorToInt(targetPos), path);

            if (pathfound) {
                Debug.DrawLine((Vector2)Vector2Int.FloorToInt(_character.Position), (Vector2)Vector2Int.FloorToInt(targetPos), Color.blue, 1);
                _targetPosition = path.Pop();
            }
            else
            {
                Debug.DrawLine((Vector2)Vector2Int.FloorToInt(_character.Position), (Vector2)Vector2Int.FloorToInt(targetPos), Color.red, 1000);
                Debug.Log($"Failed Pathfind to {targetPos}, Vector2Int Pos: {Vector2Int.FloorToInt(targetPos)}, Vector2 Contains Wall? : {MapManager.HasWallAt(targetPos)}, Vector2Int Contains Wall? : {MapManager.HasWallAt(Vector2Int.FloorToInt(targetPos))}");
            }   
        }
    }

    /// <summary>
    /// given a target direction, this function finds other obscales/obstructions in the surrounding area, allowing the entity to steer around them and maintain pursuit
    /// Copiles Vectors together, weighs them based on distance to entity and dot product with target direction
    /// </summary>
    /// <param name="targDir"></param>
    /// <returns></returns>
    public Vector2 SmartSteering(Vector2 targDir)
    {
        Vector2 steeringVector = Vector2.zero;
        foreach(Vector2Int offsetVec in Directions2D.GetDirectionArray(5, true))
        {
            if (!MapManager.IsTileOccupied(offsetVec + _character.TilePosition)) {
                MapManager.DrawTile(offsetVec + _character.TilePosition, Color.gray);
                continue;
            }
            
            float angleScalar = -Mathf.Max(0, Vector2.Dot(targDir.normalized, offsetVec));
            float distScalar = offsetVec.magnitude/5;
            Debug.DrawLine(_character.Position, _character.Position + (Vector2)offsetVec * angleScalar * distScalar, Color.white);
            steeringVector += (Vector2)offsetVec * angleScalar * distScalar;

        }
        Debug.DrawLine(_character.Position, _character.Position + targDir.normalized + steeringVector.normalized, Color.green);
        return targDir.normalized + steeringVector.normalized;
    }
}