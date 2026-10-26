using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Va en el jugador. Lleva la vida, actualiza la barra del HUD (SliderVida),
// reproduce el sonido de daño y muestra el panel de Game Over cuando la vida llega a 0.
public class VidaJugador : MonoBehaviour
{
    [Header("Vida")]
    public float vidaMaxima = 100f;

    [Tooltip("Barra de vida del HUD (SliderVida)")]
    public Slider sliderVida;

    [Header("Sonido (opcional)")]
    [Tooltip("Nombres (exactos) de los audios del AudioManager que suenan A LA VEZ al recibir daño. Lista vacía = sin sonido")]
    public string[] sonidosDanio = { "Danio1", "Danio2", "Danio3" };

    [Header("Game Over")]
    [Tooltip("Panel que aparece al morir (hijo del Canvas, fuera de MainPanel)")]
    public GameObject panelGameOver;

    [Tooltip("Scripts de pausa (Menu, PausaJuego...) que se desactivan al morir para que Escape no abra la pausa")]
    public Behaviour[] pausaADesactivar;

    [Tooltip("Nombre exacto de la escena del menú de inicio (debe estar en Build Profiles)")]
    public string escenaMenuInicio = "INICIO";

    public float VidaActual { get; private set; }
    public bool EstaMuerto { get; private set; }

    // Mientras sea true el jugador no recibe daño (por ejemplo, durante el ritual)
    public bool Invulnerable { get; set; }

    void Awake()
    {
        VidaActual = vidaMaxima;
    }

    void Start()
    {
        if (sliderVida != null)
        {
            sliderVida.minValue = 0f;
            sliderVida.maxValue = vidaMaxima;
        }

        ActualizarBarra();

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);
        }
    }

    public void RecibirDanio(float cantidad)
    {
        if (EstaMuerto || Invulnerable) return;

        VidaActual = Mathf.Max(0f, VidaActual - cantidad);
        ActualizarBarra();
        ReproducirSonidoDanio();

        if (VidaActual <= 0f)
        {
            Morir();
        }
    }

    void ReproducirSonidoDanio()
    {
        if (sonidosDanio == null || sonidosDanio.Length == 0) return;

        foreach (string nombre in sonidosDanio)
        {
            SonidoSeguro.Reproducir(nombre);
        }
    }

    void Morir()
    {
        EstaMuerto = true;

        // Evita que Escape abra o cierre la pausa estando muerto
        if (pausaADesactivar != null)
        {
            foreach (Behaviour b in pausaADesactivar)
            {
                if (b != null) b.enabled = false;
            }
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }
        else
        {
            Debug.LogWarning("VidaJugador: el jugador murió pero no hay Panel Game Over asignado.", this);
        }
    }

    // Botón "Reiniciar" del panel de Game Over
    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Botón "Menú" del panel de Game Over
    public void IrAlMenuInicio()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaMenuInicio);
    }

    void ActualizarBarra()
    {
        if (sliderVida != null)
        {
            sliderVida.value = VidaActual;
        }
    }
}