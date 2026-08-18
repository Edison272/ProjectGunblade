using System.Collections.Generic;
using UnityEngine;

namespace Navigation2D
{
    public class NavigationChunk
    {
        /// Navmeshs will be divided into chunks to make calculations easier Contains:
        /// - Set worldspace bouns (can potentially overlap with others)
        /// - Maintains a "version", so that if a chunk is rebaked after a change, an agent can ask to recalculate if the version is off
        /// - List of Nodes
        public Bounds Bounds;
        public int Version;

        public readonly List<Waypoint> Nodes = new();
    }
}
