using UnityEngine;
using System.Collections.Generic;
using System;

namespace GameAI.Factions
{    /// <summary>
    /// This is intentionally a plain C# class, not a MonoBehaviour or ScriptableObject —
    /// its data changes constantly at runtime and is owned/managed by FactionManager.
    /// </summary>
    [Serializable]
    public class FactionData
    {
        public readonly string FactionName;

        /// <summary>Shared memory for this faction. 
        /// Each Character under this faction shares and updates the same blackboard with info
        public readonly Blackboard SharedBlackboard = new Blackboard();

        /// <summary> Members are assigned to squads
        public readonly List<Squad> Squads = new List<Squad>();

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
                Squad newSquad = new Squad(this, new_spawner.StartSpawn());
                Squads.Add(newSquad);
            }
        }

        public Squad AddMember(Character member, Squad squad = null)
        {
            if (squad == null)
                squad = AssignSquad(member);
            
            squad.AddMember(member);
            return squad;
        }
        public void RemoveMember(Character member, Squad squad)
        {
            squad.RemoveMember(member);
        }
        public void RemoveSquad(Squad squad)
        {
            Squads.Remove(squad);
        }

        public Squad AssignSquad(Character member)
        {
            Squad nearestSquad = null;
            float closestDist = 100f;
            foreach(Squad otherSquad in Squads)
            {
                float currDist = (otherSquad.Position - member.Position).sqrMagnitude;
                if (currDist < closestDist)
                {
                    closestDist = currDist;
                    nearestSquad = otherSquad;
                }
            }

            // if no nearby squad exists, make one
            if (nearestSquad == null)
            {
                nearestSquad = new Squad(this);
            }
            return nearestSquad;
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