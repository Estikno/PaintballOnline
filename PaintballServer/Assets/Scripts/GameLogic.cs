using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public Transform[] RespawnPoints => respawnPoints;

    [Header("Prefabs")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Other")]
    [SerializeField] private Transform[] respawnPoints;

    private void Awake()
    {
        Instance = this;
    }
}
