using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/*
Display the stats of a stack counter using bars
*/
public class IntCounterUI : StackCounterUI
{
    [field: Header("UI Elements")]
    public GameObject CounterObjectPrefab;
    public RectTransform CounterLayout;
    private List<GameObject> _activeCounters = new List<GameObject>();
    private float currentStatus = -2; // use any number that isn't 0-1 or -1
    public override StackCounter StackCounter 
    {
        get {return _stackCounter;} 
        set
        {
            _stackCounter = value;

            for (int i = 0; i < Mathf.Max(_activeCounters.Count, (int)_stackCounter.GetMaxValue()); i++)
            {
                if (_activeCounters.Count < _stackCounter.GetMaxValue())
                {
                    GameObject newObj = Instantiate(CounterObjectPrefab, CounterLayout);
                    newObj.SetActive(true);
                    _activeCounters.Add(newObj);
                }
                _activeCounters[i].SetActive(i < _stackCounter.GetMaxValue());
            }
            currentStatus = -2;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float clampStatus = Mathf.Clamp01(_stackCounter.GetStatus());
        if (currentStatus != clampStatus)
        {
            for (int i = 0; i < _activeCounters.Count; i++)
            {
                _activeCounters[i].transform.GetChild(0).gameObject.SetActive(i < Mathf.Round(clampStatus * _stackCounter.GetMaxValue()));
            }
            currentStatus = clampStatus;
        }
    }
}
