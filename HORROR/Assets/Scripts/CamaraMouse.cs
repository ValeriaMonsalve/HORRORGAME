using UnityEngine;

// Va en el jugador (el mismo objeto que Salto e Interaccion).
// Mouse horizontal -> gira el cuerpo del jugador (igual que lo hace Movement.cs con A/D).
// Mouse vertical   -> inclina solo la cámara, con un límite para no dar la vuelta.
public class CamaraMouse : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Cámara del jugador. DEBE ser hija del jugador. Si se deja vacía, usa la primera cámara hija")]
    public Transform camaraTransform;

    [Header("Ajustes")]
    public float sensibilidad = 2f;
    public bool invertirY = false;

    [Tooltip("Hasta cuántos grados se puede mirar arriba y abajo")]
    [Range(0f, 89f)] public float limiteVertical = 80f;

    [Tooltip("Oculta y bloquea el cursor al iniciar la escena")]
    public bool bloquearCursor = true;

    float rotX;
    float yInicial;
    float zInicial;

    void Awake()
    {
        if (camaraTransform == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null) camaraTransform = cam.transform;
        }

        if (camaraTransform == null || camaraTransform == transform)
        {
            Debug.LogError("CamaraMouse: la cámara debe ser un objeto HIJO del jugador. Asígnala en el Inspector.", this);
            enabled = false;
            return;
        }

        // Respeta la rotación con la que ya tienes puesta la cámara
        Vector3 e = camaraTransform.localEulerAngles;
        rotX = NormalizarAngulo(e.x);
        yInicial = e.y;
        zInicial = e.z;
    }

    void Start()
    {
        if (bloquearCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        // En pausa no se mueve la cámara: el mouse se usa para los botones
        if (Time.timeScale == 0f) return;

        // Ojo: el movimiento del mouse ya viene como "delta", no se multiplica por Time.deltaTime
        float mouseX = Input.GetAxis("Mouse X") * sensibilidad;
        float mouseY = Input.GetAxis("Mouse Y") * sensibilidad;

        // Horizontal: gira el cuerpo del jugador
        transform.Rotate(0f, mouseX, 0f);

        // Vertical: inclina la cámara
        rotX += invertirY ? mouseY : -mouseY;
        rotX = Mathf.Clamp(rotX, -limiteVertical, limiteVertical);
        camaraTransform.localRotation = Quaternion.Euler(rotX, yInicial, zInicial);
    }

    static float NormalizarAngulo(float angulo)
    {
        return angulo > 180f ? angulo - 360f : angulo;
    }
}