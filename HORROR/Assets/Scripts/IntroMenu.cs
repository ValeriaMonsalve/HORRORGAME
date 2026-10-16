using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroMenu : MonoBehaviour
{
    public void Jugar()
    {
        SceneManager.LoadScene("JUEGO");
    }

    public void SwitchScenes1()
    {
        SceneManager.LoadScene("JUEGO");
    }
    public void SwitchScenes2()
    {
        SceneManager.LoadScene("JUEGO2");
    }

    public void ExitGame()
    {
        Application.Quit();
        Debug.Log("Salir del juego");
    }

}
