using Riptide;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

public class Gun : Weapon
{
    [Header("References")]
    [SerializeField] protected GunData gunData;

    protected override void Awake()
    {
        base.Awake();

        list.Add(Id, this);

        //always make sure reloading is false at start so avoid bugs
        gunData.reloading = false;
    }

    public void StartReload() //the function that checks and excecutes the reload
    {
        //excecute if is no realoading, if is held, selected and the ammo is smaller than the mag size
        if (!gunData.reloading && gunData.currentAmmo != gunData.magSize)
        {
            //start the reloading corroutine
            StartCoroutine(Reload());
        }
    }

    //actual reloading function
    private IEnumerator Reload()
    {
        //set variables and anim
        gunData.reloading = true;

        //send reload message
        SendReload(false);

        //wait for the reload time
        yield return new WaitForSeconds(gunData.reloadTime);

        //set the ammo to the mag size
        gunData.currentAmmo = gunData.magSize;

        //set the reloading to false
        gunData.reloading = false;

        //send reload message
        SendReload(true);
    }

    //this method verifies if we can shoot
    protected bool canShoot() => !gunData.reloading && timeSinceLastShoot > 1f / (gunData.fireRate / 60f) && gunData.currentAmmo > 0;

    public override void Shoot()
    {
        if (canShoot())
        {
            //traces a raycast to see with what collides the shot
            if (Physics.Raycast(camHolder.position, camHolder.forward, out RaycastHit hitInfo, gunData.maxDistance))
            {
                //get the object script and apply the damage
                //IDamageable damageable = hitInfo.transform.GetComponent<IDamageable>();
                //damageable?.TakeDamage(gunData.damage);
                Debug.Log("shoot!!!");
            }

            //update variables
            gunData.currentAmmo--;
            timeSinceLastShoot = 0;
        }
    }

    #region Messages

    private void SendReload(bool hasReloaded)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.playerReload);
        message.AddUShort(player.Id);
        message.AddBool(hasReloaded);
        message.AddFloat(gunData.reloadTime);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    private void SendShoot()
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.playerShoot);
        message.AddUShort(player.Id);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    #endregion
}
