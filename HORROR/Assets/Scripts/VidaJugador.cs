using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Va en el jugador. Lleva la vida, actualiza la barra del HUD (SliderVida)
// y muestra el Game Over cuando llega a 0.
public class VidaJugador : MonoBehaviour
{
    [Header("Vida")]
    public float vidaMaxima = 100f;

    [Tooltip("Barra de vida del HUD (SliderVida)")]
    public Slider sliderVida;

    [Header("Game Over (opcional)")]
    [Tooltip("Panel que aparece al morir (hijo del Canvas, fuera de MainPanel)")]
    public GameObject panelGameOver;

    [Tooltip("Opcional: arrastra aquí el componente PausaJuego para que Escape no reanude la partida estando muerto")]
    public Behaviour pausaADesactivar;

    [Header("Sonido (opcional)")]
    [Tooltip("Nombres (exactos) de los audios del AudioManager que suenan A LA VEZ al recibir daño. Lista vacía = sin sonido")]
    public string[] sonidosDanio = { "Danio1", "Danio2", "Danio3" };

    public float VidaActual { get; private set; }
    public bool EstaMuerto { get; private set; }

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
        if (EstaMuerto) return;

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

        AudioManager am = AudioManager.instance;
        if (am == null) return;

        // Cada audio del AudioManager tiene su propio AudioSource, así que suenan todos a la vez
        foreach (string nombre in sonidosDanio)
        {
            if (string.IsNullOrEmpty(nombre)) continue;

            // Se comprueba antes porque AudioManager.Play() da error si el nombre no existe
            string nombreActual = nombre;
            if (System.Array.Exists(am.audios, a => a.nombre == nombreActual))
            {
                am.Play(nombreActual);
            }
            else
            {
                Debug.LogWarning("VidaJugador: el audio '" + nombreActual + "' no existe en el AudioManager.", this);
            }
        }
    }

    void Morir()
    {
        EstaMuerto = true;

        if (pausaADesactivar != null)
        {
            pausaADesactivar.enabled = false;
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }
    }

    // Botón "Reiniciar" del panel de Game Over
    public void Reiniciar()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void ActualizarBarra()
    {
        if (sliderVida != null)
        {
            sliderVida.value = VidaActual;
        }
    }
}