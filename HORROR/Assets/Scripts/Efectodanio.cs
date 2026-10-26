using UnityEngine;
using UnityEngine.UI;

// Efecto al recibir daño: los bordes de la pantalla se tiñen de rojo (viñeta) y se desvanecen,
// y la cámara se sacude (con TemblorCamara).
// Va en el jugador (el mismo objeto que VidaJugador). Detecta el daño solo, vigilando la vida.
public class EfectoDanio : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Si se deja vacío usa el VidaJugador del mismo objeto")]
    public VidaJugador vida;

    [Tooltip("Imagen de la viñeta roja que cubre toda la pantalla (en el Canvas)")]
    public Image pantallaRoja;

    [Header("Viñeta roja")]
    [Range(0f, 1f)] public float opacidadMaxima = 0.9f;

    [Tooltip("Segundos que tarda en desvanecerse")]
    public float duracionDestello = 1f;

    [Header("Temblor de cámara")]
    [Tooltip("Grados que se inclina la cámara con el golpe")]
    public float anguloTemblor = 4f;

    [Tooltip("Metros que se mueve la cámara con el golpe")]
    public float desplazamientoTemblor = 0.05f;

    [Tooltip("Segundos que dura el temblor")]
    public float duracionTemblor = 0.4f;

    float vidaAnterior;
    float restanteDestello;

    void Start()
    {
        if (vida == null) vida = GetComponent<VidaJugador>();
        if (vida == null) vida = FindAnyObjectByType<VidaJugador>();

        if (vida == null || pantallaRoja == null)
        {
            Debug.LogError("EfectoDanio: faltan referencias (VidaJugador o la imagen de la viñeta).", this);
            enabled = false;
            return;
        }

        if (TemblorCamara.instance == null)
        {
            Debug.LogWarning("EfectoDanio: no hay TemblorCamara en el jugador, la cámara no temblará.", this);
        }

        // La imagen no debe bloquear los clics del mouse
        pantallaRoja.raycastTarget = false;
        if (!pantallaRoja.gameObject.activeSelf) pantallaRoja.gameObject.SetActive(true);
        PonerOpacidad(0f);

        vidaAnterior = vida.VidaActual;
    }

    void Update()
    {
        // Si la vida bajó desde el frame anterior, hubo daño
        if (vida.VidaActual < vidaAnterior)
        {
            Activar();
        }
        vidaAnterior = vida.VidaActual;

        // Tiempo "sin escala": así el efecto también se ve en el golpe que mata
        if (restanteDestello > 0f)
        {
            restanteDestello -= Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(restanteDestello / Mathf.Max(0.01f, duracionDestello));
            PonerOpacidad(opacidadMaxima * t);
        }
    }

    // Público por si quieres activarlo desde otro script
    public void Activar()
    {
        restanteDestello = duracionDestello;

        if (TemblorCamara.instance != null)
        {
            TemblorCamara.instance.Temblar(anguloTemblor, desplazamientoTemblor, duracionTemblor);
        }
    }

    void PonerOpacidad(float alfa)
    {
        Color c = pantallaRoja.color;
        c.a = alfa;
        pantallaRoja.color = c;
    }

    // Para probarlo sin esperar al fantasma: clic derecho en el título del componente (en modo Play)
    [ContextMenu("Probar efecto")]
    void Probar()
    {
        Activar();
    }
}