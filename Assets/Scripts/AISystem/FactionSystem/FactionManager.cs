using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEditor;

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
        const string PrefabPath = "Assets/Prefabs/Managers/FactionManager.prefab";
        private static FactionManager _instance;
        public static FactionManager Instance { 
            get
            {
                if (_instance == null)
                {
                    // Check for an existing scene instance first
                    _instance = FindFirstObjectByType<FactionManager>();
                    if (_instance != null)
                        return _instance;
                    FactionManager prefab = Resources.Load<FactionManager>("Prefabs/Managers/FactionManager");
                    _instance = Instantiate(prefab.gameObject).GetComponent<FactionManager>();
                    DontDestroyOnLoad(_instance);
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }
        public readonly List<FactionData> Factions = new List<FactionData>();

        [Header("Factions to create at startup")]
        // [Tooltip("Just the names — set up relations in code via SetRelation, or extend this with a ScriptableObject config if you want it designer-editable.")]
        public List<FactionInitializer> initialFactions = new List<FactionInitializer>();

        // Builtin Factions. Only three factions get their own physics layer
        public const int PlayerFactionID = 0;
        public const int PlayerFactionLayer = 29;
        public const int EnemyFactionID = 1;
        public const int EnemyFactionLayer = 30;
        public const int NoFactionID = -1;
        public const int NoFactionLayer = 31;
        public const int FactionLayers = 1 << NoFactionLayer | 1 << EnemyFactionLayer | 1 << PlayerFactionLayer; // the factions that can be targetted (31 = NoFaction, 30 = Enemy Faction, 29 = Player Faction)
        

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            
            Factions.Add(new FactionData(PlayerFactionID, "Player", PlayerFactionLayer));
            Factions.Add(new FactionData(EnemyFactionID, "The Bad Guys", EnemyFactionLayer));
            // Add extra factions if they exist
            foreach (FactionInitializer faction in initialFactions)
            {
                Factions.Add(new FactionData(Factions.Count));
            }
        }

        /// <summary>Creates a new faction if one with this name doesn't already exist. Safe to call multiple times.</summary>
        public FactionData RegisterFaction(int FactionID, string FactionName = "Nameless")
        {
            FactionData faction = null;
            if (TryGetFaction(FactionID, out faction))
                return faction;
            
            faction = new FactionData(Factions.Count, FactionName, NoFactionLayer); // any factions are considered to be factionless
            Debug.Log(FactionID);
            Factions.Add(faction);
            return faction;
        }
        public bool TryGetFaction(int FactionID, out FactionData faction)
        {
            faction = null;
            if (FactionID >= 0 && FactionID < Factions.Count)
                faction = Factions[FactionID];
            return faction != null;
        }
    }
}