using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The 3 main types of guns available in the game
/// </summary>
public enum GunType : byte
{
    rifle,
    mid_tier,
    pistol,
    knife
}

/// <summary>
/// Represents a gun and its properties in the game
/// </summary>
[CreateAssetMenu(fileName = "Gun", menuName = "Gun")]
public class GunData : ScriptableObject
{
    [Header("Info")]
    public string Name; // The name of the gun
    public GunType Type = GunType.rifle; // The type of the gun

    [Space(5)]

    [Header("Shooting")]
    public int Damage; // The amount of damage the gun deals
    public float MaxDistance; // The maximum distance the gun can shoot

    [Space(5)]

    [Header("Reloading")]
    public int MagSize; // The magazine size of the gun
    public float FireRate; // The rate of fire of the gun (rounds/minute)
    public float ReloadTime; // The time it takes to reload the gun

    [Space(5)]

    [Header("CameraShake")]
    [Range(0f, 1f)]
    public float ShakeDuration; // The duration of the camera shake effect
    [Range(0f, 2f)]
    public float ShakeMagnitude; // The magnitude of the camera shake effect
}
