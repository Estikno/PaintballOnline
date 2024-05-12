using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public enum TypeOfSound
{
    soundEffect,
    backgroundSound,
    music
}

[System.Serializable]
public class Sound //a class used in the audioManager
{
    public string name;

    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;

    public bool loop;

    [Header("Type of sound")]
    public TypeOfSound type;

    [HideInInspector]
    public AudioSource source;
}
