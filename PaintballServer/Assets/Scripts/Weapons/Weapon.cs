using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public static Dictionary<ushort, Gun> list = new Dictionary<ushort, Gun>();

    public ushort Id { get; private set; }
    public static ushort ActualId { get; private set; } = 0;

    [Header("OBJ References")]
    [SerializeField] protected Transform camHolder;
    [SerializeField] protected Player player;

    //variable to keep track of the fire rate
    protected float timeSinceLastShoot;

    protected virtual void Awake()
    {
        Id = ActualId;
        ActualId++;
    }

    protected virtual void Update()
    {
        //update timesincelastshoot
        timeSinceLastShoot += Time.deltaTime;
    }

    public virtual void Shoot()
    {

    }
}
