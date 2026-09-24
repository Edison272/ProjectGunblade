using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.Collections.LowLevel.Unsafe;

namespace GameAI.Factions
{    /// <summary>
    /// This is intentionally a plain C# class, not a MonoBehaviour or ScriptableObject —
    /// its data changes constantly at runtime and is owned/managed by FactionManager.
    /// </summary>
    public enum TargetType {Closest, Furthest, MostHP, LeastHP}
    [Serializable]
    public class FactionData
    {
        public readonly string FactionName;
        public readonly int FactionID; // The integer value used to find the integer-index of the faction in the Manager
        public readonly int FactionLayer; // Player and Enemy factions will get a reserved physics layer

        /// <summary>Shared memory for this faction. 
        /// Each Character under this faction shares and updates the same blackboard with info
        public readonly Blackboard SharedBlackboard = new Blackboard();

        /// <summary> Members are assigned to squads
        public readonly List<Squad> Squads = new List<Squad>();

        public FactionData(int setID, string setName = "Nameless", int setLayer = FactionManager.NoFactionLayer)
        {
            FactionID = setID;
            FactionName = setName;
            FactionLayer = setLayer;
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

        #region Membership
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

        // add a character to a squad, or create a new one for them if no one's around
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
            Squads.Add(nearestSquad);
            return nearestSquad;
        }
        #endregion

        // characters belonging to this faction can use this function to get a layermask to find who they can target for attacks/abilities
        public LayerMask GetTargetMask(TargetFaction targets)
        {
            LayerMask curr_mask = 1 << 6; // terrain wall by default
            switch (targets)
            {
                case TargetFaction.Allies:
                    curr_mask |= 1 << FactionLayer;
                    break;
                case TargetFaction.Enemies:
                    if (FactionLayer == FactionManager.NoFactionLayer) // "factionless factions" are independent, and can attack each other
                        curr_mask |= FactionManager.FactionLayers;
                    else
                        curr_mask |= FactionManager.FactionLayers & ~(1<<FactionLayer);
                    break;
                case TargetFaction.Both:
                    curr_mask |= FactionManager.FactionLayers;
                    break;
            }
            return curr_mask;
        }
    }
}