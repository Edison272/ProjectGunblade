using UnityEngine;

namespace Navigation2D
{
    // Node-to-node connector. a single node can have multiple neighbors, and the cost the travel to them
    public struct NavigationNeighbor
    {
        public int NodeIndex;
        public float Cost;

        public NavigationNeighbor(int nodeIndex, float cost)
        {
            NodeIndex = nodeIndex;
            Cost = cost;
        }
    }
}
