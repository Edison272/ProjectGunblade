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
            new Dictionary<InputEvent, Type>{
                {InputEvent.MainStart, typeof(Action)}, 
                {InputEvent.MainEnd, typeof(Action)}, 
                {InputEvent.AltStart, typeof(Action)}, 
                {InputEvent.AltEnd, typeof(Action)}, 
                {InputEvent.Reset, typeof(Action)}, 
            }
        );

        for(int i = 0; i < (int)InputEvent.Size; i++)
        {
            InputEvent input_event = (InputEvent)i;
            if (inputRelay.IsConnected(input_event))
            {
                Button input_button = Instantiate(buttonInstance, this.transform).GetComponent<Button>();
                TextMeshProUGUI button_text =  input_button.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
         
                button_text.text = (input_event).ToString();
                input_button.onClick.AddListener(() => inputRelay.Invoke(input_event));   
            }
        }
        buttonInstance.SetActive(false);
        item.NewUser(inputRelay);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
