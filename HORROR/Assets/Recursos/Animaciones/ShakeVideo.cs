using UnityEngine;
using UnityEngine.Video;

// Sacude la imagen donde se reproduce un video (una RawImage del Canvas) con golpes bruscos.
// Va en la RawImage que muestra el video. Los golpes pueden ocurrir en segundos concretos del video
// o al azar. Cada golpe mueve, inclina y hace un pequeño zoom a la imagen, y se apaga solo.
public class ShakeVideo : MonoBehaviour
{
    [System.Serializable]
    public class Sacudida
    {
        [Tooltip("Segundo del video en el que ocurre el golpe")]
        public float segundo;

        [Tooltip("Fuerza del golpe: 1 = normal, 2 = muy fuerte")]
        public float fuerza = 1f;

        [Tooltip("Cuántos segundos dura el golpe")]
        public float duracion = 0.5f;

        [HideInInspector] public bool disparada;
    }

    [Header("Referencias")]
    [Tooltip("La imagen donde se ve el video. Si se deja vacío usa el RectTransform de este mismo objeto")]
    public RectTransform imagenVideo;

    [Tooltip("El VideoPlayer. Si se deja vacío busca uno en la escena")]
    public VideoPlayer video;

    [Header("Golpes en momentos concretos")]
    public Sacudida[] sacudidas;

    [Header("Golpes al azar (opcional)")]
    public bool sacudidasAleatorias = false;
    public float intervaloMin = 3f;
    public float intervaloMax = 7f;
    public float fuerzaMin = 0.6f;
    public float fuerzaMax = 1.6f;

    [Header("Intensidad (con fuerza = 1)")]
    [Tooltip("Cuánto se mueve la imagen, en unidades del Canvas")]
    public float desplazamiento = 40f;

    [Tooltip("Grados que se inclina la imagen")]
    public float angulo = 3f;

    [Tooltip("Zoom extra durante el golpe (0.12 = 12%). Sirve también para que no se vean los bordes")]
    public float zoom = 0.12f;

    [Tooltip("Velocidad del temblor: más alto = más nervioso")]
    public float frecuencia = 25f;

    [Header("Ayuda para ponerle tiempos (solo lectura)")]
    [Tooltip("Segundo actual del video. Pausa el juego y anota el valor donde quieras un golpe")]
    [SerializeField] float tiempoActual;

    Vector2 posBase;
    Quaternion rotBase;
    Vector3 escalaBase;

    float fuerzaGolpe;
    float duracionGolpe = 1f;
    float restante;
    float semilla;
    float proximoAleatorio;
    bool aplicado;

    void Start()
    {
        if (imagenVideo == null) imagenVideo = transform as RectTransform;
        if (video == null) video = FindAnyObjectByType<VideoPlayer>();

        if (imagenVideo == null)
        {
            Debug.LogError("TemblorVideo: ponlo en una RawImage del Canvas o asigna la imagen del video.", this);
            enabled = false;
            return;
        }

        if (video == null && (sacudidas != null && sacudidas.Length > 0))
        {
            Debug.LogWarning("TemblorVideo: no encontré ningún VideoPlayer; los golpes por segundo no funcionarán.", this);
        }

        posBase = imagenVideo.anchoredPosition;
        rotBase = imagenVideo.localRotation;
        escalaBase = imagenVideo.localScale;

        ProgramarAleatorio();
    }

    void Update()
    {
        bool reproduciendo = video != null && video.isPlaying;

        if (video != null) tiempoActual = (float)video.time;

        if (reproduciendo && sacudidas != null)
        {
            foreach (Sacudida s in sacudidas)
            {
                if (!s.disparada && video.time >= s.segundo)
                {
                    s.disparada = true;

                    // Si se saltó el video hacia adelante, no dispara golpes atrasados
                    if (video.time - s.segundo < 1f)
                    {
                        Sacudir(s.fuerza, s.duracion);
                    }
                }
                else if (s.disparada && video.time < s.segundo - 0.5f)
                {
                    // El video volvió a empezar o retrocedió: el golpe puede ocurrir otra vez
                    s.disparada = false;
                }
            }
        }

        if (sacudidasAleatorias && (video == null || reproduciendo) && Time.time >= proximoAleatorio)
        {
            Sacudir(Random.Range(fuerzaMin, fuerzaMax), Random.Range(0.3f, 0.6f));
            ProgramarAleatorio();
        }
    }

    // LateUpdate: se aplica al final del frame
    void LateUpdate()
    {
        if (restante > 0f)
        {
            restante -= Time.unscaledDeltaTime;

            // Cae rápido: fuerte al inicio del golpe y se apaga enseguida
            float k = Mathf.Clamp01(restante / duracionGolpe);
            float fuerza = k * k * fuerzaGolpe;
            float t = Time.unscaledTime * frecuencia;

            Vector2 offset = new Vector2(Ruido(semilla, t), Ruido(semilla + 10f, t)) * desplazamiento * fuerza;
            float giro = Ruido(semilla + 20f, t) * angulo * fuerza;
            float zoomActual = 1f + zoom * fuerza;

            imagenVideo.anchoredPosition = posBase + offset;
            imagenVideo.localRotation = rotBase * Quaternion.Euler(0f, 0f, giro);
            imagenVideo.localScale = escalaBase * zoomActual;
            aplicado = true;
        }
        else if (aplicado)
        {
            imagenVideo.anchoredPosition = posBase;
            imagenVideo.localRotation = rotBase;
            imagenVideo.localScale = escalaBase;
            aplicado = false;
        }
    }

    // Público por si quieres dispararlo desde otro script o desde un botón
    public void Sacudir(float fuerza, float duracion)
    {
        fuerzaGolpe = fuerza;
        duracionGolpe = Mathf.Max(0.05f, duracion);
        restante = duracionGolpe;
        semilla = Random.value * 100f;
    }

    void ProgramarAleatorio()
    {
        proximoAleatorio = Time.time + Random.Range(intervaloMin, intervaloMax);
    }

    // Ruido suave entre -1 y 1
    static float Ruido(float semilla, float t)
    {
        return (Mathf.PerlinNoise(semilla, t) - 0.5f) * 2f;
    }

    // Para probar la intensidad: clic derecho en el título del componente (en modo Play)
    [ContextMenu("Probar sacudida")]
    void Probar()
    {
        Sacudir(1.5f, 0.6f);
    }
}