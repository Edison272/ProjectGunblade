using System;
using System.Collections;
using System.Collections.Generic;
using GameAI.Factions;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


// is generally a manager class used to convert inputs into in-game actions via the player character
// when switched to command mode, this becomes the body of the camera
public class PlayerController : MonoBehaviour
{    
    // Input Map
    private PlayerInput _playerInput;

    // Input Actions - direct input information from input map
    private InputAction _inputMovement;
    private InputAction _inputLookDelta;
    private InputAction _inputUseMain;
    private InputAction _inputUseAlt;
    private InputAction _inputReset;
    private InputAction _inputInteract;
    private InputAction _inputScroll;
    private InputAction _inputCommanding;

    // Input Relay!
    public InputEventRelay CharacterInputRelay;
    public InputEventRelay CommandInputRelay;

    // Camera Control
    // [SerializeField] CameraController main_cam_controller;
    [SerializeField] CinemachineCamera main_cinema_cam;
    [SerializeField] Camera main_cam;
    private CameraController _cameraController;

    // Pointer/Mouse Control
    [SerializeField] PointerController _pointerController;
    private Vector2 pointer_world_pos = Vector2.zero; // access where the player's pointer is in the game world
    private Vector2 pointer_viewport_pos = Vector2.zero; // access where the player's pointer is in the UI

    // temporary. testing for player input
    public Character active_character;

    // Faction Mechanic Controller
    private bool _isCommanding = false;
    private FactionData _playerFaction;
    private Squad _playerSquad => active_character.CharacterSquad;
    private int _squadSelector = 0;
    private Vector2 cmd_view_move_dir; // used to move camera when scoping map

    void Awake()
    {
        #region Awake - Inputs
        if (!_playerInput) {_playerInput = GetComponent<PlayerInput>();}

        // map input actions to the controls
        _inputMovement = _playerInput.actions["Move"];
        _inputLookDelta = _playerInput.actions["Look"];
        _inputUseMain = _playerInput.actions["UseMain"];
        _inputUseAlt = _playerInput.actions["UseAlt"];
        _inputReset = _playerInput.actions["Reset"];
        _inputInteract = _playerInput.actions["Interact"];
        _inputScroll = _playerInput.actions["Scroll"];
        _inputCommanding = _playerInput.actions["ToggleCommandMode"];


        // connect public events to input actions
        _inputMovement.performed += ctx => {CharacterInputRelay.Invoke(CharacterEvent.MoveStart, ctx.ReadValue<Vector2>());};
        _inputMovement.canceled += ctx => {CharacterInputRelay.Invoke(CharacterEvent.MoveEnd);};
        _inputLookDelta.started += ctx => {SetLookPosition(ctx.ReadValue<Vector2>());};


        _inputUseMain.performed += ctx => {CharacterInputRelay.Invoke(CharacterEvent.MainStart);};
        _inputUseMain.canceled += ctx => {CharacterInputRelay.Invoke(CharacterEvent.MainEnd);};
        _inputUseAlt.performed += ctx => {CharacterInputRelay.Invoke(CharacterEvent.AltStart);};
        _inputUseAlt.canceled += ctx => {CharacterInputRelay.Invoke(CharacterEvent.AltEnd);};
        

        _inputReset.started += ctx => {CharacterInputRelay.Invoke(UsableEvent.ResetStart);};
        _inputInteract.started += ctx => {CharacterInputRelay.Invoke(CharacterEvent.Interact);};
        _inputInteract.started += ctx => {CommandInputRelay.Invoke(CharacterEvent.Interact);};

        _inputScroll.performed += ctx => {InputScroll(ctx.ReadValue<Vector2>());};

        // Command-related inputs
        _inputUseMain.performed += ctx => {CommandInputRelay.Invoke(CharacterEvent.MainStart);};
        _inputCommanding.performed += ctx => {SetCommandMode(!_isCommanding);};

        // Setup Input Relays
        CharacterInputRelay = new InputEventRelay(
            new (Enum, Type)[] {
                (GlobalEvent.Update, typeof(Action)),
                
                //(InterfaceEvent.Scroll, typeof(Action<Vector2>)),

                (CharacterEvent.MoveStart, typeof(Action<Vector2>)),
                (CharacterEvent.MoveEnd, typeof(Action)),
                (CharacterEvent.MainStart, typeof(Action)), 
                (CharacterEvent.MainUpdate, typeof(Action)), 
                (CharacterEvent.MainEnd, typeof(Action)), 
                (CharacterEvent.AltStart, typeof(Action)), 
                (CharacterEvent.AltUpdate, typeof(Action)), 
                (CharacterEvent.AltEnd, typeof(Action)), 
                (CharacterEvent.LookPos, typeof(Action<Vector2>)), 
                (CharacterEvent.Interact, typeof(Action)), 
                
                (UsableEvent.ResetStart, typeof(Action)), 
            }
        );
        CommandInputRelay = new InputEventRelay(
            new (Enum, Type)[] {
                (InterfaceEvent.Scroll, typeof(Action<Vector2>)),

                (CharacterEvent.MoveStart, typeof(Action<Vector2>)),
                (CharacterEvent.MoveEnd, typeof(Action)),
                (CharacterEvent.MainStart, typeof(Action)), 
                (CharacterEvent.AltStart, typeof(Action)), 
                (CharacterEvent.LookPos, typeof(Action<Vector2>)), 
                (CharacterEvent.Interact, typeof(Action))
            }
        );


        _inputMovement.performed += ctx => {cmd_view_move_dir = ctx.ReadValue<Vector2>();};
        _inputMovement.canceled += ctx => {cmd_view_move_dir = Vector2.zero;};

        CommandInputRelay.ConnectEvent(CharacterEvent.MainStart,  (Action)(() => {SelectMapPosition(pointer_world_pos);}));
        CommandInputRelay.ConnectEvent(CharacterEvent.Interact,  (Action)(() => {CycleSquads();}));

        #endregion

        #region Awake - Pointer
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _pointerController = new PointerController(main_cam);
        //_cameraController = new CameraController();
        #endregion
    }

    void Start()
    {
        SetPlayerCharacter(active_character);

        // setup faction control
        _playerFaction = FactionManager.Instance.RegisterFaction(active_character.FactionTag);
        FactionInspector.SetSquadSelection(_squadSelector);
        // setup UI components
        FactionInspector.SetFaction(_playerFaction);
        SetCommandMode(false);  



        // setup control UI
    }



    // update the accel values on input

    // get player control
    void SetPlayerCharacter(Character set_character)
    {
        //active_character?.ConnectPlayer(this);
        set_character?.LinkController(CharacterInputRelay);
        // set camera target
        main_cinema_cam.Target.TrackingTarget = set_character.transform;
    }

    void Update()
    {        
        pointer_world_pos = _pointerController.GetSourceTo_worldPos(active_character.Position);
        CharacterInputRelay.Invoke(CharacterEvent.LookPos, pointer_world_pos);   
        Debug.DrawLine(active_character.Position, pointer_world_pos);

        if (_isCommanding && cmd_view_move_dir != Vector2.zero)
        {
            transform.position += (Vector3)cmd_view_move_dir * Time.deltaTime * 10f;
        }

        CharacterInputRelay.Invoke(GlobalEvent.Update);   
    }
    private void SetLookPosition(Vector2 lookDelta)
    {
        _pointerController.UpdateDelta(lookDelta, active_character.Position);
    }

    #region UI Interaction
    public void InputScroll(Vector2 scrollVec)
    {
        float deltaScroll = scrollVec.y * Time.deltaTime;
        // Debug.Log(deltaScroll);
    }



    void SetCommandMode(bool mode)
    {
        _isCommanding = mode;


        this.transform.position = active_character.Position;
        main_cinema_cam.Target.TrackingTarget = _isCommanding ? transform : active_character.transform;
        CharacterInputRelay.Invoke(CharacterEvent.MoveEnd);
        
        // stop anything the player mightve been doing
        if (_isCommanding)
        {
            CharacterInputRelay.Invoke(CharacterEvent.MainEnd);
            CharacterInputRelay.Invoke(CharacterEvent.AltEnd);
        }
        CharacterInputRelay.IsActive = !_isCommanding;
        
        // enable 
        CommandInputRelay.IsActive = _isCommanding;
        FactionInspector.SetActive(_isCommanding);

        
    }

    public void CycleSquads(int force_index = -1)
    {
        if (force_index > 0)
            _squadSelector = force_index;
        else
            _squadSelector += 1;
        _squadSelector = _squadSelector % _playerFaction.Squads.Count;
        // skip player squad
        if (_playerFaction.Squads[_squadSelector] == _playerSquad)
        {
            _squadSelector += 1;
            _squadSelector = _squadSelector % _playerFaction.Squads.Count;
        }

        FactionInspector.SetSquadSelection(_squadSelector);
    }

    // used while commanding to select a location on the map to direct people
    public void SelectMapPosition(Vector2 selectPos)
    {
        Squad targetSquad = _playerFaction.Squads[_squadSelector];
        foreach(Character member in targetSquad.Members)
        {
            if (member == active_character)
                continue;
                
            member._behaviorController.SetTargetMovePos(selectPos);
        }
    }

    #endregion
}
