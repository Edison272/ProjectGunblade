using UnityEngine;
using UnityEngine.InputSystem;

public class CanvasController : MonoBehaviour
{
    public static CanvasController Instance { get; private set; }
    private static InputEventRelay _inputEvents;
    [field: SerializeField] public GameObject ControlUI {get; private set;}
    [field: SerializeField] public GameObject PlayerHUD {get; private set;}
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // setup the player's control over the canvas controller
    public static void SetupCanvasController(InputEventRelay playerEvents)
    {
        _inputEvents = playerEvents;
    }


    #region Control UI
    public static void SetupControlUI()
    {
        if (!Instance)
        {
            Debug.LogError("Singleton Instance of Canvas Controller NOT FOUND");
            return;
        }
        else if (!Instance.ControlUI)
        {
            Debug.LogError("ControlUI object NOT FOUND");
            return;
        }


    }
    public static void SetControlUI(bool isActive)
    {
        if (!Instance)
        {
            Debug.LogError("Singleton Instance of Canvas Controller NOT FOUND");
            return;
        }
        else if (!Instance.ControlUI)
        {
            Debug.LogError("ControlUI object NOT FOUND");
            return;
        }

        Instance.ControlUI.SetActive(isActive);
    }

    #endregion
}