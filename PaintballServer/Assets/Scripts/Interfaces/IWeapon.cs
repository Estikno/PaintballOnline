using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This is the interface of any weapon
/// </summary>
public interface IWeapon
{
    /// <summary>
    /// The weapon data
    /// </summary>
    public GunData GunData { get; }

    /// <summary>
    /// Pick up the weapon
    /// </summary>
    /// <param name="weaponHolder">The weapon holder transform to attach the weapon</param>
    /// <param name="camp">The camera holder transform</param>
    public void PickUp(Transform weaponHolder, Transform camp, ushort player_id);

    /// <summary>
    /// It shoots the weapon
    /// </summary>
    public void Shoot();

    /// <summary>
    /// It reloads the weapon
    /// </summary>
    public void Reload();

    public void destroyItself();
}
