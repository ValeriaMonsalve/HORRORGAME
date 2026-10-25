using UnityEngine;

// Reproduce un audio de tu AudioManager sin dar error si el nombre no existe
// o si la escena no tiene AudioManager. Un nombre vacío simplemente no suena.
public static class SonidoSeguro
{
    public static void Reproducir(string nombre)
    {
        if (string.IsNullOrEmpty(nombre)) return;

        AudioManager am = AudioManager.instance;
        if (am == null) return;

        if (System.Array.Exists(am.audios, a => a.nombre == nombre))
        {
            am.Play(nombre);
        }
        else
        {
            Debug.LogWarning("SonidoSeguro: el audio '" + nombre + "' no existe en el AudioManager.");
        }
    }
}