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
    public WeaponManager WeaponManager => weaponManager;

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private WeaponManager weaponManager;

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    public static void Spawn(ushort id, string username)
    {
        foreach (Player otherPlayer in list.Values)
            otherPlayer.SendSpawned(id);

        Player player = Instantiate(GameLogic.Instance.PlayerPrefab, new Vector3(4.82f, .35f, 3.58f), Quaternion.identity).GetComponentInChildren<Player>();
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

    [MessageHandler((ushort)ClientToServerId.primaryUse)]
    private static void PrimaryUse(ushort fromClient, Message message)
    {
        if (list.TryGetValue(fromClient, out Player player))
            player.weaponManager.PrimaryUsePressed();
    }

    [MessageHandler((ushort)ClientToServerId.reloadWeapon)]
    private static void RealoadWeapon(ushort fromClient, Message message)
    {
        if (list.TryGetValue(fromClient, out Player player))
            player.weaponManager.ReloadWeapon();
    }

    #endregion
}
