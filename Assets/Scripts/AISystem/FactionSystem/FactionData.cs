using UnityEngine;
using System.Collections.Generic;

namespace GameAI.Factions
{
    public enum FactionStanding { Hostile, Neutral, Friendly }

    /// <summary>
    /// Represents one faction: its identity, its relationships to other factions,
    /// its current member list, and a shared Blackboard that all members of this
    /// faction can read/write for faction-wide facts (e.g. "last known player position
    /// reported by ANY member", "current alert level").
    ///
    /// This is intentionally a plain C# class, not a MonoBehaviour or ScriptableObject —
    /// its data changes constantly at runtime and is owned/managed by FactionManager.
    /// </summary>
    public class FactionData
    {
        public readonly string FactionName;

        /// <summary>Shared memory for this faction. 
        /// Each Character under this faction shares and updates the same blackboard with info
        public readonly Blackboard SharedBlackboard = new Blackboard();

        /// <summary>Members currently belonging to this faction. 
        public readonly List<Character> Members = new List<Character>();

        public FactionData(string factionName)
        {
            FactionName = factionName;
        }

        public FactionData(FactionInitializer factionInitializer)
        {
            FactionName = factionInitializer.FactionName;
            foreach(CharacterSpawner spawner in factionInitializer.DefaultMembers)
            {
                CharacterSpawner new_spawner = MonoBehaviour.Instantiate(spawner, Vector3.zero, Quaternion.identity);
                foreach(Character new_member in new_spawner.StartSpawn())
                {
                    Members.Add(new_member);
                    Members[Members.Count-1].SetFaction(FactionName);
                }
            }
        }

        public void AddMember(Character member)
        {
            if (!Members.Contains(member)) Members.Add(member);
        }

        public void RemoveMember(Character member)
        {
            Members.Remove(member);
        }

        // --- Common well-known faction-blackboard keys, mirroring Blackboard.Keys pattern ---
        public static class Keys
        {
            public const string AlertLevel = "AlertLevel";                     // float 0..1, e.g. drives music/spawns
            public const string LastReportedPlayerPos = "LastReportedPlayerPos"; // Vector3
            public const string LastReportedTime = "LastReportedTime";          // float (Time.time)
        }
    }
}