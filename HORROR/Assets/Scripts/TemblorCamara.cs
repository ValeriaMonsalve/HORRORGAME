using UnityEngine;

// Sacude la cámara del jugador: la inclina y la mueve un poco, y el efecto se apaga solo.
// Va en el jugador. Otros scripts lo usan así:  TemblorCamara.instance.Temblar(grados, metros, segundos);
// Funciona junto a CamaraMouse: la sacudida se suma a la rotación del mouse y se quita sola cada frame.
[DefaultExecutionOrder(-1000)]
public class TemblorCamara : MonoBehaviour
{
    public static TemblorCamara instance;

    [Tooltip("Cámara del jugador (hija del jugador). Si se deja vacía, usa la primera cámara hija")]
    public Transform camaraTransform;

    [Tooltip("Velocidad del temblor: más alto = más nervioso")]
    public float frecuencia = 25f;

    float anguloMax;
    float posicionMax;
    float duracion = 1f;
    float restante;
    float semilla;

    Vector3 offsetPos;
    Quaternion offsetRot = Quaternion.identity;
    bool aplicado;

    void Awake()
    {
        instance = this;

        if (camaraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) camaraTransform = cam.transform;
        }

        if (camaraTransform == null || camaraTransform == transform)
        {
            Debug.LogError("TemblorCamara: la cámara debe ser un objeto HIJO del jugador. Asígnala en el Inspector.", this);
            enabled = false;
        }
    }

    // grados: cuánto se inclina la cámara. metros: cuánto se desplaza. segundos: cuánto dura.
    public void Temblar(float grados, float metros, float segundos)
    {
        if (!enabled) return;

        // Si ya hay un temblor más fuerte en curso, no lo reemplaza
        float fuerzaActual = restante > 0f ? anguloMax * (restante / duracion) : 0f;
        if (grados < fuerzaActual) return;

        anguloMax = grados;
        posicionMax = metros;
        duracion = Mathf.Max(0.01f, segundos);
        restante = duracion;
        semilla = Random.value * 100f;
    }

    // Primero se quita la sacudida del frame anterior (antes de que CamaraMouse y los demás lean la cámara)...
    void Update()
    {
        Deshacer();
    }

    // ...y al final del frame se aplica la nueva, después de que todo lo demás ya movió la cámara.
    void LateUpdate()
    {
        if (restante <= 0f) return;

        restante -= Time.unscaledDeltaTime;

        float fuerza = Mathf.Clamp01(restante / duracion);
        float t = Time.unscaledTime * frecuencia;

        float rx = Ruido(semilla, t) * anguloMax * fuerza;
        float ry = Ruido(semilla + 10f, t) * anguloMax * fuerza;
        float rz = Ruido(semilla + 20f, t) * anguloMax * fuerza;

        offsetRot = Quaternion.Euler(rx, ry, rz);
        offsetPos = new Vector3(Ruido(semilla + 30f, t), Ruido(semilla + 40f, t), 0f) * posicionMax * fuerza;

        camaraTransform.localPosition += offsetPos;
        camaraTransform.localRotation = camaraTransform.localRotation * offsetRot;
        aplicado = true;
    }

    void OnDisable()
    {
        Deshacer();
    }

    void Deshacer()
    {
        if (!aplicado || camaraTransform == null) return;

        camaraTransform.localPosition -= offsetPos;
        camaraTransform.localRotation = camaraTransform.localRotation * Quaternion.Inverse(offsetRot);
        aplicado = false;
    }

    // Ruido suave entre -1 y 1
    static float Ruido(float semilla, float t)
    {
        return (Mathf.PerlinNoise(semilla, t) - 0.5f) * 2f;
    }

    // Para probarlo desde el Inspector: clic derecho en el título del componente (en modo Play)
    [ContextMenu("Probar temblor fuerte (ritual)")]
    void ProbarFuerte()
    {
        Temblar(10f, 0.25f, 3f);
    }

    [ContextMenu("Probar temblor suave (daño)")]
    void ProbarSuave()
    {
        Temblar(4f, 0.05f, 0.4f);
    }
}