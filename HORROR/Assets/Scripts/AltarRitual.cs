using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// Va en el altar del sótano: un objeto en el piso con un Collider delgado que NO sea Trigger
// (por ejemplo un Box Collider plano que cubra el círculo del ritual).
// Cada vez que el jugador lo mira y presiona E, coloca un objeto de los que lleva.
// Cuando todas las ranuras están ocupadas, se hace el ritual.
public class AltarRitual : MonoBehaviour, IInteractuable
{
    [System.Serializable]
    public class Ranura
    {
        [Tooltip("Tipo de objeto que va aquí (igual al 'Tipo' del objeto recogible)")]
        public string tipo;

        [Tooltip("Modelo ya puesto en su lugar del piso, DESACTIVADO. Se activa al colocar el objeto")]
        public GameObject visual;

        [HideInInspector] public bool colocada;
    }

    [Header("Ritual")]
    [Tooltip("Todas las ranuras del ritual. Puedes repetir el mismo tipo (por ejemplo 5 ranuras de Vela)")]
    public Ranura[] ranuras;

    [Tooltip("Marcado: los objetos solo se pueden colocar en el orden de la lista. Desmarcado: en cualquier orden")]
    public bool ordenEstricto = false;

    [Header("Sonidos (opcionales)")]
    public string sonidoColocar = "";
    public string sonidoRitual = "";

    [Header("Al completarse el ritual")]
    [Tooltip("Aquí puedes encender luces, activar objetos, etc. sin programar")]
    public UnityEvent alCompletarRitual;

    [Tooltip("Escena a cargar después del ritual (por ejemplo la del Inframundo). Vacío = no cambia de escena")]
    public string escenaSiguiente = "";

    [Tooltip("Segundos de espera antes de cambiar de escena")]
    public float retardo = 4f;

    [Header("Sacudida de cámara del ritual")]
    [Tooltip("Grados que se inclina la cámara. Más alto = más brusco")]
    public float anguloTemblor = 10f;

    [Tooltip("Metros que se mueve la cámara")]
    public float desplazamientoTemblor = 0.25f;

    [Tooltip("Segundos que dura. Mantén el Retardo igual o mayor para que se vea antes del cambio de escena")]
    public float duracionTemblor = 3f;

    bool completado;
    int colocadas;

    void Start()
    {
        int cantidadRanuras = ranuras == null ? 0 : ranuras.Length;

        if (InventarioRitual.instance != null && InventarioRitual.instance.totalObjetos != cantidadRanuras)
        {
            Debug.LogWarning("AltarRitual: el altar tiene " + cantidadRanuras + " ranuras pero InventarioRitual espera "
                + InventarioRitual.instance.totalObjetos + " objetos. Deberían coincidir.", this);
        }
    }

    public string TextoInteraccion
    {
        get
        {
            if (completado) return "";

            int i = BuscarRanuraColocable();
            if (i >= 0) return "Colocar " + ranuras[i].tipo;

            return "Place the ritual objects";
        }
    }

    public void Interactuar()
    {
        if (completado || ranuras == null) return;

        int i = BuscarRanuraColocable();

        if (i < 0)
        {
            // Mensaje neutro: no revela el orden correcto
            Aviso.Mostrar(ordenEstricto ? "Something isn't quite right..." : "You are still missing items");
            return;
        }

        Ranura r = ranuras[i];
        InventarioRitual.instance.Quitar(r.tipo);
        r.colocada = true;
        colocadas++;

        if (r.visual != null) r.visual.SetActive(true);

        SonidoSeguro.Reproducir(sonidoColocar);
        Aviso.Mostrar("You placed: " + r.tipo + " (" + colocadas + "/" + ranuras.Length + ")");

        if (colocadas >= ranuras.Length)
        {
            StartCoroutine(RealizarRitual());
        }
    }

    // Devuelve la ranura que se puede llenar ahora con lo que lleva el jugador, o -1 si ninguna
    int BuscarRanuraColocable()
    {
        if (ranuras == null || InventarioRitual.instance == null) return -1;

        for (int i = 0; i < ranuras.Length; i++)
        {
            if (ranuras[i].colocada) continue;

            if (InventarioRitual.instance.Cantidad(ranuras[i].tipo) > 0) return i;

            // En orden estricto solo cuenta la primera ranura libre
            if (ordenEstricto) return -1;
        }

        return -1;
    }

    IEnumerator RealizarRitual()
    {
        completado = true;

        // Durante el ritual ningún fantasma puede matar al jugador antes del cambio de escena
        VidaJugador vida = FindAnyObjectByType<VidaJugador>();
        if (vida != null) vida.Invulnerable = true;

        // Sacudida brusca de la cámara
        if (TemblorCamara.instance != null)
        {
            TemblorCamara.instance.Temblar(anguloTemblor, desplazamientoTemblor, duracionTemblor);
        }

        SonidoSeguro.Reproducir(sonidoRitual);

        if (alCompletarRitual != null)
        {
            alCompletarRitual.Invoke();
        }

        if (!string.IsNullOrEmpty(escenaSiguiente))
        {
            yield return new WaitForSeconds(retardo);
            SceneManager.LoadScene(escenaSiguiente);
        }
    }
}