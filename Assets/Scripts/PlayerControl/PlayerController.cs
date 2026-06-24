using System;
using System.Collections;
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

    // Publically accessible events
    public event Action<Vector2> OnMoveStart; // called when the player goes from not moving to moving
    public event Action OnMoveEnd; // called when player stops moving

    public event Action OnMainActionStart;
    public event Action OnMainActionEnd;
    public event Action OnAltActionStart;
    public event Action OnAltActionEnd;
    public event Action OnReset;
    public event Action OnInteract;
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
        input_movement.performed += ctx => {OnMoveStart?.Invoke(ctx.ReadValue<Vector2>());};
        input_movement.canceled += ctx => {OnMoveEnd?.Invoke();};

        input_use_main.performed += ctx => {OnMainActionStart?.Invoke();};
        input_use_main.canceled += ctx => {OnMainActionEnd?.Invoke();};
        input_use_alt.performed += ctx => {OnAltActionStart?.Invoke();};
        input_use_alt.canceled += ctx => {OnAltActionEnd?.Invoke();};

        input_reset.started += ctx => {OnReset?.Invoke();};
        input_interact.started += ctx => {OnInteract?.Invoke();};
    }

    void Start()
    {
        SetPlayerCharacter(active_character);
    }

    // update the accel values on input

    // get player control
    void SetPlayerCharacter(Character set_character)
    {
        active_character?.ConnectPlayer(this);

        // set camera target
        main_cinema_cam.Target.TrackingTarget = active_character.transform;
    }

    void Update()
    {
        Vector2 mouseScreenPos = main_cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        
        active_character.Look(mouseScreenPos);
    }
}
