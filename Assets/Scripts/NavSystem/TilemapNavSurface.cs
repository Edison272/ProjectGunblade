using UnityEngine;
using UnityEngine.Tilemaps;

namespace Navigation2D
{
    public class TilemapNavSurface : MonoBehaviour
    {
        
        public Tilemap Floor;
        public Tilemap Walls;
        private NavigationGraph _graph = new();

        public NavigationGraph Graph => _graph;

        public void FullBake()
        {
            Debug.Log("FullBake() not implemented yet.");
        }

        public void ReBake(BoundsInt dirtyArea)
        {
            Debug.Log($"ReBake({dirtyArea}) not implemented yet.");
        }
        
        public void GenerateNavmesh()
        {
            
        }
    }

}
