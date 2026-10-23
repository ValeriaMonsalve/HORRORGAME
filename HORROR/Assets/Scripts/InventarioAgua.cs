using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Lleva la cuenta de frascos de agua bendita y actualiza el marcador del HUD.
// Ponlo en un objeto de la escena que NO esté dentro de MainPanel (el panel de pausa),
// por ejemplo un objeto vacío llamado "ControlAgua" o el propio Canvas.
public class InventarioAgua : MonoBehaviour
{
    public static InventarioAgua instance;

    [Header("Marcador del HUD (asigna solo el que uses)")]
    [Tooltip("Si tu número es un TextMeshPro")]
    public TMP_Text textoTMP;

    [Tooltip("Si tu número es un Text normal (Legacy)")]
    public Text textoLegacy;

    [Header("Ajustes")]
    [SerializeField] int cantidadInicial = 0;

    public int Cantidad { get; private set; }

    void Awake()
    {
        instance = this;
        Cantidad = cantidadInicial;
    }

    void Start()
    {
        ActualizarMarcador();
    }

    public void Agregar(int n = 1)
    {
        Cantidad += n;
        ActualizarMarcador();
    }

    // Para usarlo después al lanzar el agua bendita.
    // Devuelve false si no hay suficientes frascos.
    public bool Usar(int n = 1)
    {
        if (Cantidad < n) return false;

        Cantidad -= n;
        ActualizarMarcador();
        return true;
    }

    void ActualizarMarcador()
    {
        string valor = Cantidad.ToString();

        if (textoTMP != null) textoTMP.text = valor;
        if (textoLegacy != null) textoLegacy.text = valor;
    }
}