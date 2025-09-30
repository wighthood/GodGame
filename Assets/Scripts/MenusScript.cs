using UnityEngine;
using UnityEngine.SceneManagement;

public class MenusScript : MonoBehaviour
{
    public static void Begin()
    {
        SceneManager.LoadScene("GameScene");
    }

    public static void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
    }
    
    public static void Quit()
    {
        Application.Quit();
    }

    public static void Pause()
    {
        Time.timeScale = Time.timeScale == 0 ? 1 : 0;
    }

    public static void Pause(bool pause)
    {
        Time.timeScale = pause ? 1 : 0;
    }
}
