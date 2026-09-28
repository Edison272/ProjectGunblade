using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class TilemapMapper : MonoBehaviour
{
    [Header("Tilemap References")]
    [SerializeField] private Tilemap referenceTilemap;
    [SerializeField] private Tilemap targetTilemap;

    [Header("Target Tile")]
    [Tooltip("The tile placed on the target map's outward edge. (Highly recommended to assign a specific tile here).")]
    [SerializeField] private TileBase ruleOrFillTile;

    [Header("Edge Settings")]
    [Range(1, 10)]
    [Tooltip("How many layers thick should the outward border expand?")]
    [SerializeField] private int edgeThickness = 1;

    private readonly Vector3Int[] cardinalDirections = new Vector3Int[]
    {
        new Vector3Int(0, 1, 0),  // Up
        new Vector3Int(0, -1, 0), // Down
        new Vector3Int(-1, 0, 0), // Left
        new Vector3Int(1, 0, 0)   // Right
    };

    public void FillTargetMapOutwardEdges()
    {
        if (referenceTilemap == null || targetTilemap == null)
        {
            Debug.LogError("TilemapMapper: Please assign both Reference and Target Tilemaps.", this);
            return;
        }

        #if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(targetTilemap, "Fill Outward Edges");
        #endif

        targetTilemap.ClearAllTiles();
        BoundsInt bounds = referenceTilemap.cellBounds;

        // Tracks every coordinate that qualifies for the target map
        HashSet<Vector3Int> totalOutwardLayers = new HashSet<Vector3Int>();
        
        // Tracks the current active wave expanding outward
        List<Vector3Int> currentLayer = new List<Vector3Int>();

        // --- LAYER 1: Find empty positions directly touching a reference tile ---
        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            if (referenceTilemap.HasTile(position))
            {
                // Look at neighbors of this filled tile
                foreach (Vector3Int direction in cardinalDirections)
                {
                    Vector3Int neighborPos = position + direction;

                    // If it is completely empty space, it's a valid Layer 1 position
                    if (!referenceTilemap.HasTile(neighborPos) && !totalOutwardLayers.Contains(neighborPos))
                    {
                        currentLayer.Add(neighborPos);
                        totalOutwardLayers.Add(neighborPos);
                    }
                }
            }
        }

        // --- LAYERS 2+: Expand outward iteratively ---
        for (int layer = 1; layer < edgeThickness; layer++)
        {
            List<Vector3Int> nextLayer = new List<Vector3Int>();

            foreach (Vector3Int activePos in currentLayer)
            {
                foreach (Vector3Int direction in cardinalDirections)
                {
                    Vector3Int neighborPos = activePos + direction;

                    // Validation checklist for expanding outward:
                    // 1. MUST NOT collide with an existing tile on the reference tilemap
                    // 2. MUST NOT have already been added to a previous outward layer
                    if (!referenceTilemap.HasTile(neighborPos) && !totalOutwardLayers.Contains(neighborPos))
                    {
                        nextLayer.Add(neighborPos);
                        totalOutwardLayers.Add(neighborPos);
                    }
                }
            }

            if (nextLayer.Count == 0) break;
            currentLayer = nextLayer;
        }

        // --- DRAWING: Write coordinates to target map ---
        foreach (Vector3Int targetPos in totalOutwardLayers)
        {
            if (ruleOrFillTile != null)
            {
                targetTilemap.SetTile(targetPos, ruleOrFillTile);
            }
            else
            {
                // Fallback: If no tile is specified, sample an adjacent reference tile to mimic it
                targetTilemap.SetTile(targetPos, GetApproximateSampleTile(targetPos));
            }
        }

        #if UNITY_EDITOR
        EditorUtility.SetDirty(targetTilemap.gameObject);
        #endif
        
        Debug.Log($"TilemapMapper: Generated outward edge with thickness {edgeThickness} without colliding with reference tiles!", this);
    }

    private TileBase GetApproximateSampleTile(Vector3Int targetPos)
    {
        // Simple fallback to find any valid tile nearby to copy if ruleOrFillTile is blank
        foreach (Vector3Int direction in cardinalDirections)
        {
            Vector3Int check = targetPos + direction;
            if (referenceTilemap.HasTile(check))
            {
                return referenceTilemap.GetTile(check);
            }
        }
        return null;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TilemapMapper))]
public class TilemapMapperEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(15);

        TilemapMapper mapper = (TilemapMapper)target;

        GUI.backgroundColor = new Color(0.6f, 0.3f, 0.8f); // Purple tint for expansion mode
        if (GUILayout.Button("Expand Outward Edges", GUILayout.Height(35)))
        {
            mapper.FillTargetMapOutwardEdges();
        }
    }
}
#endif