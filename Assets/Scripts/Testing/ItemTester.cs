using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class ItemTester : MonoBehaviour
{
    public Item item;
    public GameObject buttonInstance;
    private InputEventRelay inputRelay;
    void Awake()
    {
        inputRelay = new InputEventRelay(
            new (InputEvent, Type)[] {
                (InputEvent.General_Passive, typeof(Action)), 
                (InputEvent.Character_MainStart, typeof(Action)), 
                (InputEvent.Character_MainEnd, typeof(Action)), 
                (InputEvent.Character_AltStart, typeof(Action)), 
                (InputEvent.Character_AltEnd, typeof(Action)), 
                (InputEvent.Usable_ResetStart, typeof(Action)), 
            }
        );

        for(int i = 0; i < (int)InputEvent.Size; i++)
        {
            InputEvent input_event = (InputEvent)i;
            if (inputRelay.IsConnected(input_event) && input_event != InputEvent.General_Passive)
            {
                Button input_button = Instantiate(buttonInstance, this.transform).GetComponent<Button>();
                TextMeshProUGUI button_text =  input_button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
         
                button_text.text = (input_event).ToString();
                input_button.onClick.AddListener(() => inputRelay.Invoke(input_event));   
            }
        }
        buttonInstance.SetActive(false);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        item.NewUser(inputRelay);
    }

    // Update is called once per frame
    void Update()
    {
        inputRelay.Invoke(InputEvent.General_Passive);
    }
    public void ToggleEquip(bool is_equipped)
    {

        item.SetEquipped(is_equipped);
    }

    public void TogglePickedup(bool is_pickedup)
    {
        InputEventRelay new_relay = is_pickedup? inputRelay : null;
        item.NewUser(new_relay);
    }
}
