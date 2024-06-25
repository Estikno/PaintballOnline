using Riptide;
using Riptide.Transports.Tcp;
using Riptide.Utils;
using System;
using Unity.VisualScripting;
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
    weaponShoot,
    reloadWeapon,
    health,
    respawn,
    ping
}

/// <summary>
/// The id's of the messages send from the client to the server
/// </summary>
public enum ClientToServerId : ushort
{
    name = 1,
    input,
    primaryUse,
    reloadWeapon,
    ping,
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

    [SerializeField] private ushort tickDivergeTolerance = 5;

    //other
    private string provisionalUsername;
    private bool hasSentName = false;
    private bool hasReceiveSync = false;
    private bool connected = false;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        Application.targetFrameRate = 120;

        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);

        Client = new Client(); //for udp
        //Client = new Client(new TcpClient()); //for tcp   

        Client.Connected += DidConnect;
        Client.ConnectionFailed += FailedToConnect;
        Client.ClientDisconnected += PlayerLeft;
        Client.Disconnected += DidDisconnect;

        ServerTick = 2;
    }

    private void FixedUpdate()
    {
        Client.Update();

        if (SceneManager.GetActiveScene().name != "Menu" && !hasSentName)
        {
            Debug.Log("Sending name");
            SendName();
            hasSentName = true;
        }

        //ping
        if (connected && ServerTick % 30 == 0)
        {
            Message message = Message.Create(MessageSendMode.Unreliable, ClientToServerId.ping);
            message.AddUShort(ServerTick);

            Client.Send(message);
        }

        ServerTick++;
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

    public void Disconnect()
    {
        Client.Disconnect();
        hasSentName = false;
        connected = false;
        hasReceiveSync = false;
    }

    public void DidConnect(object sender, EventArgs e)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        connected = true;
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
        hasSentName = false;
        connected = false;
        hasReceiveSync = false;
    }

    private void SetTick(ushort serverTick)
    {
        if(Mathf.Abs(ServerTick - serverTick) > tickDivergeTolerance)
        {
            Debug.Log($"Client tick: {ServerTick} -> {serverTick}");
            ServerTick = serverTick;
        }
    }

    #region Messages

    [MessageHandler((ushort)ServerToClientId.sync)]
    private static void Sync(Message message)
    {
        Instance.SetTick(message.GetUShort());

        if (!Instance.hasReceiveSync)
        {
            Instance.hasReceiveSync = true;
            GameLogic.Instance.LoadingScreen.SetActive(false);
        }
    }

    public void SendName()
    {
        Message message = Message.Create(MessageSendMode.Reliable, ClientToServerId.name);
        message.AddString(provisionalUsername);

        Client.Send(message);
    }

    [MessageHandler((ushort)ServerToClientId.ping)]
    public static void Ping(Message message)
    {
        ushort sent = message.GetUShort();
        ushort received = message.GetUShort();

        float ping = ((float)received - (float)sent) / 60f;

        GameLogic.Instance.PingText.text = $"{Mathf.Abs(Mathf.Round(ping * 100f) / 100f)} ms";
    }

    #endregion
}
