using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DG.Tweening;
using System;

public class WeaponManager : MonoBehaviour
{
    [HideInInspector]
    public int SelectedWeapon { get; private set; } = 0; //The selected weapon in your inventory
    [SerializeField] private Weapon Weapon;

    [SerializeField] private Transform weaponHolder;
    [SerializeField] private WeaponFingerValues weaponFingerValues;

    /// <summary>
    /// This picks up a weapon
    /// </summary>
    /// <param name="weapon"></param>
    public void PickUp(bool isLocalPlayer, ushort PlayerId)
    {
        Weapon.AddId(PlayerId);

        //Applies the weaponGfx layer to the meshes
        if (isLocalPlayer)
        {
            foreach (GameObject gfx in Weapon.weaponGFXs)
            {
                gfx.layer = Weapon.weaponGfxLayer;
            }
        }

        //sway
        if(isLocalPlayer) Weapon.weaponSway.Initiate();

        Weapon.Holder = PlayerId;

        //hand IK
        if(!isLocalPlayer) weaponFingerValues.Init(Weapon.RightHand, Weapon.LeftHand, Weapon.PoleRight, Weapon.PoleLeft);
    }

    public void Shoot(bool isLocalPlayer, ushort PlayerId)
    {
        Weapon.Shoot(isLocalPlayer, PlayerId);
    }

    public void Shoot(bool isLocalPlayer, Vector3 hitPoint, Vector3 normal, ushort PlayerId)
    {
        Weapon.Shoot(isLocalPlayer, PlayerId);
        Instantiate(GameLogic.Instance.BulletImpactEffect, hitPoint, Quaternion.identity).transform.forward = normal;
        Instantiate(GameLogic.Instance.HitMarkerEffect, hitPoint, Quaternion.identity).transform.forward = normal;
    }

    public void Reload(bool isFinished, float reloadTime, bool isLocalPlayer, ushort PlayerId)
    {
        Weapon.Reload(isFinished, reloadTime, isLocalPlayer, PlayerId);
    }

    #region Messages

    [MessageHandler((ushort)ServerToClientId.weaponShoot)]
    private static void ShootWeapon(Message message)
    {
        if (Player.list.TryGetValue(message.GetUShort(), out Player player))
        {
            if (message.GetBool())
                player.WeaponManager.Shoot(player.IsLocal, message.GetVector3(), message.GetVector3(), player.Id);
            else
                player.WeaponManager.Shoot(player.IsLocal, player.Id);
        }
    }

    [MessageHandler((ushort)ServerToClientId.reloadWeapon)]
    private static void ReloadWeapon(Message message)
    {
        if (Player.list.TryGetValue(message.GetUShort(), out Player player))
            player.WeaponManager.Reload(message.GetBool(), message.GetFloat(), player.IsLocal, player.Id);
    }

    #endregion
}
