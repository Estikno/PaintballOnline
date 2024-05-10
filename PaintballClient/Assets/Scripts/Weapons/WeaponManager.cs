using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DG.Tweening;

public class WeaponManager : MonoBehaviour
{
    [HideInInspector]
    public Weapon[] Weapons { get; private set; } = new Weapon[1]; //The weapons in your inventory
    public int SelectedWeapon { get; private set; } = 2; //The selected weapon in your inventory

    /// <summary>
    /// This selectes a weapon
    /// </summary>
    /// <param name="_weapon">The weapon to select</param>
    public void SetSelected(int _weapon)
    {
        for (int i = 0; i < Weapons.Length; i++)
        {
            if (i == _weapon)
                continue;

            if (Weapons[i] != null)
                Weapons[i].gameObject.SetActive(false);
        }

        SelectedWeapon = _weapon;

        Weapons[SelectedWeapon].gameObject.SetActive(true);
        //Weapons[SelectedWeapon].SelectAnimation();
    }

    /// <summary>
    /// This drops a weapon
    /// </summary>
    /// <param name="weapon">The weapon to drop</param>
    public void Drop(Weapon weapon)
    {
        //Kill all dotween proceses in the weapon transform so there is no strange position changes
        DOTween.Kill(weapon.transform, false);

        //Setup interpolator
        weapon.interpolator.SetupInterpolator();

        //Set other variables
        weapon.Held = false;
        weapon.transform.SetParent(null);
        Weapons[Helper.GetGunIndexWithType(weapon.GunType)] = null;

        foreach (GameObject gfx in weapon.weaponGFXs)
        {
            gfx.layer = 0;
        }
    }

    /// <summary>
    /// This picks up a weapon
    /// </summary>
    /// <param name="weapon"></param>
    public void PickUp(Weapon weapon, bool isLocalPlayer)
    {
        //Disable interpolator
        weapon.interpolator.ClearTransforms();

        //Set the position
        weapon.transform.SetParent(transform);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.Euler(Vector3.zero);
        weapon.Held = true;

        //Applies the weaponGfx layer to the meshes
        if (isLocalPlayer)
        {
            foreach (GameObject gfx in weapon.weaponGFXs)
            {
                gfx.layer = weapon.weaponGfxLayer;
            }
        }

        //weapon.SelectAnimation();
        Weapons[Helper.GetGunIndexWithType(weapon.GunType)] = weapon;
    }

    public void Shoot(Weapon weapon, bool isLocalPlayer, Vector3 dir, Vector3 bulletStartPos)
    {
        weapon.Shoot(isLocalPlayer);
        Instantiate(GameLogic.Instance.Bullet, bulletStartPos, Quaternion.identity).GetComponent<Bullet>().Initiate(dir);
    }

    public void Reload(Weapon weapon, bool isFinished, float reloadTime, bool isLocalPlayer)
    {
        weapon.Reload(isFinished, reloadTime, isLocalPlayer);
    }

    public void SetRotation(Vector3 forward)
    {
        if (forward != Vector3.zero)
        {
            Vector3 forw = (forward - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(forw);
        }
    }

    #region Messages

    [MessageHandler((ushort)ServerToClientId.addWeapon)]
    private static void AddWeapon(Message message)
    {
        Weapon.Spawn(message.GetUShort(), message.GetString(), message.GetVector3(), message.GetQuaternion(), message.GetInt());
    }

    [MessageHandler((ushort)ServerToClientId.pickUpWeapon)]
    private static void PickUpWeapon(Message message)
    {
        ushort weaponId = message.GetUShort();
        ushort playerId = message.GetUShort();

        if (Weapon.Weapons.TryGetValue(weaponId, out Weapon weapon))
        {
            if (Player.list.TryGetValue(playerId, out Player player))
            {
                player.WeaponManager.PickUp(weapon, player.IsLocal);
            }
        }
    }

    [MessageHandler((ushort)ServerToClientId.dropWeapon)]
    private static void DropWeapon(Message message)
    {
        if (Weapon.Weapons.TryGetValue(message.GetUShort(), out Weapon weapon))
        {
            if (Player.list.TryGetValue(message.GetUShort(), out Player player))
            {
                player.WeaponManager.Drop(weapon);
            }
        }
    }

    [MessageHandler((ushort)ServerToClientId.selectedWeapon)]
    private static void SelectWeapon(Message message)
    {
        ushort weaponId = message.GetUShort();
        bool selected = message.GetBool();
        ushort playerId = message.GetUShort();
        int gunType = message.GetInt();

        if (Weapon.Weapons.TryGetValue(weaponId, out Weapon weapon))
        {
            if (Player.list.TryGetValue(playerId, out Player player))
            {
                player.WeaponManager.SetSelected(gunType);
            }
        }
    }

    [MessageHandler((ushort)ServerToClientId.weaponShoot)]
    private static void ShootWeapon(Message message)
    {
        if (Weapon.Weapons.TryGetValue(message.GetUShort(), out Weapon weapon))
        {
            if (Player.list.TryGetValue(message.GetUShort(), out Player player))
                player.WeaponManager.Shoot(weapon, player.IsLocal, message.GetVector3(), message.GetVector3());
        }
    }

    [MessageHandler((ushort)ServerToClientId.reloadWeapon)]
    private static void ReloadWeapon(Message message)
    {
        if (Weapon.Weapons.TryGetValue(message.GetUShort(), out Weapon weapon))
            if (Player.list.TryGetValue(message.GetUShort(), out Player player))
                player.WeaponManager.Reload(weapon, message.GetBool(), message.GetFloat(), player.IsLocal);
    }

    /*[MessageHandler((ushort)ServerToClientId.knifeAttack)]
    private static void KnifeAttack(Message message)
    {
        if (Weapon.Weapons.TryGetValue(message.GetUShort(), out Weapon weapon))
        {
            if (Player.list.TryGetValue(message.GetUShort(), out Player player))
                player.weaponManager.Shoot(weapon, player.IsLocal);
        }
    }*/

    #endregion
}
