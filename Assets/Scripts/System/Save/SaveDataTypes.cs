using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RessourceSave
{
    public List<RessourceSaveData> ress = new List<RessourceSaveData>();
}

[Serializable]
public class RessourceSaveData
{
    public Vector3 ressourcePos;
    public RessourceType ressourceType;
}
[Serializable]
public class TilemapSave
{
    public List<TileSaveData> tiles = new List<TileSaveData>();
}

[Serializable]
public class TileSaveData
{
    public int x;
    public int y;
    public int tileId;
}

[Serializable]
public class ColonySystemSaveData
{
    public int nextColonyId;
    public List<ColonySaveData> colonies = new List<ColonySaveData>();
}

[Serializable]
public class ColonySaveData
{
    public int id;
    public string name;
    public Vector3 position;
    public float influenceRadius;
    public int maxPop;
    public List<BlackboardEntry> blackboard = new List<BlackboardEntry>();
    public List<AgentSaveData> agents = new List<AgentSaveData>();
}

[Serializable]
public class AgentSaveData
{
    public string species;
    public Vector3 position;
    public float hunger;
    public float health;
    public float maxHealth;
}

[Serializable]
public class BlackboardEntry
{
    public string key;
    public string type;
    public float floatVal;
    public int intVal;
    public bool boolVal;
    public string stringVal;
}

[Serializable]
public class MapBuildingSystemSaveData
{
    public List<BuildingSaveData> buildings = new List<BuildingSaveData>();
}

[Serializable]
public class BuildingSaveData
{
    public Vector3 position;
    public int buildTypeId; 
    public int ownerColonyId; // -1 if none
}

[Serializable]
public class PlayerCameraSaveData
{
    public Vector3 position;
    public float zoom;
}
