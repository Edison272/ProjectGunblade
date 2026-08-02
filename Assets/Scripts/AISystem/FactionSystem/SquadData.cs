using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SocialPlatforms.Impl;

namespace GameAI.Factions
{
    /// <summary> 
    /// Squads are an organizational unit within factions. They represent a collection of a faction's members
    /// 
    /// Squads are formed when characters are spawned in close proximity to each other, or by force joining
    /// Squads generally do things together when possible, so they share data (cheaper too)
    /// Squadless characters will join the nearest squad
    /// 
    /// 
    /// Each squad contains a "leader", which the other members of the squad will follow
    /// if the leader dies, another member will assume command
    /// </summary>
    public class Squad
    {
        public readonly FactionData Faction;
        public Character Leader {get; private set;}
        public readonly HashSet<Character> Members = new HashSet<Character>();
        public readonly SquadBlackboard SquadBoard;

        public Vector2 Position => Leader.TilePosition;
        
        public Squad(FactionData factionData)
        {
            Faction = factionData;
        }
        public Squad(FactionData factionData, Character[] newMembers)
        {
            Faction = factionData;
            if (newMembers.Length > 0)
            {
                Members = newMembers.ToHashSet();
                Leader = newMembers[0];
            }
        }

        public void AddMember(Character member)
        {
            if (!Members.Contains(member)) Members.Add(member);
        }
        public void RemoveMember(Character member)
        {
            if (Members.Contains(member)) Members.Remove(member);
            if (Members.Count == 0) Faction.RemoveSquad(this);

            else {
                if (Leader == null)
                {
                    Leader = Members.FirstOrDefault();
                }
            }
        }
    }

    // a version of the blackboard reserved for localized squad usage.
    public class SquadBlackboard
    {
        
    }
}