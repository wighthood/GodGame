using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static GameData LoadedData;   // persistant en mémoire

    string GetPath()
    {
        return Application.persistentDataPath + "/AllData.json";
    }

    public void LoadGame()
    {
        string filePath = GetPath();
        if (!System.IO.File.Exists(filePath))
        {
            Debug.LogWarning("Aucune sauvegarde trouvée");
            return;
        }

        string allData = System.IO.File.ReadAllText(filePath);
        LoadedData = JsonUtility.FromJson<GameData>(allData);

        SceneManager.LoadScene("GameScene");
    }
}