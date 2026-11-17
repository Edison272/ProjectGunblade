using UnityEngine;
using UnityEngine.InputSystem;
using GameAI.Factions;
using System.Collections.Generic;
using Unity.VisualScripting;


// The LHS UI element displaying all active squads of a faction
public class FactionInspector : MonoBehaviour
{
    private static FactionInspector _instance;
    public static FactionInspector Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<FactionInspector>();
                if (_instance != null)
                    return _instance;
                FactionInspector instance = Resources.Load<FactionInspector>("Prefabs/UI/Canvas/FactionInspector");
                _instance = Instantiate(instance).GetComponent<FactionInspector>();
                DontDestroyOnLoad(_instance);
            }
            return _instance;
        }
        private set { _instance = value; }
    }
    public FactionData InspectFaction;
    private readonly List<CharacterStatusUI> _squadDataUI = new List<CharacterStatusUI>();
    private int _squadSelector = 0;
    private int _activeUICount = 0; // how many ui elements are actually in use. Used to update the ui whenever faction data is changed


    [SerializeField] private GameObject _squadLayout;
    [SerializeField] private GameObject _inspectorCursor;

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
        
        GameObject squadDataUI = Resources.Load<GameObject>("Prefabs/UI/Components/HUD/CharacterStatus");
        Debug.Log(squadDataUI)
;        for (int i = 0; i < 16; i++)
        {
            GameObject instance = Instantiate(squadDataUI, _squadLayout.transform);
            instance.SetActive(false);
            _squadDataUI.Add(instance.GetComponent<CharacterStatusUI>());
        }
    }
    void Start()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }
    public static void SetFaction(FactionData newFaction)
    {
        if (!Instance)
        {
            Debug.LogError("Singleton Instance of FactionInspector NOT FOUND");
            return;
        }

        Instance.InspectFaction = newFaction;
        Instance.UpdateUI();
    }

    void Update()
    {
        if (Instance && Instance.InspectFaction.Squads.Count != _activeUICount)
        {
            UpdateUI();
        }
    }
    public void UpdateUI()
    {
        for (int i = 0; i < Instance.InspectFaction.Squads.Count; i++)
        {
            Instance._squadDataUI[i].gameObject.SetActive(true);
            Instance._squadDataUI[i].SetActiveCharacter(Instance.InspectFaction.Squads[i].Leader);
        }
        _activeUICount = Instance.InspectFaction.Squads.Count;
    }

    public static void SetActive(bool isActive)
    {
        if (!Instance)
        {
            Debug.LogError("Singleton Instance of FactionInspector NOT FOUND");
            return;
        }
        Instance.gameObject.SetActive(isActive);
        Debug.LogWarning($"{Instance.InspectFaction.FactionName}, {Instance.InspectFaction.Squads}");
        for (int i = 0; i < Instance.InspectFaction.Squads.Count; i++)
        {
            Instance._squadDataUI[i].gameObject.SetActive(isActive);
        }
    }
    #region Cursor
    public static void SetCursorPosition(Vector2 pos)
    {
        Instance._inspectorCursor.transform.position = pos;
    }

    #endregion

    #region Choosing Squad
    public static void SetSquadSelection(int index = -1)
    {
        if (!Instance)
        {
            Debug.LogError("Singleton Instance of FactionInspector NOT FOUND");
            return;
        }
        if (index == -1)
            Instance.CycleSquadSelection();
    }
    private void CycleSquadSelection()
    {
        _squadDataUI[_squadSelector].ToggleSelection(false);

        _squadSelector = (_squadSelector + 1) % InspectFaction.Squads.Count;
        // // skip player squad
        // if (InspectFaction.Squads[_squadSelector] == _playerSquad)
        // {
        //     _squadSelector += 1;
        //     _squadSelector = _squadSelector % InspectFaction.Squads.Count;
        // }
        _squadDataUI[_squadSelector].ToggleSelection(true);
    }
    #endregion

    #region Directing Squad
    public static void SetAnchor(Vector3 worldPos)
    {
        if (!Instance)
        {
            Debug.LogError("Singleton Instance of FactionInspector NOT FOUND");
            return;
        }
        Squad targetSquad = Instance.InspectFaction.Squads[Instance._squadSelector];
        foreach(Character member in targetSquad.Members)
        {
            member._behaviorController.SetTargetMovePos(worldPos);
        }
    }
    public static void UseAbility(Vector3 worldPos)
    {
        if (!Instance)
        {
            Debug.LogError("Singleton Instance of FactionInspector NOT FOUND");
            return;
        }
        Squad targetSquad = Instance.InspectFaction.Squads[Instance._squadSelector];
        foreach(Character member in targetSquad.Members)
        {
            member._behaviorController.SetTargetMovePos(worldPos);
        }
    }
    #endregion
}