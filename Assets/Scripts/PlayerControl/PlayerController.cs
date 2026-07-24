using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


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

    // Input Relay!
    public InputEventRelay inputRelay;

    // Camera Control
    // [SerializeField] CameraController main_cam_controller;
    [SerializeField] CinemachineCamera main_cinema_cam;
    [SerializeField] Camera main_cam;

    // Pointer/Mouse Control
    [SerializeField] PointerController _pointerController;
    private Vector2 pointer_world_pos = Vector2.zero; // access where the player's pointer is in the game world
    private Vector2 pointer_viewport_pos = Vector2.zero; // access where the player's pointer is in the UI

    // temporary. testing for player input
    public Character active_character;

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


        // connect public events to input actions
        _inputMovement.performed += ctx => {inputRelay.Invoke(CharacterEvent.MoveStart, ctx.ReadValue<Vector2>());};
        _inputMovement.canceled += ctx => {inputRelay.Invoke(CharacterEvent.MoveEnd);};
        _inputLookDelta.started += ctx => {SetLookPosition(ctx.ReadValue<Vector2>());};

        _inputUseMain.performed += ctx => {inputRelay.Invoke(CharacterEvent.MainStart);};
        _inputUseMain.canceled += ctx => {inputRelay.Invoke(CharacterEvent.MainEnd);};
        _inputUseAlt.performed += ctx => {inputRelay.Invoke(CharacterEvent.AltStart);};
        _inputUseAlt.canceled += ctx => {inputRelay.Invoke(CharacterEvent.AltEnd);};

        _inputReset.started += ctx => {inputRelay.Invoke(UsableEvent.ResetStart);};
        _inputInteract.started += ctx => {inputRelay.Invoke(CharacterEvent.Interact);};
        //_inputInteract.started += ctx => {inputRelay.Invoke(InputEvent.Item_Reset);};

        // Setup Input Relay
        inputRelay = new InputEventRelay(
            new (Enum, Type)[] {
                (GlobalEvent.Update, typeof(Action)),
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
        #endregion
        #region Awake - Pointer
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _pointerController = new PointerController(main_cam);
        #endregion
    }

    void Start()
    {
        SetPlayerCharacter(active_character);
    }

    // update the accel values on input

    // get player control
    void SetPlayerCharacter(Character set_character)
    {
        //active_character?.ConnectPlayer(this);
        active_character?.LinkController(inputRelay);

        // set camera target
        main_cinema_cam.Target.TrackingTarget = active_character.transform;
    }

    void Update()
    {        
        pointer_world_pos = _pointerController.GetSourceTo_worldPos(active_character.Position);
        inputRelay.Invoke(CharacterEvent.LookPos, pointer_world_pos);   
        Debug.DrawLine(active_character.Position, pointer_world_pos);

        inputRelay.Invoke(GlobalEvent.Update);   
    }
    private void SetLookPosition(Vector2 lookDelta)
    {
        _pointerController.UpdateDelta(lookDelta, active_character.Position);

    }
}
