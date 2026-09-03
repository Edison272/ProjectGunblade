using UnityEngine;
using UnityEngine.UI;


/*
Display the stats of a stack counter using bars
*/
public class StackBarUI : StackCounterUI
{
    [field: Header("UI Elements")]
    public RectTransform Background;
    public Image StatBar;
    public Image StatDrift;

    private const float c_health_drift_lerp = 5;

    public override StackCounter StackCounter 
    {
        get {return _stackCounter;} 
        set
        {
            _stackCounter = value;
            StatBar.fillAmount = _stackCounter.GetStatus(); 
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        StatBar.fillAmount = StackCounter.GetStatus(); 
        
        // drift effect
        StatDrift.fillAmount = Mathf.Lerp(StatBar.fillAmount, StatDrift.fillAmount, 1-c_health_drift_lerp*Time.deltaTime);
    }
}
