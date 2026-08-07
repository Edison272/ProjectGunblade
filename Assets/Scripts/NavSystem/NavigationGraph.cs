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
        public readonly List<NavigationChunk> Chunks = new();
    }
}
