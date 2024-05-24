using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button[] mainButtons;

    public void QuitGame()
    {
        Application.Quit();
    }

    public void DisableAllOptions(bool disable)
    {
        foreach(Button button in mainButtons)
        {
            button.interactable = !disable;
        }
    }
}
