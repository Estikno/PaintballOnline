using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public static readonly Dictionary<ushort, Bullet> list = new Dictionary<ushort, Bullet>();
    public static ushort NextBullet = 1;

    public ushort BulletId { get; private set; }

    [Header("Values")]
    [SerializeField] private float velocity;

    private Vector3 dir;

    public void Initiate(Vector3 dir)
    {
        this.dir = dir;
    }

    private void Awake()
    {
        BulletId = NextBullet;
        NextBullet++;
        list.Add(BulletId, this);
    }

    private void Update()
    {
        transform.Translate(dir * velocity * Time.deltaTime);
    }

    private void OnDestroy()
    {
        list.Remove(BulletId);
    }

    #region Messages

    [MessageHandler((ushort)ServerToClientId.destroyBullet)]
    private static void DestroyBullet(Message message)
    {
        if(list.TryGetValue(message.GetUShort(), out Bullet bullet))
        {
            Debug.Log("IMPACT!!!!");
            Destroy(bullet.gameObject);

            if (message.GetBool())
            {
                float damage = message.GetFloat();
                Transform impactEffect = Instantiate(GameLogic.Instance.BulletImpactEffect, message.GetVector3(), Quaternion.identity).transform;
                impactEffect.forward = message.GetVector3();
            }
        }
    }

    #endregion
}
