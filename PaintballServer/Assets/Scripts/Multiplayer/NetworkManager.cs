using UnityEngine;
using Riptide.Utils;
using Riptide;

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
    health
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
            if(instance == null)
            {
                instance = value;
            }
            else if(instance != null)
            {
                Debug.Log($"{nameof(NetworkManager)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public Server Server {  get; private set; }

    //Every 27.3 minutes the currentTick needs to be reseted
    public ushort CurrentTick { get; private set; } = 0;

    [SerializeField] private ushort port;
    [SerializeField] private ushort maxClientCount;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Application.targetFrameRate = 60;

#if UNITY_EDITOR
        RiptideLogger.Initialize(Debug.Log, Debug.Log, Debug.LogWarning, Debug.LogError, false);
#else
        System.Console.Title = "Server";
        System.Console.Clear();
        Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);
        RiptideLogger.Initialize(Debug.Log, true);
#endif

        Server = new Server();
        Server.Start(port, maxClientCount);
        Server.ClientDisconnected += PlayerLeft;
    }

    private void FixedUpdate()
    {
        Server.Update();

        if (CurrentTick % 200 == 0)
            SendSync();

        CurrentTick++;
    }

    private void OnApplicationQuit()
    {
        Server.Stop();
    }

    private void PlayerLeft(object sender, ServerDisconnectedEventArgs e)
    {
        if (Player.list.TryGetValue(e.Client.Id, out Player player))
            Destroy(player.transform.parent.gameObject);
    }

    private void SendSync()
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.sync);
        message.AddUShort(CurrentTick);

        Server.SendToAll(message);
    }
}
