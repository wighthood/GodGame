using UnityEngine;
using UnityEngine.SceneManagement;

public class MenusScript : MonoBehaviour
{
    public void Begin()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void Pause()
    {
        Time.timeScale = Time.timeScale == 0 ? 1 : 0;
    }
}
