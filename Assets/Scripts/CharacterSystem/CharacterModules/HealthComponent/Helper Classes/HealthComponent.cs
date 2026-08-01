using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HealthComponent
{
    [field: Header("Base Data")]
    public readonly int BaseMaxHealth = 100;

    [field: Header("Current Data")]
    [field: SerializeField] public int CurrHealth {get; private set;}
    [field: SerializeField] public int MaxHealth {get; private set;}
    [field: SerializeField] public int shield {get; private set;}
    public float health_ratio => CurrHealth/(float)MaxHealth;
    public int total_curr_hitpoints => CurrHealth + shield;
    public int total_max_hitpoints => MaxHealth + shield;
    public bool is_alive => CurrHealth > 0;
    //public float  {get; private set;}

    private InputEventRelay _inputRelay;

    [Header("Stat Modifiers")]
    [SerializeField] List<ChangeHealthTick> health_ticks = new List<ChangeHealthTick>();

    #region initializers
    // copy constructor
    public HealthComponent(HealthComponent copied)
    {
        this.BaseMaxHealth = copied.MaxHealth;
        this.MaxHealth = BaseMaxHealth;
        this.CurrHealth = BaseMaxHealth;
        this.shield = copied.shield;
    }
    public HealthComponent(CharacterSO base_data)
    {
        this.BaseMaxHealth = base_data.health;
        this.MaxHealth = base_data.health;
        this.CurrHealth = base_data.health;
        this.shield = base_data.spawn_shield;
    }
    public void ResetHealthComponent(float spawn_health_perc = 1)
    {
        MaxHealth = BaseMaxHealth;
        CurrHealth = (int)(MaxHealth * spawn_health_perc);

        health_ticks.Clear();
    }
    #endregion

    public void UpdateHealth()
    {
        if (health_ticks.Count > 0)
        {
            int net_health_change = 0;
            for(int i = health_ticks.Count-1; i >= 0; i--)
            {
                ChangeHealthTick health_tick = health_ticks[i];
                if (health_tick.effect_complete)
                {
                    // swap n pop removal
                    int list_end = health_ticks.Count - 1;
                    health_ticks[i] = health_ticks[list_end];
                    health_ticks.RemoveAt(list_end); 
                }
                else
                {
                    net_health_change += health_tick.UpdateTick();
                    health_ticks[i] = health_tick;
                }
            }
            if (net_health_change != 0)
            {
                ChangeHealth(net_health_change);
            }
        }
    }

    public void ChangeHealth(int damage_amt)
    {     
        if (damage_amt > 0) // this is damage
        {
            shield -= damage_amt;
            damage_amt = -shield;
            if (shield <= 0)
            {
                shield = 0;
            } 
            if (damage_amt > 0)
            {
                CurrHealth -= damage_amt;
                if (CurrHealth < 0)
                {
                    CurrHealth = 0;
                }
            }

        } 
        else // negative damage is healing
        {
            CurrHealth -= damage_amt;
            if (CurrHealth > MaxHealth)
            {
                CurrHealth = MaxHealth;
            }
        }
    }
    // public void ChangeHealthTick(int change_amt, float duration, float tick_rate, AbilityEffectComponent effect_controller)
    // {
    //     health_ticks.Add(new ChangeHealthTick(change_amt, tick_rate, duration, effect_controller));
    // }
    #region Stat Change Methods
    // public void MaxHealthBoost(int boost_amt, float duration, AbilityEffectComponent effect_controller)
    // {
    //     float curr_ratio = health_ratio;
    //     MaxHealth += boost_amt;
    //     CurrHealth = (int)(MaxHealth * curr_ratio);
    // }
    // public void ShieldBoost(int boost_amt)
    // {
    //     shield += boost_amt;
    // }
    #endregion
}