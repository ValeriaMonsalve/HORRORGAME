using UnityEngine;

// Va en la MiniMapCamera. La mantiene sobre el jugador, mirando hacia abajo.
// La altura (Y) y el tamaño del mapa (Orthographic Size) los sigues controlando tú desde la cámara.
public class MiniMapaCamara : MonoBehaviour
{
    [Tooltip("El jugador. Si se deja vacío busca el que tiene el script VidaJugador")]
    public Transform jugador;

    [Tooltip("Marcado: el mapa gira para que 'arriba' sea siempre hacia donde miras. Desmarcado: el norte queda fijo")]
    public bool girarConJugador = false;

    void Start()
    {
        if (jugador == null)
        {
            VidaJugador vida = FindAnyObjectByType<VidaJugador>();
            if (vida != null) jugador = vida.transform;
        }

        if (jugador == null)
        {
            Debug.LogError("MiniMapaCamara: no encontré al jugador. Arrástralo al campo Jugador.", this);
            enabled = false;
        }
    }

    // LateUpdate: así se coloca después de que el jugador se movió en este frame
    void LateUpdate()
    {
        Vector3 posicion = transform.position;
        posicion.x = jugador.position.x;
        posicion.z = jugador.position.z;
        transform.position = posicion;

        float giro = girarConJugador ? jugador.eulerAngles.y : 0f;
        transform.rotation = Quaternion.Euler(90f, giro, 0f);
    }
}