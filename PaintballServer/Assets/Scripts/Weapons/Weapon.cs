using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class Weapon : MonoBehaviour, IWeapon
{
    // Dictionary to store all weapons in the game
    public static Dictionary<ushort, Weapon> Weapons = new Dictionary<ushort, Weapon>();

    //Basic properties of the weapon
    public ushort WeaponId { get; private set; } = 0;
    public WeaponManager Manager { get; private set; }

    [Header("References")]
    [SerializeField] private GunData gunData;
    public GunData GunData => gunData;

    [Header("OBJ References")]
    [SerializeField] protected Collider[] Colliders;

    [Header("Throwing and picking up")]
    [SerializeField] protected float throwForce = 5f;
    [SerializeField] protected float throwExtraForce = 11f;
    [SerializeField] protected float rotationForce = 10f;

    protected Transform cam;
    protected Rigidbody rb;

    //variable to keep track of the realoading rotation time in live time
    protected float rotationTime;
    //variable to keep track of the fire rate
    protected float timeSinceLastShoot;

    private void OnDestroy()
    {
        Weapons.Remove(WeaponId);
    }

    public void destroyItself()
    {
        Destroy(gameObject);
    }

    protected virtual void Awake()
    {
        
    }

    protected virtual void Update()
    {
        //update timesincelastshoot
        timeSinceLastShoot += Time.deltaTime;
    }

    /*private void FixedUpdate()
    {
        //Send the weapon movement if it's not held by any player
        if (!Held)
            SendMovement();
    }*/

    public virtual void Shoot()
    {

    }

    public virtual void Reload()
    {

    }

    public virtual void PickUp(Transform weaponHolder, Transform camp, ushort player_id)
    {
        WeaponId = player_id;
        Weapons.Add(player_id, this);

        //Assign some variables
        cam = camp;

        //Destroy the rigidbody as it would not be necessary for now
        if (rb != null)
            Destroy(rb);

        //Stablish the position to fit the weapon holder
        transform.SetParent(weaponHolder);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        //Disable colliders
        foreach (Collider col in Colliders)
        {
            col.enabled = false;
        }

        //Set the manager variable
        Manager = GetComponentInParent<WeaponManager>();
    }
}
