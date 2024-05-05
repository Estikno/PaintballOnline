using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }

    public WeaponManager WeaponManager => weaponManager;

    [SerializeField] private Transform camHolder;
    [SerializeField] private Interpolator interpolator;
    [SerializeField] private WeaponManager weaponManager;

    private string username;

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    private void Move(ushort tick, Vector3 newPosition, Vector3 forward)
    {
        //transform.position = newPosition;

        interpolator.NewUpdate(tick, newPosition);

        if(!IsLocal)
            camHolder.forward = forward;
    }

    public static void Spawn(ushort id, string username, Vector3 position)
    {
        Player player;

        if(id == NetworkManager.Instance.Client.Id)
        {
            player = Instantiate(GameLogic.Instance.LocalPlayerPrefab, position, Quaternion.identity).GetComponentInChildren<Player>();
            player.IsLocal = true;
        }
        else
        {
            player = Instantiate(GameLogic.Instance.PlayerPrefab, position, Quaternion.identity).GetComponentInChildren<Player>();
            player.IsLocal = false;
        }

        player.name = $"Player {id} ({(string.IsNullOrEmpty(username) ? "Guest" : username)})";
        player.Id = id;
        player.username = username;

        list.Add(id, player);
    }

    #region Messages

    [MessageHandler((ushort)ServerToClientId.playerSpawned)]
    private static void SpawnPlayer(Message message)
    {
        Spawn(message.GetUShort(), message.GetString(), message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.playerMovement)]
    private static void PlayerMovement(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))
            player.Move(message.GetUShort(), message.GetVector3(), message.GetVector3());
    }

    #endregion
}
