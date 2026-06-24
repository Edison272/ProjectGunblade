using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public struct BodyPartDictItem
{
    public GameObject key;
    public CharacterBodyPart value;
    public BodyPartDictItem(GameObject key, CharacterBodyPart value)
    {
        this.key = key;
        this.value = value;
    }
}

[System.Serializable]
public class BodyPartDictionary : IEnumerable<BodyPartDictItem>
{
    [SerializeField] List<BodyPartDictItem> stat_list = new();

    public void Add(GameObject key, CharacterBodyPart value)
    {
        stat_list.Add(new BodyPartDictItem(key, value));
    }
    public void Insert(int index, GameObject key, CharacterBodyPart value = CharacterBodyPart.None)
    {
        stat_list.Insert(index, new BodyPartDictItem(key, value));
    }
    public GameObject Key(int index)
    {
        return index >= stat_list.Count ? null : stat_list[index].key;
    }
    public void RemoveAt(int index)
    {
        if (index > 0 && index < stat_list.Count)
        {
            stat_list.RemoveAt(index);   
        }
    }
    public void RemoveNonMatching(GameObject[] match)
    {
        stat_list.RemoveAll(
            stat => !match.Contains(stat.key)
        );
    }
    public void Clear()
    {
        stat_list.Clear();
    }

    public bool Contains(GameObject key)
    {
        foreach(BodyPartDictItem item in stat_list)
        {
            if (item.key == key)
            {
                return true;
            }
        }
        return false;
    }
    public int Length => stat_list.Count;

    public Dictionary<GameObject, CharacterBodyPart> ToDictionary()
    {
        Dictionary<GameObject, CharacterBodyPart> new_dict = new Dictionary<GameObject, CharacterBodyPart>();
        foreach(BodyPartDictItem stat in stat_list) 
        {
            new_dict[stat.key] = stat.value;
        }
        return new_dict;
    }
    
    public IEnumerator<BodyPartDictItem> GetEnumerator()
    {
        return stat_list.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}