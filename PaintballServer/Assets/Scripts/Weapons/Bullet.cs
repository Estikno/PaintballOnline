using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public static readonly Dictionary<ushort, Bullet> list = new Dictionary<ushort, Bullet>();
    public static ushort NextBullet = 1;

    public ushort BulletId {  get; private set; }

    [Header("Values")]
    [SerializeField] private float velocity;
    [Range(1f, 2f)]
    [SerializeField] private float timeThreshold = 1.2f;

    private float damage;
    private string gunName;
    private Vector3 dir;
    private Vector3 currentPos;

    public void Initiate(float maxDistance, float damage, string gunName, Vector3 dir)
    {
        this.damage = damage;
        this.gunName = gunName;
        this.dir = dir;

        currentPos = transform.position;

        Invoke(nameof(destroy), (maxDistance / velocity) * timeThreshold);
    }

    private void Awake()
    {
        BulletId = NextBullet;
        NextBullet++;
        list.Add(BulletId, this);
    }

    private void Update()
    {
        currentPos = transform.position;

        transform.Translate(dir * velocity * Time.deltaTime);
    }

    private void LateUpdate()
    {
        if(Physics.Raycast(currentPos, dir, out RaycastHit hit, Vector3.Distance(currentPos, transform.position)))
        {
            //has collided with sth

            //IDamageable damageable = hit.transform.GetComponent<IDamageable>();
            //damageable?.TakeDamage(GunData.Damage);

            destroy(hit.point, hit.normal);
        }

        if (currentPos == transform.position) Debug.LogWarning($"The last frame position ({currentPos}) is the same as the current one ({transform.position})");
    }

    private void destroy()
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.destroyBullet);
        message.AddUShort(BulletId);
        //the bool is true if it has hitted sth
        message.AddBool(false);

        NetworkManager.Instance.Server.SendToAll(message);

        Destroy(gameObject);
    }

    private void destroy(Vector3 hitPosition, Vector3 normal)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.destroyBullet);
        message.AddUShort(BulletId);
        //the bool is true if it has hitted sth
        message.AddBool(true);
        message.AddFloat(damage);
        message.AddVector3(hitPosition);
        message.AddVector3(normal);

        NetworkManager.Instance.Server.SendToAll(message);

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        list.Remove(BulletId);
    }
}
