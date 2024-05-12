using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] audioContainers; //0: sound effects, 1: background sounds, 2: background music
    [SerializeField]
    private AudioMixerGroup[] mixers; //0: sound effects, 1: background sounds, 2: background music
    [SerializeField]
    private GameObject OnlyOneUseAudioObject; //a gameobject template used for plating 3d sounds

    [SerializeField]
    private Sound[] sounds; //all the sounds of the game with 2d volume

    private static AudioManager _instance;

    public static AudioManager Instance
    {
        get => _instance;
        set
        {
            if (_instance == null)
                _instance = value;
            else if (_instance != value)
            {
                Debug.Log($"{nameof(AudioManager)} instance already exists, dstroying duplicate!");
                Destroy(value);
            }
        }
    }

    private void Awake()
    {
        Instance = this;

        foreach (Sound s in sounds)
        {
            //distribute each type of sound to a specific gameobject
            s.source = audioContainers[(ushort)s.type].AddComponent<AudioSource>();
            s.source.outputAudioMixerGroup = mixers[(ushort)s.type];

            //set the values to the audiosource
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;
            s.source.loop = s.loop;
        }
    }

    public void Play(string name, bool oneShot) //play a designated audio
    {
        Sound s = Array.Find(sounds, sound => sound.name == name); //find the audio in the list

        if (s == null) //check if the audio is not null
        {
            Debug.LogError("Sound: " + name + " not found");
        }

        if (oneShot) //play the audio in oneshot or normally
        {
            s.source.PlayOneShot(s.clip);
        }
        else
        {
            s.source.Play();
        }
    }

    public void Stop(string name) //stop a playing audio
    {
        Sound s = Array.Find(sounds, sound => sound.name == name); //find the audio in the list

        if (s == null) //check if the audio is not null
        {
            Debug.LogError("Sound: " + name + " not found");
        }

        s.source.Stop(); //stop the designated audio
    }

    public bool isPlaying(string name) //returns true if an audio is playing and false if not
    {
        Sound s = Array.Find(sounds, sound => sound.name == name); //find the audio in the list

        if (s == null) //check if the audio is not null
        {
            Debug.LogError("Sound: " + name + " not found");
        }

        return s.source.isPlaying; //return the isPlaying state
    }

    /// <summary>
    /// Creates an object that plays an audio in 3D space only once
    /// </summary>
    /// <param name="sound_info"></param>
    /// <param name="pos"></param>
    /// <param name="minDistance"></param>
    /// <param name="maxDistance"></param>
    public void PlayAudioIn3DSpace(string name, Vector3 pos, float minDistance, float maxDistance)
    {
        //instantiate the object and get the audio source component
        Sound sound_info = Array.Find(sounds, sound => sound.name == name);

        if (sound_info == null) Debug.LogError($"Sound: {name} not found");

        GameObject audioObject = Instantiate(OnlyOneUseAudioObject, pos, Quaternion.identity);
        AudioSource sourceOBJ = audioObject.GetComponent<AudioSource>();

        //assign the wanted values to the audio source
        sourceOBJ.clip = sound_info.clip;
        sourceOBJ.volume = sound_info.volume;
        sourceOBJ.pitch = sound_info.pitch;
        sourceOBJ.loop = sound_info.loop;
        sourceOBJ.minDistance = minDistance;
        sourceOBJ.maxDistance = maxDistance;

        //play the audio
        sourceOBJ.Play();

        //make the object destroy after a few seconds
        Destroy(audioObject, sound_info.clip.length);
    }
}
