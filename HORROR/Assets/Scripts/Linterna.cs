using UnityEngine;
using UnityEngine.UI;

// Linterna con la tecla F y batería.
// - Encendida gasta batería: se agota tras "duracionBateria" segundos de uso.
// - Apagada (o agotada) se recarga: de 0 a 100% en "tiempoRecarga" segundos.
// - Al agotarse se apaga sola y no se puede encender hasta recuperar "cargaParaReactivar".
// Va en el jugador; la Spot Light debe ser HIJA de la cámara para que apunte hacia donde mira el jugador.
public class Linterna : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Spot Light de la linterna. Si se deja vacía, usa la primera luz hija del jugador")]
    public Light luz;

    [Header("Ajustes")]
    public KeyCode tecla = KeyCode.F;
    public bool empiezaEncendida = false;

    [Header("Batería")]
    [Tooltip("Segundos de uso hasta agotar la batería completa")]
    public float duracionBateria = 30f;

    [Tooltip("Segundos que tarda en recargarse de 0 a 100% (se recarga mientras está apagada)")]
    public float tiempoRecarga = 120f;

    [Tooltip("Carga (0 a 1) que necesita para poder encenderla otra vez tras agotarse. 1 = recarga completa")]
    [Range(0f, 1f)] public float cargaParaReactivar = 1f;

    [Tooltip("Barra de batería del HUD (opcional)")]
    public Slider barraBateria;

    [Header("Sonido (opcional)")]
    [Tooltip("Nombre de un audio del AudioManager que suena al presionar F. Vacío = sin sonido")]
    public string sonidoInterruptor = "";

    public bool Encendida { get; private set; }

    // Carga actual de 0 (vacía) a 1 (llena)
    public float Carga { get; private set; }

    // True desde que se vacía hasta que recupera "cargaParaReactivar"
    public bool Agotada { get; private set; }

    void Start()
    {
        if (luz == null) luz = GetComponentInChildren<Light>();

        if (luz == null)
        {
            Debug.LogError("Linterna: asigna la Spot Light en el Inspector.", this);
            enabled = false;
            return;
        }

        Carga = 1f;
        Agotada = false;

        if (barraBateria != null)
        {
            barraBateria.minValue = 0f;
            barraBateria.maxValue = 1f;
        }

        Establecer(empiezaEncendida, false);
        ActualizarBarra();
    }

    void Update()
    {
        // Sin linterna ni consumo de batería en pausa ni en game over
        if (Time.timeScale == 0f) return;

        if (Input.GetKeyDown(tecla))
        {
            Alternar();
        }

        if (Encendida)
        {
            // Gasta batería
            Carga -= Time.deltaTime / Mathf.Max(0.01f, duracionBateria);

            if (Carga <= 0f)
            {
                Carga = 0f;
                Agotada = true;
                Establecer(false, false);
            }
        }
        else
        {
            // Se recarga mientras está apagada
            Carga = Mathf.Min(1f, Carga + Time.deltaTime / Mathf.Max(0.01f, tiempoRecarga));

            if (Agotada && Carga >= cargaParaReactivar)
            {
                Agotada = false;
            }
        }

        ActualizarBarra();
    }

    void Alternar()
    {
        if (Encendida)
        {
            Establecer(false, true);
            return;
        }

        // No se puede encender si se agotó y aún no se recarga lo suficiente, o si no queda carga
        if (Agotada || Carga <= 0f) return;

        Establecer(true, true);
    }

    // Público por si otra mecánica necesita encenderla o apagarla
    public void Establecer(bool encendida, bool conSonido)
    {
        Encendida = encendida;
        luz.enabled = encendida;

        if (conSonido) ReproducirSonido();
    }

    void ActualizarBarra()
    {
        if (barraBateria != null)
        {
            barraBateria.value = Carga;
        }
    }

    void ReproducirSonido()
    {
        if (string.IsNullOrEmpty(sonidoInterruptor)) return;

        AudioManager am = AudioManager.instance;
        if (am == null) return;

        // Se comprueba antes porque AudioManager.Play() da error si el nombre no existe
        if (System.Array.Exists(am.audios, a => a.nombre == sonidoInterruptor))
        {
            am.Play(sonidoInterruptor);
        }
        else
        {
            Debug.LogWarning("Linterna: el audio '" + sonidoInterruptor + "' no existe en el AudioManager.", this);
        }
    }
}