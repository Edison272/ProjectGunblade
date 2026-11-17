using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.UI;

public class EntityHealthBars : MonoBehaviour
{
    private static EntityHealthBars _instance;
    public static EntityHealthBars Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<EntityHealthBars>();
                if (_instance != null)
                    return _instance;
                EntityHealthBars instance = Resources.Load<EntityHealthBars>("Prefabs/UI/Canvas/EntityHealthBars");
                _instance = Instantiate(instance).GetComponent<EntityHealthBars>();
                DontDestroyOnLoad(_instance);
            }
            return _instance;
        }
        private set { _instance = value; }
    }

    private static GameObject _barPrefab;
    private static LinkedList<HealthUI> _activeUI = new LinkedList<HealthUI>();
    private static Stack<GameObject> _inactiveUI = new Stack<GameObject>();
    void Start()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }
    public static void AddHealthBar(Transform uiTransform, HealthComponent health)
    {
        // null safety check
        GameObject new_instance = null;
        while (_inactiveUI.Count > 0 && !new_instance)
        {
            new_instance = _inactiveUI.Pop(); // skip entries destroyed externally
        } 
        if (!_barPrefab) _barPrefab = Resources.Load<GameObject>("Prefabs/UI/OnField/HealthBar1");
        if (!new_instance) new_instance = Instantiate(_barPrefab, Instance.transform);
        new_instance.SetActive(true);
        _activeUI.AddLast(new HealthUI(uiTransform, health, new_instance));
    }
    public void Update()
    {
        LinkedListNode<HealthUI> current = _activeUI.First;
        while (current != null)
        {
            HealthUI val = current.Value;
            if (!val.BarTransform || val.Health == null)
            {
                _activeUI.Remove(current);
                RemoveHealthBar(val.HealthBar);
            }
            else
            {
                val.HealthBar.transform.position = val.BarTransform.position;
                val.UpdateHealth();
            }
            current = current.Next;
        }
    }
    public void RemoveHealthBar(GameObject remove_instance)
    {
        remove_instance.SetActive(false);
        _inactiveUI.Push(remove_instance);
    }

    #region Health UI
    private struct HealthUI
    {
        public Transform BarTransform;
        public HealthComponent Health;
        public GameObject HealthBar;

        [field: Header("UI Elements")]
        public Image health_bar;
        public Image health_drift_bar;
        public Image shield_bar;
        private const float c_health_drift_lerp = 5;
        public HealthUI(Transform uiTransform, HealthComponent health, GameObject instance)
        {
            BarTransform = uiTransform;
            Health = health;
            HealthBar = instance;

            // set transform elements
            health_drift_bar = HealthBar.transform.GetChild(0).GetChild(0).GetComponent<Image>();
            health_bar = HealthBar.transform.GetChild(0).GetChild(1).GetComponent<Image>();
            shield_bar = HealthBar.transform.GetChild(0).GetChild(2).GetComponent<Image>();
        }
        public void UpdateHealth()
        {
            //HealthBar.GetComponent<Image>().material.SetFloat("_Percent", Health.health_ratio);

            float MaxHealth_ratio = (float)Health.MaxHealth / Health.total_max_hitpoints;
            float shield_ratio = 1 - MaxHealth_ratio;
            
            health_bar.fillAmount = Health.health_ratio * MaxHealth_ratio;

            // shield_bar.fillAmount = shield_ratio;
            // Vector3 shield_bar_pos += Vector3(health_bar.fillAmount, 0, 0);
            // shield_bar_pos.x += MaxHealth_ratio;
            // shield_bar.transform.localPosition = shield_bar_pos;   
        }
    }
    #endregion
}