using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct StatDictItem
{
    public string key;
    public float value;
    public StatDictItem(string key, float value)
    {
        this.key = key;
        this.value = value;
    }
}

[System.Serializable]
public class StatDictionary : IEnumerable<StatDictItem>
{
    [SerializeField] List<StatDictItem> stat_list = new();

    public void Add(string key, float value)
    {
        stat_list.Add(new StatDictItem(key, value));
    }
    public void Insert(int index, string key, float value = 0)
    {
        stat_list.Insert(index, new StatDictItem(key, value));
    }
    public string Key(int index)
    {
        return index >= stat_list.Count ? "0" : stat_list[index].key;
    }
    public float Value(int index)
    {
        return index >= stat_list.Count ? 0 : stat_list[index].value;
    }
    public void RemoveAt(int index)
    {
        if (index > 0 && index < stat_list.Count)
        {
            stat_list.RemoveAt(index);   
        }
    }
    public void RemoveNonMatching(string[] match)
    {
        stat_list.RemoveAll(
            stat => !match.Contains(stat.key)
        );
    }
    public void Clear()
    {
        stat_list.Clear();
    }

    public bool Contains(string key)
    {
        foreach(StatDictItem item in stat_list)
        {
            if (item.key == key)
            {
                return true;
            }
        }
        return false;
    }
    public int Length => stat_list.Count;

    public Dictionary<string, float> ToDictionary()
    {
        Dictionary<string, float> new_dict = new Dictionary<string, float>();
        foreach(StatDictItem stat in stat_list) 
        {
            new_dict[stat.key] = stat.value;
        }
        return new_dict;
    }
    
    public IEnumerator<StatDictItem> GetEnumerator()
    {
        return stat_list.GetEnumerator();
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}