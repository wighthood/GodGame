using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenusScript : MonoBehaviour
{
    public Button buttonLoad;
    public TextMeshProUGUI text;
    public TextMeshProUGUI textLoad;
    
    string SavePath => Application.persistentDataPath + "/savegame.json";
    
    public void Start()
    {
        if (File.Exists(SavePath))
        {
            if (buttonLoad != null)
            {
                buttonLoad.interactable = true;
                buttonLoad.image.color = new Color(1, 1, 1, 1);
                textLoad.color = new Color(0, 0, 0, 1);
            }
            else
            {
                Debug.LogWarning("MenusScript: buttonLoad n'est pas assigné dans l'Inspecteur !");
            }
        }
    }

    public static void MainMenu()
    {
        SceneManager.LoadScene("Main Menu");
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
        SceneManager.LoadScene("GameScene");
        
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
        SceneManager.LoadScene("GameScene");

        if (AudioManager.Instance != null) 
            AudioManager.Instance.ChangeMusic(AudioManager.SoundType.Music_game);
    }

    public void OpenURL(string url)
    {
        Application.OpenURL(url);
    }

    public void DestroyShroom(GameObject shroom)
    {
        StartCoroutine(DestroyShroomMainMenu(shroom));
    }

    public IEnumerator DestroyShroomMainMenu(GameObject shroomMainMenu)
    {
        Image image = shroomMainMenu.GetComponent<Image>();
        
        image.enabled = false;
        yield return new WaitForSecondsRealtime(2f);
        image.enabled = true;
    }
    
    public void Save() { SaveEvents.OnRequestSaveEvent?.Invoke(); }
}
