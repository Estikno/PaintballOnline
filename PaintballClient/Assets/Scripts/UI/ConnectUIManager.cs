using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Riptide;

public class ConnectUIManager : MonoBehaviour
{
    private static ConnectUIManager instance;
    public static ConnectUIManager Instance
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
                Debug.Log($"{nameof(ConnectUIManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    [Header("UIs")]
    [SerializeField] private GameObject connectUI;
    [SerializeField] private GameObject gameUI;

    [Header("Fields")]
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField IPField;

    private void Awake()
    {
        Instance = this;
    }

    public void ConnectClicked()
    {
        usernameField.interactable = false;
        IPField.interactable = false;

        //set UIs values
        connectUI.SetActive(false);
        gameUI.SetActive(true);

        NetworkManager.Instance.Connect(IPField.text);
    }

    public void BackToMain()
    {
        usernameField.interactable = true;
        IPField.interactable = true;

        //set UIs values
        connectUI.SetActive(true);
        gameUI.SetActive(false);
    }

    public void SendName()
    {
        Message message = Message.Create(MessageSendMode.Reliable, ClientToServerId.name);
        message.AddString(usernameField.text);

        NetworkManager.Instance.Client.Send(message);
    }
}
