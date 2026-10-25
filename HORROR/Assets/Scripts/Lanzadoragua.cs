using UnityEngine;

// Va en el jugador (el mismo objeto que Salto, Interaccion y Linterna).
// Clic derecho (mantener) = apuntar: zoom y mira en pantalla.
// Clic izquierdo mientras apuntas = lanzar un frasco de agua bendita (gasta 1 del contador).
public class LanzadorAgua : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Si se deja vacío usa la cámara con tag MainCamera")]
    public Camera camara;

    [Tooltip("Prefab del frasco que se lanza (con Rigidbody, Collider y el script FrascoLanzado)")]
    public GameObject frascoPrefab;

    [Tooltip("Imagen de mira en el centro del Canvas. Solo se ve mientras apuntas")]
    public GameObject mira;

    [Header("Lanzamiento")]
    [Tooltip("Velocidad inicial del frasco")]
    public float fuerza = 14f;

    [Tooltip("Cuánto se eleva el tiro respecto a la mira para que haga arco")]
    public float elevacion = 0.12f;

    public float tiempoEntreLanzamientos = 0.8f;

    [Tooltip("Distancia frente a la cámara donde aparece el frasco")]
    public float distanciaSalida = 0.6f;

    [Header("Apuntar")]
    [Tooltip("Cuántos grados se reduce el campo de visión al apuntar (zoom)")]
    public float zoomApuntado = 12f;
    public float velocidadZoom = 10f;

    [Header("Sonido (opcional)")]
    public string sonidoLanzar = "";

    public bool Apuntando { get; private set; }

    float fovNormal;
    float proximoLanzamiento;
    Collider[] colsJugador;

    void Start()
    {
        if (camara == null) camara = Camera.main;
        if (camara == null) camara = GetComponentInChildren<Camera>();

        if (camara == null || frascoPrefab == null)
        {
            Debug.LogError("LanzadorAgua: falta la cámara o el prefab del frasco.", this);
            enabled = false;
            return;
        }

        fovNormal = camara.fieldOfView;
        colsJugador = GetComponentsInChildren<Collider>();
        MostrarMira(false);
    }

    void Update()
    {
        // En pausa: sin mira ni zoom
        if (Time.timeScale == 0f)
        {
            Apuntando = false;
            camara.fieldOfView = fovNormal;
            MostrarMira(false);
            return;
        }

        // Solo se puede apuntar si queda agua bendita
        bool tieneAgua = InventarioAgua.instance != null && InventarioAgua.instance.Cantidad > 0;
        Apuntando = tieneAgua && Input.GetMouseButton(1);

        // Zoom suave
        float fovObjetivo = Apuntando ? fovNormal - zoomApuntado : fovNormal;
        camara.fieldOfView = Mathf.Lerp(camara.fieldOfView, fovObjetivo, velocidadZoom * Time.deltaTime);

        MostrarMira(Apuntando);

        if (Apuntando && Input.GetMouseButtonDown(0) && Time.time >= proximoLanzamiento)
        {
            Lanzar();
        }
    }

    void Lanzar()
    {
        if (!InventarioAgua.instance.Usar(1)) return;

        proximoLanzamiento = Time.time + tiempoEntreLanzamientos;

        Transform t = camara.transform;
        Vector3 posicion = t.position + t.forward * distanciaSalida;

        // Si hay una pared pegada, el frasco aparece justo antes de ella
        if (Physics.Raycast(t.position, t.forward, out RaycastHit muro, distanciaSalida, ~0, QueryTriggerInteraction.Ignore))
        {
            posicion = muro.point - t.forward * 0.1f;
        }

        GameObject frasco = Instantiate(frascoPrefab, posicion, t.rotation);

        // El frasco no debe chocar con el propio jugador
        Collider[] colsFrasco = frasco.GetComponentsInChildren<Collider>();
        foreach (Collider cj in colsJugador)
        {
            foreach (Collider cf in colsFrasco)
            {
                Physics.IgnoreCollision(cf, cj);
            }
        }

        Rigidbody rb = frasco.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 direccion = (t.forward + t.up * elevacion).normalized;
            rb.AddForce(direccion * fuerza, ForceMode.VelocityChange);
            rb.AddTorque(Random.insideUnitSphere * 4f, ForceMode.VelocityChange);
        }

        SonidoSeguro.Reproducir(sonidoLanzar);
    }

    void MostrarMira(bool mostrar)
    {
        if (mira != null && mira.activeSelf != mostrar)
        {
            mira.SetActive(mostrar);
        }
    }
}