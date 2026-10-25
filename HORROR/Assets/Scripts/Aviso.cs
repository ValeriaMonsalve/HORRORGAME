using UnityEngine;
using TMPro;

// Muestra mensajes cortos en pantalla ("Recogiste: Vela") y los borra solos.
// Ponlo en un objeto activo fuera de los paneles de menú, y arrastra un texto del Canvas al campo "Texto".
public class Aviso : MonoBehaviour
{
    static Aviso instance;

    [Tooltip("Texto del Canvas donde aparecen los avisos")]
    public TMP_Text texto;

    [Tooltip("Segundos que se queda visible el mensaje")]
    public float duracion = 2f;

    float restante;

    void Awake()
    {
        instance = this;
        if (texto != null) texto.text = "";
    }

    // Se llama desde otros scripts: AvisoUI.Mostrar("mensaje");
    public static void Mostrar(string mensaje)
    {
        if (instance == null || instance.texto == null) return;

        instance.texto.text = mensaje;
        instance.restante = instance.duracion;
    }

    void Update()
    {
        if (restante <= 0f) return;

        restante -= Time.unscaledDeltaTime;
        if (restante <= 0f && texto != null)
        {
            texto.text = "";
        }
    }
}