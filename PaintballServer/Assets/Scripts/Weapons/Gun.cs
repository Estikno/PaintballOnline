using Riptide;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : Weapon
{
    //Variables needed for shooting
    private bool reloading;
    private int currentAmmo;

    [Header("Other")]
    [SerializeField] private Transform gunPoint;
    [SerializeField] private LayerMask whatToHit;

    protected override void Awake()
    {
        base.Awake();

        //always make sure reloading is false at start so avoid bugs
        reloading = false;

        //reload all ammo at start
        currentAmmo = GunData.MagSize;
    }

    public override void Reload()
    {
        //excecute if is no realoading, if is held, selected and the ammo is smaller than the mag size
        if (!reloading && currentAmmo != GunData.MagSize)
        {
            //start the reloading corroutine
            StartCoroutine(_Reload());
        }
    }

    //actual reloading function
    private IEnumerator _Reload()
    {
        //Send reload message
        SendReload(false);

        //set variables and anim
        reloading = true;

        //wait for the reload time
        yield return new WaitForSeconds(GunData.ReloadTime);

        //set the reloading to false
        reloading = false;

        //Send reload message
        SendReload(true);

        //set the ammo to the mag size
        currentAmmo = GunData.MagSize;
    }


    //this method verifies if we can shoot
    private bool canShoot() => !reloading && timeSinceLastShoot > 1f / (GunData.FireRate / 60f) && currentAmmo > 0;

    public override void Shoot()
    {
        if (canShoot())
        {
            if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hitInfo, GunData.MaxDistance, whatToHit))
            {
                //print(hitInfo.transform.name);
                //damage the player
                Player player = hitInfo.transform.GetComponentInParent<Player>();

                if (player != null)
                {
                    //damage the player
                    player.Damage(GunData.Damage);
                }

                //sends the shot
                SendShoot(hitInfo.point, hitInfo.normal);
            }
            else
            {
                SendShoot();
            }

            //update variables
            currentAmmo--;
            timeSinceLastShoot = 0;
        }
    }

    #region Messages

    //Sends a shoot message
    private void SendShoot(Vector3 hitPoint, Vector3 normal)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.weaponShoot);
        message.AddUShort(Manager.player.Id);
        message.AddBool(true);
        message.AddVector3(hitPoint);
        message.AddVector3(normal);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    private void SendShoot()
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.weaponShoot);
        message.AddUShort(Manager.player.Id);
        message.AddBool(false);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    //Sends the reload messages
    private void SendReload(bool isFinished)
    {
        Message message = Message.Create(MessageSendMode.Reliable, ServerToClientId.reloadWeapon);
        message.AddUShort(Manager.player.Id);
        message.AddBool(isFinished);
        message.AddFloat(GunData.ReloadTime);

        NetworkManager.Instance.Server.SendToAll(message);
    }

    #endregion
}
