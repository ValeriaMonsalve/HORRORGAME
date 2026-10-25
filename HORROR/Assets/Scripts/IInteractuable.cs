// Todo lo que el jugador activa con la tecla E (agua bendita, objetos del ritual, altar...)
// implementa esta interfaz, y así Interaccion.cs los trata a todos igual.
public interface IInteractuable
{
    // Texto que se muestra junto a la E. Si devuelve vacío, no se muestra el prompt.
    string TextoInteraccion { get; }

    // Lo que ocurre al presionar E.
    void Interactuar();
}