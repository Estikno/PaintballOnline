using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[Serializable]
public struct NameWeapon
{
    public string Name;
    public GameObject prefab;
}

public class GameLogic : MonoBehaviour
{
    private static GameLogic instance;
    public static GameLogic Instance
    {
        get => instance;
        private set
        {
            if (instance == null)
            {
                instance = value;
            }
            else if (instance != null)
            {
                Debug.Log($"{nameof(GameLogic)} instance already exists, destroying duplicate!");
                Destroy(value);
            }
        }
    }

    public GameObject PlayerPrefab => playerPrefab;
    public GameObject LocalPlayerPrefab => localPlayerPrefab;
    public NameWeapon[] Weapons => weapons;
    public GameObject BulletImpactEffect => bulletImpactEffect;
    public TMP_Text CurrentAmmoText => currentAmmoText;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject localPlayerPrefab;
    [SerializeField] private NameWeapon[] weapons;

    [Header("Effects")]
    [SerializeField] private GameObject bulletImpactEffect;

    [Header("UIs")]
    [SerializeField] private TMP_Text currentAmmoText;

    private void Awake()
    {
        Instance = this;
    }
}
