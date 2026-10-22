using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public GameObject mainPanel;
    bool activo;


    private void Start()
    {
        mainPanel.SetActive(false);
        activo = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            activo = !activo;
            ActivarPanelMain();
        }
    }

    void ActivarPanelMain()
    {
        if (activo)
        {
            mainPanel.SetActive(true);
        }
        else
        {
            mainPanel.SetActive(false);
        }
    }

    public void Jugar()
    {
        SceneManager.LoadScene("JUEGO");
    }
    public void Intro()
    {
        SceneManager.LoadScene("INICIO");
    }

}
