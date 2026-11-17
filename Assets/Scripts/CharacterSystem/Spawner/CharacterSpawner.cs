using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using GameAI.Factions;


// will spawn everything in the SpawnCharacters Array. 
// distributes spawning locations to children locations or self
public class CharacterSpawner : MonoBehaviour
{
    public int FactionID = FactionManager.EnemyFactionID;
    public CharacterSO[] SpawnCharacters;
    public float SpawnSpeed = 1;

    public void Start()
    {
        //StartSpawn();
        InvokeRepeating("StartSpawn", 0, SpawnSpeed);
    }
    public Character[] StartSpawn()
    {
        Transform[] spawnLocations = null;
        if (transform.childCount > 0)
        {
            spawnLocations = new Transform[transform.childCount];
            for (int i = 0; i < transform.childCount; i++)
            {
                spawnLocations[i] = transform.GetChild(i);
            }
        }
        else
        {
            spawnLocations = new Transform[1] {transform};
        }

        Character[] newSpawns = new Character[SpawnCharacters.Length];
        for (int i = 0; i < SpawnCharacters.Length; i++)
        {
            int locIndex = spawnLocations.Length * i/SpawnCharacters.Length;
            newSpawns[i] = SpawnCharacters[i].Spawn(spawnLocations[locIndex].position, FactionID);
        }
        return newSpawns;
    }
}