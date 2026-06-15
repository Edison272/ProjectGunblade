using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class PlayerController : MonoBehaviour
{    
    // Input Map
    public PlayerInput player_input {get; private set;}

    // Input Actions - direct input information from input map
    public InputAction player_movement {get; private set;}
    public InputAction player_use_main {get; private set;}
    public InputAction player_use_alt {get; private set;}
    public InputAction player_interact {get; private set;}


    // temporary. testing for player input
    public Character active_character;

    void Awake()
    {
        if (!player_input) {player_input = GetComponent<PlayerInput>();}

        // map input actions to the controls
        player_movement = player_input.actions["Move"];
        player_use_main = player_input.actions["UseMain"];
        player_use_alt = player_input.actions["UseAlt"];
        player_interact = player_input.actions["Interact"];


    }

    void Start()
    {
        active_character?.ConnectPlayer(this);
    }

    // update the accel values on input
    void OnMoveStart(InputAction.CallbackContext context)
    {
        Debug.Log("vroom vroom!");
    }

    void OnMoveUpdate(InputAction.CallbackContext context)
    {
        Debug.Log("moving!");
    }

    void OnMoveEnd(InputAction.CallbackContext context)
    {
        Debug.Log("skrrrryyy!");
    }

    void OnInteract(InputAction.CallbackContext context)
    {
    }
}
