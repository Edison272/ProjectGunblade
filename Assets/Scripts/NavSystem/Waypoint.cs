using System.Collections.Generic;
using UnityEngine;

namespace Navigation2D
{
    /// <summary>
    /// the basic unit of the NavMesh. Contains:
    ///  - Id for lookups within the chunk
    ///  - Position for Unity world space
    /// - future proofing for potentially traversing different terrain types
    /// - A list of neighbors
    /// </summary>
    public sealed class Waypoint
    {
        public int Id;
        public Vector2 Position;
        public uint TraversalMask = uint.MaxValue;
        public IReadOnlyList<WaypointNeighbor> Neighbors => _neighbors;
        private readonly WaypointNeighbor[] _neighbors;
    }
}
