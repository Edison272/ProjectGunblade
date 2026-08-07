using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

using Random = UnityEngine.Random; // LEAVE ME ALONE DAMNIT

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }
    [field: SerializeField] public Tilemap Floor { get; private set; }
    [field: SerializeField] public Tilemap Wall { get; private set; }

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
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
