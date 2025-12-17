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
    public int remainingAmount;
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
    public string species;
    public List<BlackboardEntry> blackboard = new List<BlackboardEntry>();
    public List<AgentSaveData> agents = new List<AgentSaveData>();
    public List<ColonyRelationData> relations = new List<ColonyRelationData>();
}

[Serializable]
public class AgentSaveData
{
    public string species;
    public Vector3 position;
    public float hunger;
    public float health;
    public float maxHealth;
    public InventoryItemData carriedItem;
}

[Serializable]
public struct InventoryItemData
{
    public int type;
    public int amount;
}

[Serializable]
public class MeteoSaveData
{
    public int weatherState;
    public float timer;
}

[Serializable]
public struct ColonyRelationData
{
    public int targetId;
    public float opinion;
    public RelationState state;
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
    public List<InventoryItemData> storedItems = new List<InventoryItemData>();
}

[Serializable]
public class PlayerCameraSaveData
{
    public Vector3 position;
    public float zoom;
}
