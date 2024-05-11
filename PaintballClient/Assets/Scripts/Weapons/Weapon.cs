using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Riptide;
using System;
using DG.Tweening;

/// <summary>
/// This are the 3 main types of weapons
/// </summary>
public enum GunType
{
    rifle,
    mid_tier,
    pistol,
    knife
}

/// <summary>
/// This is the weapon class that every weapon has to contain
/// </summary>
[RequireComponent(typeof(Interpolator))]
public class Weapon : MonoBehaviour
{
    /// <summary>
    /// This dictionary contains all weapons in the game
    /// </summary>
    public static Dictionary<ushort, Weapon> Weapons = new Dictionary<ushort, Weapon>();

    //Some references
    [Header("OBJ References")]
    public int weaponGfxLayer;
    public GameObject[] weaponGFXs;
    public WeaponSway weaponSway;

    [Header("Values")]
    [SerializeField] private float camShakeMagnitude = .025f;
    [SerializeField] private float camShakeDuration = .05f;

    //Weapons properties
    public ushort WeaponId { get; private set; }
    public string Name { get; private set; }
    [HideInInspector]
    public bool Held;
    public GunType GunType { get; private set; }

    //The manager that helds this weapons
    public WeaponManager manager { get; private set; }

    [Header("Recoil")]
    [SerializeField] private float kickBackForce = 0.075f;

    public Interpolator interpolator { get; private set; }

    private bool reloading;

    private void Awake()
    {
        //anim = GetComponent<Animator>();
        interpolator = GetComponent<Interpolator>();
    }

    public void Shoot(bool isLocalPlayer)
    {
        //print("shoot: " + DOTween.Kill(transform, false));
        DOTween.Kill(transform, false);

        //anim.Play("Idle");

        if (GunType == GunType.knife)
        {
            //plays the attack animation of the knife
            //anim.Play("Attack");

            //audio
            /*if (isLocalPlayer)
                AudioManager.Instance.Play("KnifeImpact", true);
            else
                AudioManager.Instance.PlayAudioIn3DSpace("KnifeImpact", transform.position, 5, 30);*/
        }
        else
        {
            //set the weapons position to the kickback force
            transform.localPosition -= new Vector3(0, 0, kickBackForce);

            //Move the gun slowly to the original position
            transform.DOLocalMoveZ(0f, .2f);

            //audio
            /*if (isLocalPlayer)
                AudioManager.Instance.Play("GunShoot", true);
            else
                AudioManager.Instance.PlayAudioIn3DSpace("GunShoot", transform.position, 5, 100);*/

            if (isLocalPlayer) StartCoroutine(CameraShake.Instance.Shake(camShakeDuration, camShakeMagnitude));
        }
    }

    public void Reload(bool isFinished, float _reloadTime, bool isLocalPlayer)
    {
        reloading = !isFinished;
        //print("reloading: " + isFinished);

        if (!isFinished)
        {
            //anim.Play("Idle");
            if(isLocalPlayer) weaponSway.Stop();

            //make the rotation animation using dotween
            transform.DOLocalRotate(new Vector3(360.01f, 0f, 0f), _reloadTime, RotateMode.FastBeyond360).SetEase(Ease.OutCubic);

            //audio
            /*if (isLocalPlayer)
                AudioManager.Instance.Play("GunReload", true);
            else
                AudioManager.Instance.PlayAudioIn3DSpace("GunReload", transform.position, 5, 30);*/
        }
        else
        {
            DOTween.Kill(transform, false);
            if(isLocalPlayer) weaponSway.Initiate();

            //set the position to 0 so that there is no problem with the movement
            transform.localRotation = Quaternion.Euler(Vector3.zero);
        }
    }

    /// <summary>
    /// This function spawns a weapon
    /// </summary>
    /// <param name="id">The id of the new weapon</param>
    /// <param name="name">Its name</param>
    /// <param name="position">The position to spawn to</param>
    /// <param name="rotation">Its rotation</param>
    /// <param name="type">The type of the gun</param>
    public static void Spawn(ushort id, string name, Vector3 position, Quaternion rotation, int type)
    {
        Weapon weapon = null;

        //Search the weapon prefab to instantiate
        foreach (NameWeapon _name in GameLogic.Instance.Weapons)
        {
            if (_name.Name.ToLower() == name.ToLower())
                weapon = Instantiate(_name.prefab, position, rotation).GetComponent<Weapon>();
        }

        //If the prefab hasn't been found exit the function
        if (weapon == null)
        {
            Debug.LogError($"The weapon: {name} hasn't not been found in the prefab list");
            return;
        }

        //Set basic properties
        weapon.WeaponId = id;
        weapon.Name = name;
        weapon.GunType = (GunType)type;

        //Lastly, adds this weapon to the weapons list
        Weapons.Add(id, weapon);
    }

    /// <summary>
    /// Simply plays the select animation of the gun
    /// </summary>
    /*public void SelectAnimation()
    {
        anim.Play("Select");
    }*/

    #region Messages

    [MessageHandler((ushort)ServerToClientId.weaponMovement)]
    private static void WeaponMovement(Message message)
    {
        if (Weapon.Weapons.TryGetValue(message.GetUShort(), out Weapon weapon))
        {
            weapon.interpolator.NewUpdate(message.GetUShort(), message.GetVector3(), message.GetQuaternion());
        }
    }

    #endregion
}
