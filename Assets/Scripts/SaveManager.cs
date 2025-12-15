using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Collections;

[Serializable]
public class SaveFileStructure
{
    public List<SystemSaveData> SystemsData = new List<SystemSaveData>();
}

[Serializable]
public class SystemSaveData
{
    public string ID;
    public string JsonData;
}

public class SaveManager : MonoBehaviour
{
    private string SavePath => Application.persistentDataPath + "/savegame.json";
    
    private List<ISaveable> _saveables = new List<ISaveable>();
    private bool _shouldLoadAfterSceneChange;

    private static bool _isInitialized;

    private void Awake()
    {
        if (_isInitialized)
        {
            Destroy(gameObject);
            return;
        }

        _isInitialized = true;
        // Manager must persist to handle scene changes
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SaveEvents.OnRegisterSaveableEvent += Register;
        SaveEvents.OnUnregisterSaveableEvent += Unregister;
        
        SaveEvents.OnRequestNewGameEvent += HandleRequestNewGame;
        SaveEvents.OnRequestLoadGameEvent += HandleRequestLoadGame;
        SaveEvents.OnRequestSaveEvent += HandleRequestSave;
        
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SaveEvents.OnRegisterSaveableEvent -= Register;
        SaveEvents.OnUnregisterSaveableEvent -= Unregister;
        
        SaveEvents.OnRequestNewGameEvent -= HandleRequestNewGame;
        SaveEvents.OnRequestLoadGameEvent -= HandleRequestLoadGame;
        SaveEvents.OnRequestSaveEvent -= HandleRequestSave;

        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Register(ISaveable saveable)
    {
        if (_saveables.Contains(saveable)) return;

        // Ensure no duplicate IDs exist
        string id = saveable.GetSaveID();
        // Remove any existing saveable with same ID
        for (int i = _saveables.Count - 1; i >= 0; i--)
        {
            if (_saveables[i] == null || _saveables[i].Equals(null))
            {
                _saveables.RemoveAt(i);
                continue;
            }
            
            if (_saveables[i].GetSaveID() == id)
            {
                Debug.LogWarning($"SaveManager: Remplacement du système sauvegardable ID: '{id}'");
                _saveables.RemoveAt(i);
            }
        }

        _saveables.Add(saveable);
    }

    private void Unregister(ISaveable saveable)
    {
        if (_saveables.Contains(saveable)) _saveables.Remove(saveable);
    }

    private void HandleRequestNewGame(string sceneName)
    {
        _shouldLoadAfterSceneChange = false;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    private void HandleRequestLoadGame(string sceneName)
    {
        _shouldLoadAfterSceneChange = true;
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    private void HandleRequestSave()
    {
        SaveGame();
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name == "Main Menu") return;

        if (_shouldLoadAfterSceneChange)
        {
            StartCoroutine(LoadGameRoutine());
        }
        else
        {
            SaveEvents.OnNewGameStartEvent?.Invoke();
        }
    }

    private IEnumerator LoadGameRoutine()
    {
        yield return null;
        LoadGame();
    }

    public void SaveGame()
    {
        Debug.Log("Début de la sauvegarde...");
        
        SaveFileStructure globalSave = new SaveFileStructure();
        
        foreach (ISaveable saveable in _saveables)
        {
            string id = saveable.GetSaveID();
            string data = saveable.CaptureState();

            if (!string.IsNullOrEmpty(data))
            {
                globalSave.SystemsData.Add(new SystemSaveData 
                { 
                    ID = id, 
                    JsonData = data 
                });
            }
        }
        
        string finalJson = JsonUtility.ToJson(globalSave, true);
        File.WriteAllText(SavePath, finalJson);
        
        Debug.Log($"Sauvegarde terminée avec succès ! ({globalSave.SystemsData.Count} systèmes sauvegardés)");
    }
    
    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("Aucun fichier de sauvegarde trouvé.");
            return;
        }

        Debug.Log("Chargement de la partie...");
        
        string json = File.ReadAllText(SavePath);
        SaveFileStructure globalSave = JsonUtility.FromJson<SaveFileStructure>(json);

        if (globalSave == null) return;
        
        Dictionary<string, string> dataMap = new Dictionary<string, string>();
        foreach (var data in globalSave.SystemsData)
        {
            if (dataMap.ContainsKey(data.ID))
            {
                Debug.LogWarning($"SaveManager: ID dupliqué '{data.ID}' dans le fichier de sauvegarde. Ignoré.");
                continue;
            }
            dataMap.Add(data.ID, data.JsonData);
        }
        
        foreach (ISaveable saveable in _saveables)
        {
            string id = saveable.GetSaveID();
            
            if (dataMap.TryGetValue(id, out string systemJson))
            {
                saveable.RestoreState(systemJson);
            }
            else
            {
                Debug.LogWarning($"SaveManager: Pas de données trouvées pour le système '{id}'.");
            }
        }
        
        Debug.Log("Chargement terminé !");
    }
}