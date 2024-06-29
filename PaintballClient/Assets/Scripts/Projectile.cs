using Riptide;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public static Dictionary<ushort, Projectile> list = new Dictionary<ushort, Projectile>();

    [SerializeField] private WeaponType type;
    private ushort id;

    private void OnDestroy()
    {
        list.Remove(id);
    }

    public static void Spawn(ushort id, WeaponType type, ushort shooterId, Vector3 position, Vector3 direction, string shoot_tick)
    {
        Player.list[shooterId].WeaponManager.Shot(type);

        //calculate rtt when shooting
        if (Player.list[shooterId].IsLocal)
        {
            DateTime now = DateTime.UtcNow;
            DateTime before = DateTime.Parse(shoot_tick, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);

            double ms = (now - before).TotalMilliseconds;

            UIManager.Singleton.ShootRTTUpdate(ms.ToString("0.000"));
            Player.list[shooterId].UpdateShootRtt(ms.ToString("0.000"));
        }

        Projectile projectile;
        switch (type)
        {
            case WeaponType.pistol:
                projectile = Instantiate(GameLogic.Singleton.BulletPrefab, position, Quaternion.LookRotation(direction)).GetComponent<Projectile>();
                break;
            case WeaponType.teleporter:
                projectile = Instantiate(GameLogic.Singleton.TeleporterPrefab, position, Quaternion.LookRotation(direction)).GetComponent<Projectile>();
                break;
            case WeaponType.laser:
                projectile = Instantiate(GameLogic.Singleton.LaserPrefab, position, Quaternion.LookRotation(direction)).GetComponent<Projectile>();
                break;
            default:
                Debug.LogError($"Can't spawn unknown projectile type '{type}'!");
                return;
        }

        projectile.name = $"Projectile {id}";
        projectile.id = id;

        list.Add(id, projectile);
    }

    #region Messages
    [MessageHandler((ushort)ServerToClientId.projectileSpawned)]
    private static void ProjectileSpawned(Message message)
    {
        Spawn(message.GetUShort(), (WeaponType)message.GetByte(), message.GetUShort(), message.GetVector3(), message.GetVector3(), message.GetString());
    }

    [MessageHandler((ushort)ServerToClientId.projectileMovement)]
    private static void ProjectileMovement(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Projectile projectile))
            projectile.transform.position = message.GetVector3();
    }

    [MessageHandler((ushort)ServerToClientId.projectileCollided)]
    private static void ProjectileCollided(Message message)
    {
        if (list.TryGetValue(message.GetUShort(), out Projectile projectile))
            Destroy(projectile.gameObject);
    }

    [MessageHandler((ushort)ServerToClientId.projectileHitmarker)]
    private static void ProjectileHitmarker(Message message)
    {
        UIManager.Singleton.ShowHitmarker();
    }
    #endregion
}
