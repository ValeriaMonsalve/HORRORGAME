using UnityEngine;
using UnityEngine.Video; 
using UnityEngine.SceneManagement; 

public class VideoSceneChanger : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Arrastra aquí el componente VideoPlayer")]
    public VideoPlayer videoPlayer;

    [Tooltip("Escribe exactamente el nombre de la escena a la que quieres ir")]
    public string nombreSiguienteEscena;

    void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
        }
        else
        {
            Debug.LogError("No se encontró ningún componente VideoPlayer asignado.");
        }
    }

    void OnVideoEnd(VideoPlayer vp)
    {

        if (!string.IsNullOrEmpty(nombreSiguienteEscena))
        {
            SceneManager.LoadScene(nombreSiguienteEscena);
        }
        else
        {
            Debug.LogWarning("El nombre de la siguiente escena está vacío.");
        }
    }

    void OnDestroy()
    {
        // Buenas prácticas: desuscribirse del evento cuando el objeto se destruye para evitar fugas de memoria
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
    }
}