using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public abstract class Weapon : MonoBehaviour, IWeapon
{
    // Dictionary to store all weapons in the game
    public static Dictionary<ushort, Weapon> Weapons = new Dictionary<ushort, Weapon>();
    // ID for the next weapon
    public static ushort NextWeapon = 1;

    //Basic properties of the weapon
    public ushort WeaponId { get; private set; } = 0;
    public bool Held { get; private set; } = false;
    public bool Selected { get; private set; } = false;
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
        // Assign a unique weapon ID and add it to the Weapons dictionary
        WeaponId = NextWeapon;
        NextWeapon++;
        Weapons.Add(WeaponId, this);

        //Send message to the client informing about the new weapon
        AddWeapon();
    }

    protected virtual void Update()
    {
        //update timesincelastshoot
        timeSinceLastShoot += Time.deltaTime;
    }

    private void FixedUpdate()
    {
        //Send the weapon movement if it's not held by any player
        if (!Held)
            SendMovement();
    }

    public virtual void Shoot()
    {

    }

    public virtual void Reload()
    {

    }

    public virtual void PickUp(Transform weaponHolder, Transform camp)
    {
        //Assign some variables
        cam = camp;
        Held = true;

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

        //Send the pick up state to the clients
        SendPickUp();
    }

    public virtual void Drop()
    {
        //Assing some variables
        Held = false;
        Selected = false;

        //Create new rigidbody
        rb = gameObject.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.mass = .1f;

        //Throws the gun
        Vector3 forward = cam.forward;
        forward.y = 0f;
        rb.velocity = forward * throwForce;
        rb.velocity += Vector3.up * throwExtraForce;
        rb.angularVelocity = Random.onUnitSphere * rotationForce;

        //enable colliders and objects
        foreach (Collider col in Colliders)
        {
            col.enabled = true;
        }
        transform.SetParent(null);

        //Send the drop state to the clients
        SendDrop();

        //Set the manager to null
        Manager = null;
    }

    #region Messages

    /// <summary>
    /// Sends a message to update the weapon selection status
    /// </summary>
    /// <param name="select">The weapon selected status</param>
    public void SetSelection(bool select)
    {
        Selected = select;

        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.selectedWeapon);
        message.AddUShort(WeaponId);
        message.AddBool(select);
        message.AddUShort(Manager.player.Id);
        message.AddInt(Helper.GetGunIndexWithType(gunData.Type));

        NetworkManager.Instance.Server.SendToAll(message);
    }

    public void SetSelection(bool select, ushort client_id)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.selectedWeapon);
        message.AddUShort(WeaponId);
        message.AddBool(select);
        message.AddUShort(Manager.player.Id);
        message.AddInt(Helper.GetGunIndexWithType(gunData.Type));

        NetworkManager.Instance.Server.Send(message, client_id);
    }

    /// <summary>
    /// Sends a message to add the weapon to the game
    /// </summary>
    private void AddWeapon()
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.addWeapon);
        message.AddUShort(WeaponId);
        message.AddString(GunData.Name);
        message.AddVector3(transform.position);
        message.AddQuaternion(transform.rotation);
        message.AddInt((int)gunData.Type);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    public void AddWeapon(ushort client_id)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.addWeapon);
        message.AddUShort(WeaponId);
        message.AddString(GunData.Name);
        message.AddVector3(transform.position);
        message.AddQuaternion(transform.rotation);
        message.AddInt((int)gunData.Type);

        NetworkManager.Instance.Server.Send(message, client_id);
    }

    /// <summary>
    /// Sends a message to notify the weapon drop event
    /// </summary>
    private void SendDrop()
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.dropWeapon);
        message.AddUShort(WeaponId);
        message.AddUShort(Manager.player.Id);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    /// <summary>
    /// Sends a message to notify the weapon pickup event
    /// </summary>
    private void SendPickUp()
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.pickUpWeapon);
        message.AddUShort(WeaponId);
        message.AddUShort(Manager.player.Id);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    public void SendPickUp(ushort client_id)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.pickUpWeapon);
        message.AddUShort(WeaponId);
        message.AddUShort(Manager.player.Id);

        NetworkManager.Instance.Server.Send(message, client_id);
    }

    /// <summary>
    /// Sends the weapon movement of the gun
    /// </summary>
    private void SendMovement()
    {
        Message message = Message.Create(MessageSendMode.Unreliable, ServerToClientId.weaponMovement);
        message.AddUShort(WeaponId);
        message.AddUShort(NetworkManager.Instance.CurrentTick);
        message.AddVector3(transform.position);
        message.AddQuaternion(transform.rotation);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    #endregion
}
