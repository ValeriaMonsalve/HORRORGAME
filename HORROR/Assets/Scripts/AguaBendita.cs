using UnityEngine;

// Va en cada frasco de agua bendita que pongas en la escena.
// El frasco necesita un Collider que NO sea Trigger.
public class AguaBendita : MonoBehaviour, IInteractuable
{
    [Tooltip("Cuántos frascos da este objeto al recogerlo")]
    public int cantidad = 1;

    [Tooltip("Nombre de un audio del AudioManager (opcional). Déjalo vacío si no quieres sonido")]
    public string sonidoRecoger = "";

    public string TextoInteraccion
    {
        get { return "Recoger agua bendita"; }
    }

    public void Interactuar()
    {
        Recoger();
    }

    public void Recoger()
    {
        if (InventarioAgua.instance == null)
        {
            Debug.LogError("AguaBendita: no hay un InventarioAgua en la escena.", this);
            return;
        }

        InventarioAgua.instance.Agregar(cantidad);
        ReproducirSonido();
        Destroy(gameObject);
    }

    void ReproducirSonido()
    {
        if (string.IsNullOrEmpty(sonidoRecoger)) return;

        AudioManager am = AudioManager.instance;
        if (am == null) return;

        // Se comprueba antes porque AudioManager.Play() da error si el nombre no existe
        if (System.Array.Exists(am.audios, a => a.nombre == sonidoRecoger))
        {
            am.Play(sonidoRecoger);
        }
        else
        {
            Debug.LogWarning("AguaBendita: el audio '" + sonidoRecoger + "' no existe en el AudioManager.", this);
        }
    }
}