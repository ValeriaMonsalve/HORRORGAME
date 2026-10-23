using UnityEngine;
using UnityEngine.Audio;

[System.Serializable]
public class Audio
{
    public string nombre;
    public AudioClip audioFile;

    [Range(0f, 50f)]
    public float volume;

    [Range(0.1f, 2f)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public AudioSource source;

}
