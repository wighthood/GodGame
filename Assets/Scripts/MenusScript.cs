using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenusScript : MonoBehaviour
{
    public Button buttonLoad;
    
    string StatsPath => Application.persistentDataPath + "/AllData.json";
    string TilemapPath => Application.persistentDataPath + "/tilemap.json";
    string RessourcePath => Application.persistentDataPath + "/ressource.json";
    string BlackBoardPath => Application.persistentDataPath + "/blackboard.json";
    
    public void Start()
    {
        if (File.Exists(StatsPath) || File.Exists(TilemapPath) || File.Exists(RessourcePath) ||
            File.Exists(BlackBoardPath))
        {
            buttonLoad.interactable = true;
        }
    }

    public static void Begin()
    {
        SceneManager.LoadScene("GameScene");
        AudioManager.Instance.ChangeMusic(AudioManager.SoundType.Music_game);
    }

    public static void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
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
        GameModeManager.Instance.saveManager.LoadAll();
    }
}
