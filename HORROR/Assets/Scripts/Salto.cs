using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Salto : MonoBehaviour
{
    [Header("Salto")]
    [Tooltip("Altura aproximada del salto, en metros")]
    public float alturaSalto = 1f;

    [Tooltip("Tiempo mínimo entre saltos (evita saltos dobles accidentales)")]
    public float tiempoEntreSaltos = 0.2f;

    [Header("Detección de suelo")]
    [Tooltip("Capas que cuentan como suelo. Por defecto: todas")]
    public LayerMask capaSuelo = ~0;

    [Tooltip("Margen por debajo de los pies para considerar que está en el suelo")]
    public float distanciaSuelo = 0.15f;

    // Útil para otras mecánicas futuras (pasos, agacharse, sonidos, etc.)
    public bool EnSuelo { get; private set; }

    Rigidbody rb;
    Collider col;
    bool quiereSaltar;
    float ultimoSalto = -10f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        col = GetComponent<Collider>();
        if (col == null) col = GetComponentInChildren<Collider>();
        if (col == null)
        {
            Debug.LogError("Salto: el jugador necesita un Collider (por ejemplo, un Capsule Collider).", this);
        }

        // Configuración necesaria para que la gravedad y el salto funcionen.
        // La rotación se congela solo para la física: el giro de Movement.cs
        // (transform.Rotate) sigue funcionando igual.
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        // El input se lee en Update para no perder pulsaciones...
        if (Input.GetKeyDown(KeyCode.Space))
        {
            quiereSaltar = true;
        }
    }

    void FixedUpdate()
    {
        // ...y la física se aplica en FixedUpdate.
        EnSuelo = DetectarSuelo();

        if (quiereSaltar)
        {
            quiereSaltar = false;

            if (EnSuelo && Time.time - ultimoSalto >= tiempoEntreSaltos)
            {
                // v = sqrt(2 * g * h) -> velocidad inicial para alcanzar la altura deseada
                float velocidad = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * alturaSalto);
                rb.AddForce(Vector3.up * velocidad, ForceMode.VelocityChange);
                ultimoSalto = Time.time;
            }
        }
    }

    bool DetectarSuelo()
    {
        if (col == null) return false;

        Bounds b = col.bounds;
        float radio = Mathf.Max(0.05f, Mathf.Min(b.extents.x, b.extents.z) * 0.9f);
        float distancia = Mathf.Max(0.01f, b.extents.y - radio + distanciaSuelo);

        // Esfera lanzada desde el centro hacia abajo; ignora el propio collider del jugador
        // y los triggers.
        return Physics.SphereCast(
            b.center,
            radio,
            Vector3.down,
            out _,
            distancia,
            capaSuelo,
            QueryTriggerInteraction.Ignore
        );
    }
}