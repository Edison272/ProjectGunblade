using UnityEngine;
using System;
using System.Collections.Generic;

namespace GameAI.Factions
{
    /// <summary>
    /// initializer data for the Faction Manager.
    /// Declare what factions exist by default, who's on they're team and where theyre dropped
    /// </summary>

    [Serializable]
    public struct FactionInitializer
    {
        public string FactionName;
        public List<CharacterSpawner> DefaultMembers;

    }
}