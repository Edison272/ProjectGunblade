using UnityEngine;
using UnityEngine.InputSystem;
using GameAI.Factions;
using System.Collections.Generic;
using Unity.VisualScripting;


// The LHS UI element displaying all active squads of a faction
public class FactionInspector : MonoBehaviour
{
    public static FactionInspector Instance { get; private set; }
    public FactionData _factionData;
    private readonly List<GameObject> _squadDataUI = new List<GameObject>();
    private int _squadSelector = 0;

    // Initialize 8 squads by default. make more if necessary
    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        GameObject squadDataUI = Resources.Load<GameObject>("SquadDataUI");
        Debug.Log(squadDataUI)
;        for (int i = 0; i < 16; i++)
        {
            GameObject instance = Instantiate(squadDataUI, transform);
            instance.SetActive(false);
            _squadDataUI.Add(instance);
        }
    }
    public static void SetFaction(FactionData newFaction)
    {
        if (!Instance)
        {
            Debug.LogError("Singleton Instance of FactionInspector NOT FOUND");
            return;
        }

        Instance._factionData = newFaction;
        Debug.LogWarning(Instance._factionData.Squads.Count);
        for (int i = 0; i < Instance._factionData.Squads.Count; i++)
        {
            Debug.LogWarning("hai");
            Instance._squadDataUI[i].SetActive(true);
        }
    }

    public static void SetActive(bool isActive)
    {
        if (!Instance)
        {
            Debug.LogError("Singleton Instance of FactionInspector NOT FOUND");
            return;
        }
        Instance.gameObject.SetActive(isActive);
        Debug.LogWarning($"{Instance._factionData.FactionName}, {Instance._factionData.Squads}");
        for (int i = 0; i < Instance._factionData.Squads.Count; i++)
        {
            Instance._squadDataUI[i].SetActive(isActive);
        }
    }

    public static void SetSquadSelection(int index)
    {
        if (!Instance)
        {
            Debug.LogError("Singleton Instance of FactionInspector NOT FOUND");
        }
        Instance.InternalCycleSquadSelection(index);
    }
    private void InternalCycleSquadSelection(int index)
    {
        _squadDataUI[_squadSelector].transform.localScale = new Vector3(1,1,1);
        _squadSelector = index;
        _squadDataUI[_squadSelector].transform.localScale = new Vector3(1.3f,1.3f,1.3f);
    }
}