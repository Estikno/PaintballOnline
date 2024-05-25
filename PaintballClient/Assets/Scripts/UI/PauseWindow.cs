using QuantumTek.QuantumUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseWindow : MonoBehaviour
{
    private static PauseWindow instance;
    public static PauseWindow Instance
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
                Debug.Log($"{nameof(PauseWindow)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public bool IsPaused { get; private set; } = false;

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private QUI_Window window;

    public void DisplayPauseMenu(bool display)
    {
        window.SetActive(display);
        IsPaused = display;
    }

    public void Exit()
    {
        SettingsManager.GetMouseActivity = true;
        NetworkManager.Instance.Disconnect();
        SceneManager.LoadScene(0);
    }

    public void Resume()
    {
        CameraController.Instance.Resume();
    }
}
