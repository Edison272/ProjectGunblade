using System.Collections.Generic;
 
namespace GameAI
{
    /// <summary>
    /// Shared memory for an agent. Perception writes facts here,
    /// Utility AI reads/writes goal state, Behavior Tree reads/writes execution state.
    /// Keeping this as the single source of truth avoids tight coupling between systems.
    /// </summary>
    public class Blackboard
    {
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
 
        public void Set<T>(string key, T value) => _data[key] = value;
 
        public T Get<T>(string key, T defaultValue = default)
        {
            if (_data.TryGetValue(key, out var value) && value is T typed)
                return typed;
            return defaultValue;
        }
 
        public bool Has(string key) => _data.ContainsKey(key);
 
        public void Clear(string key) => _data.Remove(key);
 
        // --- Common well-known keys, as constants to avoid string typos across the codebase ---
        public static class Keys
        {
            public const string Target = "Target";                 // Transform
            public const string LastKnownTargetPos = "LastKnownTargetPos"; // Vector3
            public const string Health = "Health";                 // float
            public const string PatrolPoints = "PatrolPoints";      // Transform[]
            public const string PatrolIndex = "PatrolIndex";        // int
            public const string CurrentGoal = "CurrentGoal";        // string (e.g. "Chase", "Patrol", "Flee")
            public const string CanSeeTarget = "CanSeeTarget";      // bool
            public const string HeardNoiseAt = "HeardNoiseAt";      // Vector3?
        }
    }
}