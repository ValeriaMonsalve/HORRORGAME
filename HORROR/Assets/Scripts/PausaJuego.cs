using UnityEngine;

// Complemento para tu Menu.cs (que sigue funcionando igual, sin cambios).
// Vigila el panel de pausa: mientras está abierto congela el juego y libera el cursor;
// al cerrarse, todo vuelve a la normalidad. No escucha Escape ni toca ningún botón.
// Ponlo en el MISMO objeto que Menu.cs (nunca dentro de MainPanel).
public class PausaJuego : MonoBehaviour
{
    [Tooltip("El mismo MainPanel que usa Menu.cs")]
    public GameObject panelPausa;

    VidaJugador vida;
    bool pausado;
    CursorLockMode lockPrevio;
    bool cursorVisiblePrevio;

    void Start()
    {
        if (panelPausa == null)
        {
            Debug.LogError("PausaPorPanel: asigna el MainPanel en el Inspector.", this);
            enabled = false;
            return;
        }

        vida = FindAnyObjectByType<VidaJugador>();
        Time.timeScale = 1f;
    }

    // LateUpdate: así detecta el cambio del panel en el mismo frame en que Menu.cs lo abre o lo cierra
    void LateUpdate()
    {
        // Si el jugador murió, manda el Game Over: no se toca el tiempo
        if (vida != null && vida.EstaMuerto) return;

        bool abierto = panelPausa.activeInHierarchy;

        if (abierto && !pausado)
        {
            Pausar();
        }
        else if (!abierto && pausado)
        {
            Reanudar();
        }
    }

    void Pausar()
    {
        pausado = true;

        // Guarda el estado del cursor para devolverlo igual al reanudar
        lockPrevio = Cursor.lockState;
        cursorVisiblePrevio = Cursor.visible;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    void Reanudar()
    {
        pausado = false;

        Time.timeScale = 1f;
        Cursor.lockState = lockPrevio;
        Cursor.visible = cursorVisiblePrevio;
    }

    void OnDestroy()
    {
        // Menu.cs cambia de escena sin restablecer el tiempo (por ejemplo el botón "menu"),
        // así que aquí se evita que la escena siguiente arranque congelada.
        if (pausado)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        Time.timeScale = 1f;
    }
}