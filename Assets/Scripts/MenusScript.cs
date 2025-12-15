using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenusScript : MonoBehaviour
{
    public Button buttonLoad;
    
    string SavePath => Application.persistentDataPath + "/savegame.json";
    
    public void Start()
    {
        if (File.Exists(SavePath))
        {
            if (buttonLoad != null)
            {
                buttonLoad.interactable = true;
            }
            else
            {
                Debug.LogWarning("MenusScript: buttonLoad n'est pas assigné dans l'Inspecteur !");
            }
        }
    }

    public static void MainMenu()
    {
        SceneManager.LoadScene("SaveTestMain");
        if (AudioManager.Instance != null)
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
        SaveEvents.ShouldLoadOnStart = false;
        SceneManager.LoadScene("GameSceneCOL");
        
        if (AudioManager.Instance != null) 
            AudioManager.Instance.ChangeMusic(AudioManager.SoundType.Music_game);
    }

    public void OnClickLoad()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("Fichier de sauvegarde introuvable !");
            if (buttonLoad != null) buttonLoad.interactable = false;
            return;
        }

        SaveEvents.ShouldLoadOnStart = true;
        SceneManager.LoadScene("GameSceneCOL");

        if (AudioManager.Instance != null) 
            AudioManager.Instance.ChangeMusic(AudioManager.SoundType.Music_game);
    }
    
    public void Save() { SaveEvents.OnRequestSaveEvent?.Invoke(); }
}