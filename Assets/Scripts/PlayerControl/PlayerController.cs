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
    private PlayerInput player_input;

    // Input Actions - direct input information from input map
    private InputAction input_movement;
    private InputAction input_use_main;
    private InputAction input_use_alt;
    private InputAction input_reset;
    private InputAction input_interact;

    // Input Relay!
    public InputEventRelay inputRelay;

    // Camera Control
    [SerializeField] CinemachineCamera main_cinema_cam;
    [SerializeField] Camera main_cam;



    // temporary. testing for player input
    public Character active_character;

    void Awake()
    {
        if (!player_input) {player_input = GetComponent<PlayerInput>();}


        // map input actions to the controls
        input_movement = player_input.actions["Move"];
        input_use_main = player_input.actions["UseMain"];
        input_use_alt = player_input.actions["UseAlt"];
        input_reset = player_input.actions["Reset"];
        input_interact = player_input.actions["Interact"];


        // connect public events to input actions
        input_movement.performed += ctx => {inputRelay.Invoke(InputEvent.Character_MoveStart, ctx.ReadValue<Vector2>());};
        input_movement.canceled += ctx => {inputRelay.Invoke(InputEvent.Character_MoveEnd);};

        input_use_main.performed += ctx => {inputRelay.Invoke(InputEvent.Character_MainStart);};
        input_use_main.canceled += ctx => {inputRelay.Invoke(InputEvent.Character_MainEnd);};
        input_use_alt.performed += ctx => {inputRelay.Invoke(InputEvent.Character_AltStart);};
        input_use_alt.canceled += ctx => {inputRelay.Invoke(InputEvent.Character_AltEnd);};

        input_reset.started += ctx => {inputRelay.Invoke(InputEvent.Usable_Reset);};
        //input_interact.started += ctx => {inputRelay.Invoke(InputEvent.Item_Reset);};

        // Setup Input Relay
        inputRelay = new InputEventRelay(
            new (Enum, Type)[] {
                (InputEvent.General_Passive, typeof(Action)),
                (InputEvent.Character_MoveStart, typeof(Action<Vector2>)),
                (InputEvent.Character_MoveEnd, typeof(Action)),
                (InputEvent.Character_MainStart, typeof(Action)), 
                (InputEvent.Character_MainEnd, typeof(Action)), 
                (InputEvent.Character_AltStart, typeof(Action)), 
                (InputEvent.Character_AltEnd, typeof(Action)), 
                (InputEvent.Usable_Reset, typeof(Action)), 
            }
        );
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
        active_character?.ConnectInputs(inputRelay);

        // set camera target
        main_cinema_cam.Target.TrackingTarget = active_character.transform;
    }

    void Update()
    {
        Vector2 mouseScreenPos = main_cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        
        active_character.Look(mouseScreenPos);
    }
}
