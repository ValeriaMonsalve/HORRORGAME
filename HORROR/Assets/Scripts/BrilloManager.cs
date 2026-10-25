using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// Controla el brillo del juego con un slider, igual que el volumen en AudioManager.
// - Guarda el valor en PlayerPrefs ("Brillo"), así que se conserva entre escenas y sesiones.
// - Ajusta el "Post Exposure" del override Color Adjustments de tu Global Volume.
//   Funciona igual en URP y en HDRP.
// Ponlo en un objeto vacío que esté ACTIVO y fuera de los paneles de menú (por ejemplo "ControlBrillo").
// En el menú de inicio puedes dejar "Volumen" vacío: ahí solo guarda el valor y el juego lo aplica al cargar.
public class BrilloManager : MonoBehaviour
{
    [Header("Controles")]
    [Tooltip("Slider de brillo del menú (el slider no necesita eventos en el Inspector)")]
    public Slider brilloSlider;

    [Header("Escena de juego")]
    [Tooltip("Global Volume que tiene el override Color Adjustments. Vacío = solo guarda el valor")]
    public Volume volumen;

    [Header("Rango")]
    [Tooltip("Post Exposure con el slider al mínimo (más oscuro)")]
    public float exposicionMinima = -2f;

    [Tooltip("Post Exposure con el slider al máximo (más claro)")]
    public float exposicionMaxima = 2f;

    [Tooltip("Valor del slider la primera vez que se juega. 0.5 deja la imagen tal como la diseñaste")]
    [Range(0f, 1f)] public float brilloPorDefecto = 0.5f;

    const string Clave = "Brillo";

    VolumeParameter<float> parametroExposicion;

    void Start()
    {
        BuscarParametro();

        float guardado = PlayerPrefs.GetFloat(Clave, brilloPorDefecto);

        if (brilloSlider != null)
        {
            brilloSlider.minValue = 0f;
            brilloSlider.maxValue = 1f;
            brilloSlider.SetValueWithoutNotify(guardado);
            brilloSlider.onValueChanged.AddListener(CambiarBrillo);
        }

        Aplicar(guardado);
    }

    public void CambiarBrillo(float valor)
    {
        PlayerPrefs.SetFloat(Clave, valor);
        Aplicar(valor);
    }

    void Aplicar(float valor)
    {
        if (parametroExposicion == null) return;

        parametroExposicion.overrideState = true;
        parametroExposicion.value = Mathf.Lerp(exposicionMinima, exposicionMaxima, valor);
    }

    // Busca "Post Exposure" dentro del override Color Adjustments.
    // Se hace por nombre para que el mismo script sirva en URP y en HDRP.
    void BuscarParametro()
    {
        if (volumen == null) return;

        if (volumen.profile == null)
        {
            Debug.LogError("BrilloManager: el Volume no tiene ningún Profile asignado.", this);
            return;
        }

        // volumen.profile es una copia en tiempo de ejecución: no modifica el archivo del perfil
        foreach (VolumeComponent componente in volumen.profile.components)
        {
            if (componente.GetType().Name != "ColorAdjustments") continue;

            FieldInfo campo = componente.GetType().GetField("postExposure", BindingFlags.Public | BindingFlags.Instance);
            if (campo != null)
            {
                parametroExposicion = campo.GetValue(componente) as VolumeParameter<float>;
            }
            break;
        }

        if (parametroExposicion == null)
        {
            Debug.LogError("BrilloManager: no encontré Color Adjustments → Post Exposure en ese Volume. Agrégalo como override.", this);
        }
    }

    void OnDestroy()
    {
        PlayerPrefs.Save();
    }
}