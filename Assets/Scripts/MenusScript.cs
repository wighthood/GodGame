using UnityEngine;
using UnityEngine.SceneManagement;

public class MenusScript : MonoBehaviour
{
    public static void Begin()
    {
        SceneManager.LoadScene("GameScene SaveSystem");
        AudioManager.Instance.ChangeMusic(AudioManager.SoundType.Music_game);
    }

    public static void MainMenu()
    {
        SceneManager.LoadScene("Main Menu saveSystem");
        AudioManager.Instance.ChangeMusic(AudioManager.SoundType.Music_Menu);
        Time.timeScale = 1;
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
    
    public void OnClickPlay()
    {
        GameModeManager.Instance.currentMode = GameModeManager.GameMode.Play;
        GameModeManager.Instance.saveManager.OnClickPlay();
    }

    public void OnClickLoad()
    {
        GameModeManager.Instance.currentMode = GameModeManager.GameMode.Load;
        GameModeManager.Instance.saveManager.OnClickLoad();
    }
}
