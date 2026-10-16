using UnityEngine;
using UnityEngine.SceneManagement;

public class SkipAnimation : MonoBehaviour
{
    public void SkipFirstAnimation()
    {
        SceneManager.LoadScene("JUEGO1");
    }
}
