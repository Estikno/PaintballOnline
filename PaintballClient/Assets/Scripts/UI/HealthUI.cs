using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Riptide;
using System;

public class HealthUI : MonoBehaviour
{
    private static HealthUI instance;
    public static HealthUI Instance
    {
        get => instance;
        private set
        {
            if (instance == null)
            {
                instance = value;
            }
            else if (instance != null)
            {
                Debug.Log($"{nameof(HealthUI)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    [SerializeField] private Slider healthSlider;
    [SerializeField] private TextMeshProUGUI healthText;

    private void Awake()
    {
        Instance = this;
    }

    [MessageHandler((ushort)ServerToClientId.health)]
    private static void Health(Message message)
    {
        int _health = message.GetInt();

        Instance.healthSlider.value = _health;
        Instance.healthText.text = _health.ToString();
    }
}
