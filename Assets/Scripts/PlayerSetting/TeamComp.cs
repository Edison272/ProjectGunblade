using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TeamComp", menuName = "ScriptableObjects/PlayerSettings/TeamComp", order = 1)]
public class TeamCompSO : ScriptableObject
{
    public SquadPreset[] squad_presets; // each nest list is a squad, and index 0 of each list is the squad leader

    [Serializable]
    public struct SquadPreset
    {
        public Vector2 DeploymentLocation;
        public CharacterSO[] characters;
    }
}
