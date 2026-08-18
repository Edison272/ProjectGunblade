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

    public static bool FindPath(Vector2Int startPos, Vector2Int endPos, Stack<Vector2Int> returnPath)
    {
        // one array to contain all path nodes, and one list to keep track of traversal
        // list contains x and y indexes to access nodes
        PathNode[,] nodes = new PathNode[VecArrayMax.x,VecArrayMax.y];
        List<Vector2Int> explore = new List<Vector2Int>();

        // set base value
        PathNode currPathNode = new PathNode(startPos, startPos, endPos);
        currPathNode.Evaluated = true;
        currPathNode.Explored = true;
        nodes[startPos.x + VecIdxOffset.x, startPos.y + VecIdxOffset.y] = currPathNode;

        bool pathFound = false;
        int failSafe = 0;

        while(!pathFound && failSafe < 1000)
        {
            // get adjacents
            foreach(Vector2Int dirVec in Directions2D.FourDirections)
            {
                Vector2Int checkPos = currPathNode.Position + dirVec;
                
                // skip if wall
                if (!Instance.Floor.cellBounds.Contains((Vector3Int)checkPos) || Instance.Wall.GetTile((Vector3Int)checkPos) || nodes[checkPos.x + VecIdxOffset.x, checkPos.y + VecIdxOffset.y].Evaluated)
                {
                    continue;
                }
                else if (checkPos == endPos)
                {
                    pathFound = true;
                    explore.Clear();
                }
                else if (nodes[checkPos.x + VecIdxOffset.x, checkPos.y + VecIdxOffset.y].Explored)
                {
                    currPathNode.PointPathDir = Vector2Int.zero - dirVec;
                    continue;
                }

                PathNode newNode = new PathNode(checkPos, startPos, endPos)
                {
                    PointPathDir = Vector2Int.zero - dirVec,
                    Explored = true
                };
                nodes[checkPos.x + VecIdxOffset.x, checkPos.y + VecIdxOffset.y] = newNode;
                explore.Add(checkPos);
            }

            // traverse to lowest cost
            float lowestCost = Mathf.Infinity;
            PathNode cheapest = currPathNode;
            foreach(Vector2Int checkPos in explore)
            {
                PathNode checkNode = nodes[checkPos.x + VecIdxOffset.x, checkPos.y + VecIdxOffset.y];
                if (checkNode.TotalCost < lowestCost)
                {
                    cheapest = checkNode;
                    lowestCost = checkNode.TotalCost;
                }
            }
            
            currPathNode = cheapest;
            currPathNode.Evaluated = true;
            explore.Remove(cheapest.Position);

            failSafe++;
        }
        // backtrack
        if (pathFound)
        {
            returnPath.Push(endPos);
            failSafe = 0;
            while (currPathNode.Position != startPos && failSafe < 100)
            {
                returnPath.Push(currPathNode.Position);
                currPathNode = nodes[currPathNode.Position.x + VecIdxOffset.x, currPathNode.Position.y + VecIdxOffset.y];
                failSafe++;
            }
            returnPath.Push(startPos);
        }



        return pathFound;
    }

    public struct PathNode
    {
        public Vector2Int Position; 
        public Vector2Int PointPathDir; // is vector2int.zero by default (unexplored)
        public float StartDist; // G
        public float EndDist; // H
        public float TotalCost => StartDist + EndDist; // F
        public bool Evaluated; // true if this has already been evaluated as a potential path
        public bool Explored; // true if this has already been in the explore list
        public PathNode(Vector2Int thisPos, Vector2Int startPos, Vector2Int endPos)
        {
            Position = thisPos;
            StartDist = (thisPos - startPos).sqrMagnitude;
            EndDist = (thisPos - endPos).sqrMagnitude;
            PointPathDir = Vector2Int.zero;
            Evaluated = false;
            Explored = false;
        }

    }
}
