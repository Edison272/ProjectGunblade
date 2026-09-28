using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsScreen : MonoBehaviour
{
    public Button grayscale_button;
    public Image[] color_references;
    public Material grayscale_material;
    public Material normal_material;
    public Slider sensitivity_slider;
    public TextMeshProUGUI slider_text;
    public void Start()
    {
        SetGrayScale();
        slider_text.text = "Sensitivity: " + GameSettings.sensitivity;
    }
    public void ToggleGrayScale()
    {
        GameSettings.grayscale = !GameSettings.grayscale;
        SetGrayScale();
    }
    private void SetGrayScale()
    {
        foreach(Image color in color_references)
        {
            color.material = GameSettings.grayscale ? grayscale_material : normal_material;
        }
        string button_text = GameSettings.grayscale ? "Grayscale On (-)" : "Grayscale Off (O)";
        grayscale_button.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = button_text;
    }

    public void SetSensitivity()
    {
        GameSettings.sensitivity = sensitivity_slider.value;
        slider_text.text = "Sensitivity: " + GameSettings.sensitivity;
    }

    
}