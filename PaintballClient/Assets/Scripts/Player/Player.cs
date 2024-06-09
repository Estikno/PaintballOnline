using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

public class Player : MonoBehaviour
{
    public static Dictionary<ushort, Player> list = new Dictionary<ushort, Player>();

    public ushort Id { get; private set; }
    public bool IsLocal { get; private set; }

    public WeaponManager WeaponManager => weaponManager;

    [SerializeField] private Transform camHolder;
    [SerializeField] private Interpolator interpolator;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private Animator anim;

    [Header("Player Looking (not local player)")]
    [SerializeField] private Transform Head;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private Transform model;

    private string username;

    private void OnDestroy()
    {
        list.Remove(Id);
    }

    private void Move(ushort tick, Vector3 newPosition, Vector3 forward, Vector3 velocity)
    {
        //transform.position = newPosition;

        interpolator.NewUpdate(tick, newPosition);

        /*if(!IsLocal)
            camHolder.forward = forward;*/

        if (!IsLocal)
        {
            float yRotation = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;

            Head.forward = forward;
            weaponHolder.forward = forward;
            model.rotation = Quaternion.Euler(0, yRotation, 0);
            camHolder.rotation = Quaternion.Euler(0, yRotation, 0);

            //animator
            anim.SetFloat("VelocityX", velocity.x);
            anim.SetFloat("VelocityZ", velocity.z);
        }
    }

    public void Respawn(Vector3 pos)
    {
        transform.position = pos;
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
            player.Move(message.GetUShort(), message.GetVector3(), message.GetVector3(), message.GetVector3());
    }

    [MessageHandler((ushort)ServerToClientId.respawn)]
    private static void PlayerRespawn(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Player player))
            player.Respawn(message.GetVector3());
    }

    #endregion
}
