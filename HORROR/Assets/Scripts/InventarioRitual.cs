using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

// Lleva la cuenta de los objetos del ritual que el jugador ha recogido (velas, cabello, tiza...).
// Los objetos recogidos desaparecen del mundo pero quedan guardados aquí hasta colocarlos en el altar.
// Ponlo en un objeto vacío activo, fuera de los paneles de menú (por ejemplo "ControlRitual").
public class InventarioRitual : MonoBehaviour
{
    public static InventarioRitual instance;

    [Header("Ajustes")]
    [Tooltip("Cantidad total de objetos del ritual que hay en el juego")]
    public int totalObjetos = 12;

    [Tooltip("Mensaje que aparece al recoger el último objeto. Vacío = ninguno")]
    public string mensajeAlCompletar = "Ya tienes todos los objetos. Llévalos al sótano.";

    [Header("HUD (opcional)")]
    [Tooltip("Texto del Canvas con el progreso y la lista de objetos que lleva el jugador")]
    public TMP_Text textoInventario;

    // Cuántos objetos se han recogido en total, aunque ya se hayan colocado en el altar
    public int Recogidos { get; private set; }

    // No distingue mayúsculas: "Vela" y "vela" son lo mismo
    readonly Dictionary<string, int> objetos =
        new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        ActualizarTexto();
    }

    public void Agregar(string tipo, int cantidad = 1)
    {
        if (string.IsNullOrEmpty(tipo)) return;

        objetos[tipo] = Cantidad(tipo) + cantidad;
        Recogidos += cantidad;
        ActualizarTexto();

        if (totalObjetos > 0 && Recogidos >= totalObjetos && !string.IsNullOrEmpty(mensajeAlCompletar))
        {
            Aviso.Mostrar(mensajeAlCompletar);
        }
    }

    // Cuántos de ese tipo lleva ahora el jugador (sin colocar)
    public int Cantidad(string tipo)
    {
        if (string.IsNullOrEmpty(tipo)) return 0;

        int c;
        return objetos.TryGetValue(tipo, out c) ? c : 0;
    }

    // Gasta una unidad del objeto al colocarlo en el altar. Devuelve false si no tenía ninguna.
    // No baja el contador de Recogidos.
    public bool Quitar(string tipo)
    {
        if (Cantidad(tipo) <= 0) return false;

        objetos[tipo] = Cantidad(tipo) - 1;
        ActualizarTexto();
        return true;
    }

    void ActualizarTexto()
    {
        if (textoInventario == null) return;

        StringBuilder sb = new StringBuilder();

        sb.Append("Ritual objects: " + Recogidos);
        if (totalObjetos > 0) sb.Append("/" + totalObjetos);
        sb.AppendLine();

        foreach (KeyValuePair<string, int> par in objetos)
        {
            if (par.Value <= 0) continue;
            sb.AppendLine(par.Key + " x" + par.Value);
        }

        textoInventario.text = sb.ToString();
    }
}