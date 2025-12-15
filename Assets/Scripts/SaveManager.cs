using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Collections;
using UnityEngine.SceneManagement;

[Serializable]
public class SaveFileStructure
{
    public List<SystemSaveData> SystemsData = new();
}

[Serializable]
public class SystemSaveData
{
    public string ID;
    public string JsonData;
}

[DefaultExecutionOrder(-10)] // Ensure SaveManager initializes early
public class SaveManager : MonoBehaviour
{
    private string SavePath => Application.persistentDataPath + "/savegame.json";
    
    private List<ISaveable> _saveables = new();

    private IEnumerator Start()
    {
        yield return null;

        if (SaveEvents.ShouldLoadOnStart)
        {
            LoadGame();
            SaveEvents.ShouldLoadOnStart = false;
        }
        else
        {
            SaveEvents.OnNewGameStartEvent?.Invoke();
        }
    }

    private void OnEnable()
    {
        SaveEvents.OnRegisterSaveableEvent += Register;
        SaveEvents.OnUnregisterSaveableEvent += Unregister;
        SaveEvents.OnRequestSaveEvent += HandleRequestSave;
    }

    private void OnDisable()
    {
        SaveEvents.OnRegisterSaveableEvent -= Register;
        SaveEvents.OnUnregisterSaveableEvent -= Unregister;
        SaveEvents.OnRequestSaveEvent -= HandleRequestSave;
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

    private void HandleRequestSave()
    {
        SaveGame();
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
        SaveEvents.OnSaveCompletedEvent?.Invoke();
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
        foreach (SystemSaveData data in globalSave.SystemsData)
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