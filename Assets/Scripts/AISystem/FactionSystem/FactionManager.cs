using System.Collections.Generic;
using UnityEngine;

namespace GameAI.Factions
{
    /// <summary>
    /// Global singleton that owns every active FactionData instance in the game.
    /// NPCs look up their faction here rather than holding a direct reference to
    /// each other, so any NPC can ask "what's my faction's relation to the player's
    /// faction" or "what's the last position my faction reported" without needing
    /// to know about any specific other agent.
    /// </summary>
    public class FactionManager : MonoBehaviour
    {
        public static FactionManager Instance { get; private set; }

        public readonly Dictionary<string, FactionData> Factions = new Dictionary<string, FactionData>();

        [Header("Factions to create at startup")]
        // [Tooltip("Just the names — set up relations in code via SetRelation, or extend this with a ScriptableObject config if you want it designer-editable.")]
        public List<FactionInitializer> initialFactions = new List<FactionInitializer>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            foreach (FactionInitializer faction in initialFactions)
            {
                Factions[faction.FactionName] = new FactionData(faction);
            }
        }

        /// <summary>Creates a new faction if one with this name doesn't already exist. Safe to call multiple times.</summary>
        public FactionData RegisterFaction(string factionName)
        {
            if (Factions.TryGetValue(factionName, out var existing))
                return existing;
            var faction = new FactionData(factionName);
            Factions[factionName] = faction;
            return faction;
        }

        public FactionData GetFaction(string factionName)
        {
            if (Factions.TryGetValue(factionName, out var faction))
                return faction;

            Debug.LogWarning($"FactionManager: no faction registered with name '{factionName}'.");
            return null;
        }

        public bool TryGetFaction(string factionName, out FactionData faction)
        {
            return Factions.TryGetValue(factionName, out faction);
        }

        public IReadOnlyCollection<string> AllFactionNames => Factions.Keys;
    }
}