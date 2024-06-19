using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform camHolder;
    public Player player;

    [Header("Weapon")]
    [SerializeField] private int weaponLayer;
    [SerializeField] private float pickUpRange;
    [SerializeField] private float pickUpRadius;

    public IWeapon[] weapons { get; private set; } = new IWeapon[1];

    public int SelectedWeapon { get; private set; } = 0;

    public GameObject basicGun;
    /*public GameObject glock;
    public GameObject knife;*/

    private void Start()
    {
        Invoke(nameof(pick), 2f);
    }

    private void pick()
    {
        IWeapon weapon = Instantiate(basicGun, new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<IWeapon>();
        weapon.PickUp(transform, camHolder);
        weapons[0] = weapon;

        /*IWeapon _weapon = Instantiate(ak, new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<IWeapon>();
        _weapon.PickUp(transform, CameraHolder);
        weapons[0] = _weapon;

        IWeapon weapon2 = Instantiate(knife, new Vector3(0f, 1f, 0f), Quaternion.identity).GetComponent<IWeapon>();
        weapon2.PickUp(transform, CameraHolder);
        weapons[2] = weapon2;*/

        SetSelectedWeapon(Helper.GetGunIndexWithType(GunType.rifle));
    }

    public void PrimaryUsePressed()
    {
        if (weapons[SelectedWeapon] != null)
            weapons[SelectedWeapon].Shoot();
    }

    public void DropWeapon()
    {
        if (weapons[SelectedWeapon] == null)
            return;

        if (weapons[SelectedWeapon].GunData.Type == GunType.knife)
            return;

        weapons[SelectedWeapon].Drop();
        weapons[SelectedWeapon] = null;

        SetSelectedWeapon(Helper.GetGunIndexWithType(GunType.rifle));
    }

    public void PickWeapon()
    {
        IWeapon weaponToPick = GetPickUpWeapon();

        if (weaponToPick == null)
            return;

        GunType weaponType = weaponToPick.GunData.Type;

        if (weapons[(int)weaponType] != null)
        {
            weapons[(int)weaponType].Drop();
            weaponToPick.PickUp(transform, camHolder);
        }
        else
        {
            weaponToPick.PickUp(transform, camHolder);
        }

        weapons[Helper.GetGunIndexWithType(weaponType)] = weaponToPick;
        SetSelectedWeapon(Helper.GetGunIndexWithType(weaponType));
    }

    public void SwitchWeapon(int _switch)
    {
        if (weapons[_switch] == null)
            return;

        SetSelectedWeapon(_switch);
    }

    public void ReloadWeapon()
    {
        if (weapons[(int)SelectedWeapon] != null)
            weapons[(int)SelectedWeapon].Reload();
    }

    private IWeapon GetPickUpWeapon()
    {
        //pick up weapon
        RaycastHit[] hitList = new RaycastHit[256]; //increase number if there are a lot of objects in the scene
        int hitNumber = Physics.CapsuleCastNonAlloc(camHolder.position, camHolder.position + camHolder.forward * pickUpRange, pickUpRadius, camHolder.forward, hitList);

        List<RaycastHit> rayList = new List<RaycastHit>();
        for (int i = 0; i < hitNumber; i++)
        {
            RaycastHit hit = hitList[i];

            if (hit.transform.gameObject.layer != weaponLayer) continue; //if its not a gun continue the loop

            if (hit.point == Vector3.zero)
            {
                rayList.Add(hit);
            }
            else if (Physics.Raycast(camHolder.position, hit.point - camHolder.position, out var hitInfo, hit.distance + 0.1f) && hitInfo.transform == hit.transform)
            {
                rayList.Add(hit);
            }
        }

        if (rayList.Count == 0) return null; //if there is no weapon return

        rayList.Sort((hit1, hit2) =>
        {
            //get the distances
            float dist1 = getDistance(hit1);
            float dist2 = getDistance(hit2);

            return Mathf.Abs(dist1 - dist2) < 0.001f ? 0 : dist1 < dist2 ? -1 : 1; //return 0, -1 or 1
        });

        //re-assign variables
        return rayList[0].transform.GetComponent<IWeapon>();
    }

    private float getDistance(RaycastHit hit)
    {
        return Vector3.Distance(camHolder.position, hit.point == Vector3.zero ? hit.transform.position : hit.point);
    }

    private void SetSelectedWeapon(int _weapon)
    {
        if (weapons[SelectedWeapon] != null)
            weapons[SelectedWeapon].SetSelection(false);

        if (weapons[_weapon] != null)
        {
            SelectedWeapon = _weapon;
            weapons[_weapon].SetSelection(true);
        }
    }
}
