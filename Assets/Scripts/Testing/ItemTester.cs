using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class ItemTester : MonoBehaviour
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
    public Item item;
    public GameObject buttonInstance;
    private InputEventRelay inputRelay;
    void Awake()
    {
        if (!_playerInput) {_playerInput = GetComponent<PlayerInput>();}

        // map input actions to the controls
        //_inputMovement = _playerInput.actions["Move"];
        //_inputLookDelta = _playerInput.actions["Look"];
        _inputUseMain = _playerInput.actions["UseMain"];
        _inputUseAlt = _playerInput.actions["UseAlt"];
        _inputReset = _playerInput.actions["Reset"];
        //_inputInteract = _playerInput.actions["Interact"];

        // connect public events to input actions
        // _inputMovement.performed += ctx => {inputRelay.Invoke(InputEvent.Character_MoveStart, ctx.ReadValue<Vector2>());};
        // _inputMovement.canceled += ctx => {inputRelay.Invoke(InputEvent.Character_MoveEnd);};
        // _inputLookDelta.started += ctx => {SetLookPosition(ctx.ReadValue<Vector2>());};

        _inputUseMain.performed += ctx => {inputRelay.Invoke(InputEvent.Character_MainStart);};
        _inputUseMain.canceled += ctx => {inputRelay.Invoke(InputEvent.Character_MainEnd);};
        _inputUseAlt.performed += ctx => {inputRelay.Invoke(InputEvent.Character_AltStart);};
        _inputUseAlt.canceled += ctx => {inputRelay.Invoke(InputEvent.Character_AltEnd);};

        _inputReset.started += ctx => {inputRelay.Invoke(InputEvent.Usable_ResetStart);};
        //_inputInteract.started += ctx => {inputRelay.Invoke(InputEvent.Item_Reset);};    
    
        inputRelay = new InputEventRelay(
            new (InputEvent, Type)[] {
                (InputEvent.General_Passive, typeof(Action)), 
                (InputEvent.Character_MainStart, typeof(Action)), 
                (InputEvent.Character_MainEnd, typeof(Action)), 
                (InputEvent.Character_AltStart, typeof(Action)), 
                (InputEvent.Character_AltEnd, typeof(Action)), 
                (InputEvent.Character_LookPos, typeof(Action<Vector2>)), 
                (InputEvent.Usable_ResetStart, typeof(Action)), 
            }
        );
        // for(int i = 0; i < (int)InputEvent.Size; i++)
        // {
        //     InputEvent input_event = (InputEvent)i;
        //     if (inputRelay.IsConnected(input_event) && input_event != InputEvent.General_Passive && input_event != InputEvent.Character_LookPos)
        //     {
        //         Button input_button = Instantiate(buttonInstance, this.transform).GetComponent<Button>();
        //         TextMeshProUGUI button_text =  input_button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
         
        //         button_text.text = (input_event).ToString();
        //         input_button.onClick.AddListener(() => inputRelay.Invoke(input_event));   
        //     }
        // }
        buttonInstance.SetActive(false);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        item.NewUser(inputRelay, GetAttackTarget);
        item.SetEquipped(true);
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 mouseScreenPos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        inputRelay.Invoke(InputEvent.Character_LookPos, mouseScreenPos);

        inputRelay.Invoke(InputEvent.General_Passive);
    }
    public AttackTarget GetAttackTarget()
    {
        return new AttackTarget(item.transform.position, Vector2.zero, Vector2.zero, Vector2.zero);
    }
    public void ToggleEquip(bool is_equipped)
    {

        item.SetEquipped(is_equipped);
    }

    public void TogglePickedup(bool is_pickedup)
    {
        InputEventRelay new_relay = is_pickedup? inputRelay : null;
        item.NewUser(new_relay, GetAttackTarget);
    }
}
