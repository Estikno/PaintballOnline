using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SliderValue : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private bool sensY, sensX;

    private void Start()
    {
        slider.onValueChanged.AddListener((v) =>
        {
            text.text = v.ToString();

            if (sensY) SettingsManager.SetSensitivityY(v);
            else if(sensX) SettingsManager.SetSensitivityX(v);
        });
    }
}
