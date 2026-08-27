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

        public Vector2 Position => Leader.Position;
        public Vector2 AnchorPosition; // The general position that the squad forms around
        
        public Squad(FactionData factionData)
        {
            Faction = factionData;
        }
        public Squad(FactionData factionData, Character[] newMembers)
        {
            Faction = factionData;
            if (newMembers.Length > 0)
            {
                Leader = newMembers[0];
                foreach(Character member in newMembers)
                {
                    Members.Add(member);
                    member.SetFactionTag(Faction.FactionName);
                }
            }
        }
        #region Membership
        public void AddMember(Character member)
        {
            if (!Members.Contains(member)) 
            {
                if (!Leader) Leader = member;
                Members.Add(member);
                member.SetFactionTag(Faction.FactionName);
            }
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
        #endregion

        #region Finding Target

        public Character FindTarget(Character thisChar, bool targetAlly, TargetType targetType)
        {
            return targetAlly ? FindAlly(thisChar, targetType) : FindEnemy(thisChar, targetType);
        }

        public Character FindAlly(Character thisChar, TargetType targetType)
        {
            float highScore = -Mathf.Infinity;
            Character primeTarget = null;
            Debug.Log($"{Faction.Squads.Count}");
            foreach(Squad squad in Faction.Squads)
            {
                Debug.Log($"{squad.Members.Count}");
                foreach(Character character in squad.Members)
                {
                    if (character == thisChar || character == null)
                    {
                        continue;
                    }
                    float score = AssessTarget(thisChar, character, targetType);
                    if (score > highScore)
                    {
                        primeTarget = character;
                        highScore = score;
                        
                    }
                }
            }
            return primeTarget;
        }

        public Character FindEnemy(Character thisChar, TargetType targetType)
        {
            float highScore = -Mathf.Infinity;
            Character primeTarget = null;
            foreach(FactionData faction in FactionManager.Instance.Factions.Values)
            {
                if (faction == Faction)
                {
                    continue;
                }
                foreach(Squad squad in faction.Squads)
                {
                    foreach(Character character in squad.Members)
                    {
                        if (character == null)
                        {
                            continue;
                        }
                        float score = AssessTarget(thisChar, character, targetType);
                        if (score > highScore)
                        {
                            primeTarget = character;
                            highScore = score;
                            
                        }
                    }
                }
            }
            return primeTarget;
        }

        public float AssessTarget(Character currChar, Character targetChar, TargetType targetType)
        {
            switch (targetType)
            {
                case TargetType.Closest:
                    return GetNearestScore(currChar, targetChar);
                case TargetType.Furthest:
                    return GetFurthestScore(currChar, targetChar);
                case TargetType.MostHP:
                    return GetNearestScore(currChar, targetChar);
                case TargetType.LeastHP:
                    return GetNearestScore(currChar, targetChar);
                default:
                    return GetNearestScore(currChar, targetChar);
            }
        }
        private float GetNearestScore(Character curr_character, Character target)
        {
            float score = 1/(curr_character.GetPosition() - target.GetPosition()).sqrMagnitude + 0.001f;
            return score;
        }

        private float GetFurthestScore(Character curr_character, Character target)
        {
            float score = (curr_character.GetPosition() - target.GetPosition()).sqrMagnitude;
            if (score > curr_character.curr_range)
            {
                score = -1;
            }
            return score;
        }
        #endregion
    }
    // a version of the blackboard reserved for localized squad usage.
    public class SquadBlackboard
    {
        
    }
}