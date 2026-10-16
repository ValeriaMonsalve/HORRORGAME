using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


public class AudioManager : MonoBehaviour
{
    public Audio[] audios;
    public static AudioManager instance;

    public Slider volumeSlider;
    float volValue;
    float lastVolValue;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        foreach (Audio a in audios)
        {
            a.source = gameObject.AddComponent<AudioSource>();
            a.source.clip = a.audioFile;
            a.source.volume = a.volume;
            a.source.pitch = a.pitch;
            a.source.loop = a.loop;
        }
    }

    void Start()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("Volumen");
        volValue = volumeSlider.value;

        Play("BackGroundSound");


    }

    void Update()
    {
        volValue = volumeSlider.value;
        if (volValue != lastVolValue)
        {
            CambiarVolumen();
        }

        lastVolValue = volValue;
    }

    public void CambiarVolumen()
    {
        AudioListener.volume = volValue;
        PlayerPrefs.SetFloat("Volumen", volValue);
        PlayerPrefs.Save();
    }

    public void Play(string nombre)
    {
        Audio a = Array.Find(audios, audio => audio.nombre == nombre);
        if (a == null)
        {
            Debug.LogWarning("El nombre del archivo: " + nombre + " no existe");
        }
        a.source.Play();
    }

    public void Stop(string nombre)
    {
        Audio a = Array.Find(audios, audio => audio.nombre == nombre);
        a.source.Stop();
    }

}
