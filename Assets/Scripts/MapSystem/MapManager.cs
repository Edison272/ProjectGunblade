using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

using Random = UnityEngine.Random;
using System.IO; // LEAVE ME ALONE DAMNIT

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }
    [field: SerializeField] public Tilemap Floor { get; private set; }
    [field: SerializeField] public Tilemap Wall { get; private set; }
    public static Vector2 TILE_CENTER_OFFSET = new Vector2(0.5f, 0.5f);

    // used for tile array

    // adding offset because tilemap can go into negatives, but array only has min index of 0
    // add offset to world position vectors
    // subtract offset from x & y iterators when iterating through the 2d array
    public static Vector2Int VecIdxOffset => -(Vector2Int)Instance.Floor.cellBounds.min; 
    public static Vector2Int VecArrayMax => (Vector2Int)Instance.Floor.cellBounds.max - (Vector2Int)Instance.Floor.cellBounds.min;
    public static TileProperties[,] AllTiles;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Floor.CompressBounds();
        Wall.CompressBounds();
        
        AllTiles = new TileProperties[VecArrayMax.x, VecArrayMax.y];
        for (int x = 0; x < VecArrayMax.x; x++)
        {
            for (int y = 0; y < VecArrayMax.y; y++)
            {
                Vector2Int position = new Vector2Int(x, y) - VecIdxOffset;
                AllTiles[x, y] = new TileProperties(position);
            }
        }

    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // for (int x = 0; x < VecArrayMax.x; x++)
        // {
        //     for (int y = 0; y < VecArrayMax.y; y++)
        //     {
        //         TileProperties tileProperty = AllTiles[x, y];
        //         if (tileProperty.OccupiedByCharacter)
        //             DrawTile(tileProperty.Position, Color.red);
        //         else
        //             DrawTile(tileProperty.Position, Color.white);
        //     }
        // }
    }

    /// character.cs instances can call this function to update the "TileProperty" they are standing on every physics frame
    /// returns the tile property of the position it's occupying, if possible
    public static TileProperties UpdateCharTilePos(TileProperties currProperty, Vector2 currPos)
    {
        if (!Instance || !InMapBounds(currPos))
            return null;

        TileProperties checkProperty = GetTileProperties(currPos);
        if (checkProperty != currProperty) {
            if (currProperty != null)
                currProperty.OccupiedByCharacter = false;
                checkProperty.OccupiedByCharacter = true;
            return checkProperty;
        }
        else
        {
            currProperty.OccupiedByCharacter = true;
            return currProperty;
        }
    }

    public static TileProperties GetTileProperties(Vector2 pos)
    {
        return InMapBounds(pos) ? AllTiles[(int)Mathf.Floor(pos.x) + VecIdxOffset.x, (int)Mathf.Floor(pos.y) + VecIdxOffset.y] : null;
    }

    // check if there are adjacent tiles blocking a direction
    public static bool TileAdjacentsBlocked(Vector2 pos, Vector2 dir, bool diagonalsOnly = false)
    {
        return AllTiles[(int)Mathf.Floor(pos.x) + VecIdxOffset.x, (int)Mathf.Floor(pos.y) + VecIdxOffset.y].HasBlockedAdjacent(dir, diagonalsOnly);
    }

    public static bool InMapBounds(Vector2 pos)
    {
        return Instance.Floor.cellBounds.Contains(Vector3Int.FloorToInt((Vector3)pos));
    }
    public static bool HasWallAt(Vector2 pos)
    {
        return Instance.Wall.GetTile(Vector3Int.FloorToInt((Vector3)pos)) != null;
    }
    public static bool IsTileOccupied(Vector2Int pos)
    {
        return InMapBounds(pos) && (HasWallAt(pos) || AllTiles[pos.x + VecIdxOffset.x, pos.y + VecIdxOffset.y].OccupiedByCharacter); 
    }

    #region Pathfinding Assistance
    // A* pathfinding algorithm. returns the travel time, and returns -1 if no path could be found
    public static bool FindPath(Vector2Int startPos, Vector2Int endPos, Stack<Vector2> returnPath)
    {
        if (!InMapBounds(endPos) || !InMapBounds(startPos) || HasWallAt(endPos) || HasWallAt(startPos))
            return false;

        PathNode[,] nodes = new PathNode[VecArrayMax.x, VecArrayMax.y];
        List<Vector2Int> explore = new List<Vector2Int>();
        Vector2Int startIdx = startPos + VecIdxOffset;
        Vector2Int endIdx = endPos + VecIdxOffset;

        // Initialize start node
        PathNode startNode = new PathNode(startPos, startPos, endPos)
        {
            Evaluated = true,
            Explored = true
        };
        nodes[startIdx.x, startIdx.y] = startNode;

        PathNode currPathNode = startNode;
        bool pathFound = false;
        int failSafe = 0;

        while (!pathFound && failSafe < 10000)
        {
            foreach (Vector2Int dirVec in Directions2D.EightDirections)
            {
                Vector2Int checkPos = currPathNode.Position + dirVec;

                // Skip check if :
                // - Is not within the tilemap bounds
                // - Is a Wall Tile
                if (
                    !Instance.Floor.cellBounds.Contains((Vector3Int)checkPos)
                    || Instance.Wall.GetTile((Vector3Int)checkPos) != null 
                    || (dirVec.sqrMagnitude > 1 && GetTileProperties(currPathNode.Position).HasBlockedAdjacent(dirVec))
                )
                    continue;

                // get check node (could be uninitialized, could be preexisting)
                Vector2Int checkIdx = checkPos + VecIdxOffset;
                PathNode checkNode = nodes[checkIdx.x, checkIdx.y];

                // If this is the end position, create its node and finish
                if (checkPos == endPos)
                {
                    PathNode endNode = new PathNode(endPos, startPos, endPos)
                    {
                        Explored = true,
                        Evaluated = true,
                        PointPathDir = -dirVec
                    };

                    nodes[endIdx.x, endIdx.y] = endNode;
                    currPathNode = endNode;
                    pathFound = true;
                    break;
                }

                // If node already exists and was evaluated, skip
                if (checkNode.Initialized && checkNode.Evaluated)
                    continue;

                // If node already exists and has been explored, but NOT evaluated, update parent direction
                if (checkNode.Initialized && checkNode.Explored)
                {
                    
                    uint newCost = PathNode.GetDistance(checkNode.Position, currPathNode.Position) + currPathNode.StartDist;
                    if (newCost < checkNode.StartDist)
                    {
                        checkNode.PointPathDir = -dirVec;
                        checkNode.StartDist = newCost;
                        nodes[checkIdx.x, checkIdx.y] = checkNode;
                    }
                    continue;
                }

                // Create new node
                PathNode newNode = new PathNode(checkPos, currPathNode.Position, endPos)
                {
                    Explored = true,
                    PointPathDir = -dirVec,
                    StartDist = PathNode.GetDistance(checkPos, currPathNode.Position) + currPathNode.StartDist
                };

                nodes[checkIdx.x, checkIdx.y] = newNode;
                explore.Add(checkPos);
            }

            // search all explored nodes. find the lowest cost node and mark it as "evaluated"
            if (!pathFound)
            {
                float lowestCost = Mathf.Infinity;
                PathNode cheapest = currPathNode;

                foreach (Vector2Int pos in explore)
                {
                    Vector2Int idx = pos + VecIdxOffset;
                    PathNode node = nodes[idx.x, idx.y];

                    // do not consider the node if it's an empty node, or if it's already been evaluated before
                    if (!node.Initialized || node.Evaluated)
                        continue;

                    if (node.TotalCost < lowestCost)
                    {
                        lowestCost = node.TotalCost;
                        cheapest = node;
                    }
                }

                currPathNode = cheapest;
                currPathNode.Evaluated = true;

                Vector2Int addIdx = currPathNode.Position + VecIdxOffset;
                nodes[addIdx.x, addIdx.y] = currPathNode;
            }

            failSafe++;
        }

        // Backtrack
        if (pathFound)
        {
            returnPath.Push(endPos + TILE_CENTER_OFFSET);
            failSafe = 0;
            Vector2 prevDir = nodes[endPos.x + VecIdxOffset.x, endPos.y + VecIdxOffset.y].PointPathDir;
            while (currPathNode.Position != startPos && failSafe < 1000)
            {
                // only add to the path if the node switches direction
                if (currPathNode.PointPathDir != prevDir)
                    returnPath.Push(currPathNode.Position + TILE_CENTER_OFFSET);

                Vector2Int parentPos = currPathNode.Position + currPathNode.PointPathDir;
                Vector2Int parentIdx = parentPos + VecIdxOffset;
                prevDir = currPathNode.PointPathDir;

                currPathNode = nodes[parentIdx.x, parentIdx.y];
                failSafe++;
            }

            returnPath.Push(startPos + TILE_CENTER_OFFSET);
        }

        return pathFound;
    }

    public struct PathNode
    {
        public bool Initialized;
        public Vector2Int Position;
        public Vector2Int PointPathDir;
        public uint StartDist; // g cost
        public uint EndDist; // h cost
        public uint TotalCost => StartDist + EndDist;
        public bool Evaluated;
        public bool Explored;

        public PathNode(Vector2Int thisPos, Vector2Int startPos, Vector2Int endPos)
        {
            Initialized = true;
            Position = thisPos;
            StartDist = GetDistance(thisPos, startPos);
            EndDist = GetDistance(thisPos, endPos);
            PointPathDir = Vector2Int.zero;
            Evaluated = false;
            Explored = false;
        }


        public static uint GetDistance(Vector2Int posA, Vector2Int posB) {
            uint dstX = (uint)Mathf.Abs(posA.x - posB.x);
            uint dstY = (uint)Mathf.Abs(posA.y - posB.y);

            if (dstX > dstY)
                return 14*dstY + 10* (dstX-dstY);
            return 14*dstX + 10 * (dstY-dstX);
        }
    }
    #endregion

    #region Tools 
    public static void DrawTile(Vector2Int pos, Color line_color, float time = 0)
    {
        Debug.DrawLine(
            (Vector2)pos, 
            (Vector2)(pos + Directions2D.FourDirections[1]), 
            line_color,
            time
            );
        Debug.DrawLine(
            (Vector2)(pos + Directions2D.FourDirections[1]), 
            (Vector2)(pos + Directions2D.FourDirections[1] + Directions2D.FourDirections[0]), 
            line_color,
            time 
            );
        Debug.DrawLine(
            (Vector2)pos, 
            (Vector2)(pos + Directions2D.FourDirections[0]), 
            line_color,
            time
            );
        Debug.DrawLine(
            (Vector2)(pos + Directions2D.FourDirections[0]), 
            (Vector2)(pos + Directions2D.FourDirections[0] + Directions2D.FourDirections[1]), 
            line_color,
            time
            );
        // Crosses
        Debug.DrawLine(
            (Vector2)(pos), 
            (Vector2)(pos + Directions2D.FourDirections[0] + Directions2D.FourDirections[1]), 
            line_color,
            time
            );
        Debug.DrawLine(
            (Vector2)(pos + Directions2D.FourDirections[0]), 
            (Vector2)(pos + Directions2D.FourDirections[1]), 
            line_color,
            time
            );
    }
    #endregion
}