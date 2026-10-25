using UnityEngine;
using TMPro;

// Va en el jugador (el mismo objeto que Salto y Linterna).
// Mira hacia donde apunta la cámara: si hay algo interactuable cerca (agua bendita,
// objetos del ritual, altar...), muestra la "E" y lo activa al presionarla.
public class Interaction : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Si se deja vacío usa la cámara con tag MainCamera")]
    public Camera camara;

    [Tooltip("Objeto de UI con la 'E' (hijo directo del Canvas, fuera de MainPanel)")]
    public GameObject promptE;

    [Tooltip("Opcional: texto donde se escribe qué hará la E, por ejemplo '[E] Recoger Vela'. Puedes arrastrar el mismo texto de PromptE")]
    public TMP_Text textoPrompt;

    [Header("Ajustes")]
    public KeyCode teclaInteractuar = KeyCode.E;

    [Tooltip("Distancia máxima, en metros, a la que se puede interactuar")]
    public float distancia = 3f;

    [Tooltip("Grosor del 'rayo' al apuntar; más grande = más fácil apuntar a objetos pequeños")]
    public float radioApuntado = 0.15f;

    [Tooltip("Capas que bloquean o detectan el rayo. Por defecto: todas")]
    public LayerMask capas = ~0;

    IInteractuable objetivo;

    void Awake()
    {
        if (camara == null) camara = Camera.main;
        if (camara == null) camara = GetComponentInChildren<Camera>();

        if (camara == null)
        {
            Debug.LogError("Interaccion: no encontré ninguna cámara. Asígnala en el Inspector.", this);
        }

        MostrarPrompt(false);
    }

    void Update()
    {
        // Sin interacción mientras el juego está en pausa
        if (Time.timeScale == 0f || camara == null)
        {
            objetivo = null;
            MostrarPrompt(false);
            return;
        }

        objetivo = BuscarObjetivo();
        MostrarPrompt(objetivo != null);

        if (objetivo == null) return;

        if (textoPrompt != null)
        {
            textoPrompt.text = "[" + teclaInteractuar + "] " + objetivo.TextoInteraccion;
        }

        if (Input.GetKeyDown(teclaInteractuar))
        {
            objetivo.Interactuar();
            objetivo = null;
            MostrarPrompt(false);
        }
    }

    IInteractuable BuscarObjetivo()
    {
        // Rayo desde el centro de la pantalla
        Ray rayo = camara.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.SphereCast(rayo, radioApuntado, out RaycastHit hit, distancia, capas, QueryTriggerInteraction.Ignore))
        {
            // Busca el componente en el collider o en su padre (por si el collider está en un hijo)
            IInteractuable candidato = hit.collider.GetComponentInParent<IInteractuable>();

            if (candidato != null && !string.IsNullOrEmpty(candidato.TextoInteraccion))
            {
                return candidato;
            }
        }

        return null;
    }

    void MostrarPrompt(bool mostrar)
    {
        if (promptE != null && promptE.activeSelf != mostrar)
        {
            promptE.SetActive(mostrar);
        }
    }
}