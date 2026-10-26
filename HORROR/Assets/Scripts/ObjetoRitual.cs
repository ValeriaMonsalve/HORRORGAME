using UnityEngine;

// Va en cada objeto del ritual que el jugador puede recoger (vela, cabello, tiza, aguja...).
// El objeto necesita un Collider que NO sea Trigger.
public class ObjetoRitual : MonoBehaviour, IInteractuable
{
    [Tooltip("Tipo de objeto. Debe ser IGUAL al 'Tipo' de la ranura del altar (ej: Vela, Cabello, Tiza). No distingue mayúsculas")]
    public string tipo = "Vela";

    [Tooltip("Nombre que ve el jugador. Vacío = usa el tipo")]
    public string nombreVisible = "";

    [Tooltip("Nombre de un audio del AudioManager que suena al recogerlo (opcional)")]
    public string sonidoRecoger = "";

    string Nombre
    {
        get { return string.IsNullOrEmpty(nombreVisible) ? tipo : nombreVisible; }
    }

    public string TextoInteraccion
    {
        get { return "Get" + Nombre; }
    }

    public void Interactuar()
    {
        if (InventarioRitual.instance == null)
        {
            Debug.LogError("ObjetoRitual: no hay un InventarioRitual en la escena.", this);
            return;
        }

        Aviso.Mostrar("You collected: " + Nombre);
        InventarioRitual.instance.Agregar(tipo);
        SonidoSeguro.Reproducir(sonidoRecoger);

        Destroy(gameObject);
    }
}