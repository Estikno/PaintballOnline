using Riptide;
using Riptide.Utils;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// The id's of the messages send from the server to the client
/// </summary>
public enum ServerToClientId : ushort
{
    sync = 1,
    playerSpawned,
    playerMovement,
    selectedWeapon,
    addWeapon,
    pickUpWeapon,
    dropWeapon,
    weaponMovement,
    weaponShoot,
    reloadWeapon,
}

/// <summary>
/// The id's of the messages send from the client to the server
/// </summary>
public enum ClientToServerId : ushort
{
    name = 1,
    input,
    primaryUse,
    switchWeapon,
    pickUpWeapon,
    dropWeapon,
    reloadWeapon
}

public class NetworkManager : MonoBehaviour
{
    private static NetworkManager instance;
    public static NetworkManager Instance
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
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public Client Client { get; private set; }
    private ushort _servertick;
    public ushort ServerTick
    {
        get => _servertick;
        private set
        {
            _servertick = value;
            InterpolationTick = (ushort)(value - TicksBetweenPositionUpdates);
        }
    }
    public ushort InterpolationTick { get; private set; }
    private ushort _ticksBetweenPositionUpdates = 2;
    public ushort TicksBetweenPositionUpdates
    {
        get => _ticksBetweenPositionUpdates;
        private set
        {
            _ticksBetweenPositionUpdates = value;
            InterpolationTick = (ushort)(ServerTick - value);
        }
    }

    //[SerializeField] private string ip;
    [SerializeField] private ushort port;

    [Space(10)]

    [SerializeField] private ushort tickDivergeTolerance = 1;

    //other
    private string provisionalUsername;
    private bool hasSentName = false;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Application.targetFrameRate = 120;

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Client = new Client();
        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedToConnect;
        Client.ClientDisconnected += PlayerLeft;
        Client.Disconnected += DidDisconnect;

        ServerTick = 2;
    }

    private void FixedUpdate()
    {
        Client.Update();
        ServerTick++;

        if (SceneManager.GetActiveScene().name != "Menu" && !hasSentName)
        {
            Debug.Log("Sending name");
            SendName();
            hasSentName = true;
        }
    }

    private void OnApplicationQuit()
    {
        Client.Disconnect();
    }

    public void Connect(string ip, string username)
    {
        provisionalUsername = username;
        Client.Connect($"{ip}:{port}");
    }

    public void DidConnect(object sender, EventArgs e)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private void FailedToConnect(object sender, EventArgs e)
    {
        ConnectUIManager.Instance.BackToMain();
    }

    private void PlayerLeft(object sender, ClientDisconnectedEventArgs e)
    {
        Destroy(Player.list[e.Id].transform.parent.gameObject);
    }

    private void DidDisconnect(object sender, EventArgs e)
    {
        //ConnectUIManager.Instance.BackToMain();
        SceneManager.LoadScene(0);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void SetTick(ushort serverTick)
    {
        if(Mathf.Abs(ServerTick - serverTick) > tickDivergeTolerance)
        {
            Debug.Log($"Client tick: {ServerTick} -> {serverTick}");
            ServerTick = serverTick;
        }
    }

    [MessageHandler((ushort)ServerToClientId.sync)]
    private static void Sync(Message message)
    {
        Instance.SetTick(message.GetUShort());
    }

    public void SendName()
    {
        Message message = Message.Create(MessageSendMode.Reliable, ClientToServerId.name);
        message.AddString(provisionalUsername);

        NetworkManager.Instance.Client.Send(message);
    }
}
