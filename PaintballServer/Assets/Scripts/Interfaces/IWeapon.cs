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
    public void PickUp(Transform weaponHolder, Transform camp);

    /// <summary>
    /// Drops the weapon
    /// </summary>
    public void Drop();

    /// <summary>
    /// Sets if the weapon is selected
    /// </summary>
    /// <param name="select">The selected state</param>
    public void SetSelection(bool select);

    /// <summary>
    /// It shoots the weapon
    /// </summary>
    public void Shoot();

    /// <summary>
    /// It reloads the weapon
    /// </summary>
    public void Reload();
}
