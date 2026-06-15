using UnityEngine;

[System.Serializable]
public class HealthUI
{
    [field: Header("UI Elements")]
    public Transform bg_bar;
    public Transform health_bar;
    public Transform health_drift_bar;
    public Transform shield_bar;
    private HealthComponent health_component;
    private const float c_health_drift_lerp = 5;

    public void InitializeHealthUI(HealthComponent health_component)
    {
        this.health_component = health_component;
        SetHealth();
    }
    public void UpdateHealthUI()
    {
        SetHealth();

        // drift effect
        float drift_x = Mathf.Lerp(health_bar.localScale.x, health_drift_bar.localScale.x, 1-c_health_drift_lerp*Time.deltaTime);
        health_drift_bar.localScale = new Vector3(drift_x, 1, 1);
    }

    public void SetHealth()
    {
        float max_health_ratio = (float)health_component.max_health / health_component.total_max_hitpoints;
        float shield_ratio = 1 - max_health_ratio;
        
        health_bar.localScale = new Vector3(health_component.health_ratio * max_health_ratio, 1, 1);

        shield_bar.localScale = new Vector3(shield_ratio, 1, 1);
        Vector3 shield_bar_pos = health_bar.localPosition;
        shield_bar_pos.x += max_health_ratio;
        shield_bar.transform.localPosition = shield_bar_pos;        
    }
}