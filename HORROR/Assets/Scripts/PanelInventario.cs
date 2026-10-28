using System.Collections.Generic;
using System.Text;
using UnityEngine;
using TMPro;

// Lista de objetos del ritual para el menú de pausa: muestra cuáles faltan,
// cuáles ya recogiste y cuáles ya colocaste en el altar.
// La lista sale de las ranuras del AltarRitual, así que no hay que escribirla dos veces.
// Va en el panel de Inventario del menú de pausa y se actualiza cada vez que ese panel se abre.
public class PanelInventario : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Texto (TextMeshPro) donde se escribe la lista")]
    public TMP_Text textoLista;

    [Header("Textos")]
    public string titulo = "OBJETOS DEL RITUAL";
    public string sufijoColocado = "(colocado)";
    public string sinInformacion = "Aún no hay información del ritual.";

    [Header("Colores")]
    public Color colorFalta = new Color(0.45f, 0.45f, 0.45f);
    public Color colorRecogido = new Color(0.4f, 1f, 0.4f);
    public Color colorColocado = new Color(1f, 0.8f, 0.3f);

    void OnEnable()
    {
        Refrescar();
    }

    public void Refrescar()
    {
        if (textoLista == null) return;

        AltarRitual altar = FindAnyObjectByType<AltarRitual>();
        InventarioRitual inventario = InventarioRitual.instance;

        if (altar == null || altar.ranuras == null || altar.ranuras.Length == 0 || inventario == null)
        {
            textoLista.text = sinInformacion;
            return;
        }

        // Cuántas ranuras de cada tipo ya se colocaron en el altar
        Dictionary<string, int> colocadas = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        foreach (AltarRitual.Ranura r in altar.ranuras)
        {
            if (!r.colocada || string.IsNullOrEmpty(r.tipo)) continue;

            int c;
            colocadas.TryGetValue(r.tipo, out c);
            colocadas[r.tipo] = c + 1;
        }

        // Cada ranura es una línea. Si hay varias del mismo tipo (por ejemplo 5 velas),
        // se reparten así: primero las colocadas, luego las recogidas y al final las que faltan.
        Dictionary<string, int> posicion = new Dictionary<string, int>(System.StringComparer.OrdinalIgnoreCase);
        StringBuilder lineas = new StringBuilder();
        int total = altar.ranuras.Length;
        int conseguidos = 0;

        foreach (AltarRitual.Ranura r in altar.ranuras)
        {
            string tipo = r.tipo ?? "";

            int n;
            posicion.TryGetValue(tipo, out n);
            posicion[tipo] = n + 1;

            int nColocadas;
            colocadas.TryGetValue(tipo, out nColocadas);
            int nRecogidas = inventario.TotalRecogido(tipo);

            Color color;
            string marca;
            string sufijo = "";

            if (n < nColocadas)
            {
                color = colorColocado;
                marca = "[X]";
                sufijo = " " + sufijoColocado;
                conseguidos++;
            }
            else if (n < nRecogidas)
            {
                color = colorRecogido;
                marca = "[X]";
                conseguidos++;
            }
            else
            {
                color = colorFalta;
                marca = "[  ]";
            }

            lineas.AppendLine("<color=#" + ColorUtility.ToHtmlStringRGB(color) + ">" + marca + " " + tipo + sufijo + "</color>");
        }

        textoLista.text = titulo + "\nPlaced: " + conseguidos + "/" + total + "\n\n" + lineas.ToString();
    }
}