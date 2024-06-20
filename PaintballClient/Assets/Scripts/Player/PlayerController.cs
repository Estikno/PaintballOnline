using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using System;

public class PlayerController : MonoBehaviour
{
    private static PlayerController instance;
    public static PlayerController Instance
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
                Debug.Log($"{nameof(PlayerController)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private Transform camHolder;

    public bool[] Inputs => inputs;

    private bool[] inputs;

    private Queue<KeyCode> otherInputs = new Queue<KeyCode>();

    private void Start()
    {
        inputs = new bool[6];
    }

    private void Update()
    {
        if (PauseWindow.Instance.IsPaused) return;

        if(Input.GetKey(KeyCode.W))
            inputs[0] = true;

        if (Input.GetKey(KeyCode.S))
            inputs[1] = true;

        if (Input.GetKey(KeyCode.A))
            inputs[2] = true;

        if (Input.GetKey(KeyCode.D))
            inputs[3] = true;

        /*if (Input.GetKey(KeyCode.Space))
            inputs[4] = true;*/

        if (Input.GetKey(KeyCode.LeftShift))
            inputs[5] = true;

        if (Input.GetMouseButton(0))
            otherInputs.Enqueue(KeyCode.Mouse0);

        if (Input.GetKeyDown(KeyCode.R))
            otherInputs.Enqueue(KeyCode.R);
    }

    private void FixedUpdate()
    {
        SendInput();

        if (otherInputs.Count > 0)
        {
            foreach (KeyCode key in otherInputs)
            {
                switch (key)
                {
                    case KeyCode.Mouse0:
                        SendPrimaryUse();
                        break;
                    case KeyCode.R:
                        SendReload();
                        break;
                }
            }
        }

        for (int i = 0; i < inputs.Length; i++)
            inputs[i] = false;

        otherInputs.Clear();
    }

    #region Messages

    private void SendInput()
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ClientToServerId.input);
        message.AddBools(inputs, false);
        message.AddVector3(camHolder.forward);

        NetworkManager.Instance.Client.Send(message);
    }

    /// <summary>
    /// Sends the usage of the mouse left click
    /// </summary>
    private void SendPrimaryUse()
    {
        NetworkManager.Instance.Client.Send(Message.Create(MessageSendMode.Reliable, ClientToServerId.primaryUse));
    }

    /// <summary>
    /// Sends the reload input
    /// </summary>
    private void SendReload()
    {
        NetworkManager.Instance.Client.Send(Message.Create(MessageSendMode.Reliable, ClientToServerId.reloadWeapon));
    }

    /*
    /// <summary>
    /// Sends the pick up input
    /// </summary>
    private void SendPickUp()
    {
        NetworkManager.Instance.Client.Send(Message.Create(MessageSendMode.Reliable, ClientToServerId.pickUpWeapon));
    }

    /// <summary>
    /// Sends the drop input
    /// </summary>
    private void SendDrop()
    {
        NetworkManager.Instance.Client.Send(Message.Create(MessageSendMode.Reliable, ClientToServerId.dropWeapon));
    }

    /// <summary>
    /// Sends the switch input
    /// </summary>
    private void SendSwitchWeapon(int index)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ClientToServerId.switchWeapon);
        message.AddInt(index);

        NetworkManager.Instance.Client.Send(message);
    }*/

    #endregion
}
