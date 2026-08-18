using System.Collections.Generic;

namespace Navigation2D
{
    public class NavigationGraph
    {
        /// <summary>
        /// THE overarching graph. Contains:
        /// - the graph's global version
        /// - all chunks in the graph
        /// </summary>
        public int Version;
        private List<Waypoint> waypoints = new();
        private Dictionary<Waypoint, List<Waypoint>> graph = new();

        public void GenerateNavmesh()
        {
            waypoints.Clear();
            graph.Clear();
    
        }
    }
}
