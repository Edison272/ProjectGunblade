using System;
using UnityEngine;
using UnityEngine.Android;

public interface IHealth
{    HealthComponent health_component {get;}
    void ChangeHealth(int change_amt);
    //void MaxHealthBoost(int boost_amt, float duration, AbilityEffectComponent effect_controller = null);
    //void ChangeHealthTick(int change_amt, float duration, float tick_rate, AbilityEffectComponent effect_controller = null);
    void ShieldBoost(int boost_amt);
}
[System.Serializable]
public struct ChangeHealthTick
{
    [SerializeField] int health_change;
    [field: SerializeField] public float change_rate {get; private set;}
    private float rate_timer;
    [SerializeField] float duration;
    public bool effect_complete => duration <= 0;
    private float duration_modifier;
    //private AbilityEffectComponent effect_controller;
    // public ChangeHealthTick(int health_change, float change_rate, float duration, AbilityEffectComponent effect_controller)
    // {
    //     this.health_change = health_change;
    //     this.change_rate = change_rate;
    //     this.rate_timer = 0;
    //     this.duration = duration;   
    //     duration_modifier = 1;
    //     //this.effect_controller = effect_controller;
    // }
    public int UpdateTick()
    {
        // if (effect_controller != null && !effect_controller.is_active)
        // {
        //     StopEffect();
        // }

        duration -= Time.deltaTime;
        rate_timer += Time.deltaTime;

        int regen_amt = 0;
        if (rate_timer >= change_rate)
        {
            int ticks = (int)(rate_timer/change_rate);
            regen_amt += health_change * ticks;
            rate_timer -= change_rate * ticks;
        }
        return regen_amt;
    }
    public void StopEffect()
    {
        duration = 0;
    }
}