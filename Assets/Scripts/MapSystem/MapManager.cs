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
    public static Vector2Int VecIdxOffset => -(Vector2Int)Instance.Floor.cellBounds.min;
    public static Vector2Int VecArrayMax => (Vector2Int)Instance.Floor.cellBounds.max - (Vector2Int)Instance.Floor.cellBounds.min;

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
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public static bool InMapBounds(Vector2 pos)
    {
        return Instance.Floor.cellBounds.Contains(Vector3Int.FloorToInt((Vector3)pos));
    }
    public static bool HasObstacleAt(Vector2 pos)
    {
        return Instance.Wall.GetTile(Vector3Int.FloorToInt((Vector3)pos)) != null;
    }

    public static bool FindPath(Vector2Int startPos, Vector2Int endPos, Stack<Vector2> returnPath)
    {
        
        if (!InMapBounds(endPos) || HasObstacleAt(endPos))
        {
            return false;
        }
        
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
            foreach (Vector2Int dirVec in Directions2D.FourDirections)
            {
                Vector2Int checkPos = currPathNode.Position + dirVec;

                // Tilemap bounds check
                if (!Instance.Floor.cellBounds.Contains((Vector3Int)checkPos))
                    continue;

                // Wall check
                if (Instance.Wall.GetTile((Vector3Int)checkPos) != null)
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
                    checkNode.PointPathDir = -dirVec;
                    nodes[checkIdx.x, checkIdx.y] = checkNode;
                    continue;
                }

                // Create new node
                PathNode newNode = new PathNode(checkPos, startPos, endPos)
                {
                    Explored = true,
                    PointPathDir = -dirVec
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

            while (currPathNode.Position != startPos && failSafe < 1000)
            {
                returnPath.Push(currPathNode.Position + TILE_CENTER_OFFSET);

                Vector2Int parentPos = currPathNode.Position + currPathNode.PointPathDir;
                Vector2Int parentIdx = parentPos + VecIdxOffset;

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
        public float StartDist;
        public float EndDist;
        public float TotalCost => StartDist + EndDist;
        public bool Evaluated;
        public bool Explored;

        public PathNode(Vector2Int thisPos, Vector2Int startPos, Vector2Int endPos)
        {
            Initialized = true;
            Position = thisPos;
            StartDist = (thisPos - startPos).sqrMagnitude;
            EndDist = (thisPos - endPos).sqrMagnitude;
            PointPathDir = Vector2Int.zero;
            Evaluated = false;
            Explored = false;
        }
    }
}