using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;

[RequireComponent(typeof(PlayerMovement))]
public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public string Username { get; private set; }
    public PlayerMovement PlayerMovement => playerMovement;
    public Gun PlayerGun => playerGun;

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Gun playerGun;

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    public static void Spawn(ushort id, string username)
    {
        foreach (Player otherPlayer in list.Values)
            otherPlayer.SendSpawned(id);

        Player player = Instantiate(GameLogic.Instance.PlayerPrefab, new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponentInChildren<Player>();
        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.Username = string.IsNullOrEmpty(username) ? "Guest" : username;

        player.SendSpawned();
        list.Add(id, player);
    }

    #region Messages

    private void SendSpawned()
    {
        NetworkManager.Instance.Server.SendToAll(AddSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.playerSpawned)));
    }

    private void SendSpawned(ushort toClientId)
    {
        NetworkManager.Instance.Server.Send(AddSpawnData(Message.Create(MessageSendMode.Reliable, ServerToClientId.playerSpawned)), toClientId);
    }

    private Message AddSpawnData(Message message)
    {
        message.AddUShort(Id);
        message.AddString(Username);
        message.AddVector3(transform.parent.position);

        return message;
    }

    [MessageHandler((ushort)ClientToServerId.name)]
    private static void Name(ushort fromClientId, Message message)
    {
        Spawn(fromClientId, message.GetString());
    }

    [MessageHandler((ushort)ClientToServerId.input)]
    private static void Input(ushort fromClientId, Message message)
    {
        if(list.TryGetValue(fromClientId, out Player player))
            player.PlayerMovement.SetInput(message.GetBools(6), message.GetVector3());
    }

    [MessageHandler((ushort)ClientToServerId.primariClick)]
    private static void PrimariClick(ushort fromClientId, Message message)
    {
        if (list.TryGetValue(fromClientId, out Player player))
            player.PlayerGun.Shoot();
    }

    [MessageHandler((ushort)ClientToServerId.reloadClick)]
    private static void ReloadClick(ushort fromClientId, Message message)
    {
        if (list.TryGetValue(fromClientId, out Player player))
            player.PlayerGun.StartReload();
    }

    #endregion
}
