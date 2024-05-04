using Riptide;
using Riptide.Utils;
using System;
using UnityEngine;

public enum ServerToClientId : ushort
{
    sync = 1,
    playerSpawned,
    playerMovement,
    playerShoot,
    playerReload,
}

public enum ClientToServerId : ushort
{
    name = 1,
    input,
    primariClick,
    reloadClick
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

    [SerializeField] private string ip;
    [SerializeField] private ushort port;

    [Space(10)]

    [SerializeField] private ushort tickDivergeTolerance = 1;

    private void Awake()
    {
        Instance = this;
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
    }

    private void OnApplicationQuit()
    {
        Client.Disconnect();
    }

    public void Connect()
    {
        Client.Connect($"{ip}:{port}");
    }

    public void DidConnect(object sender, EventArgs e)
    {
        ConnectUIManager.Instance.SendName();
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
        ConnectUIManager.Instance.BackToMain();
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
}
