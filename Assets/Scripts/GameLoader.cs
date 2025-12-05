using UnityEngine;
using UnityEngine.Tilemaps;

public class GameLoader : MonoBehaviour
{
    public GameObject agentPrefab;
    [SerializeField] private GameObject agentParent;
    public WorldGeneration worldGen;

    public Tilemap tilemap;

    void Start()
    {
        LoadStatsAndAgents();
        LoadTilemap();
    }

    void LoadStatsAndAgents()
    {
        GameData data = SaveManager.LoadedData;
        if (data == null)
        {
            Debug.LogWarning("Pas de GameData, nouvelle partie");
            return;
        }

        Camera.main.transform.position = data.cam;

        for (int i = agentParent.transform.childCount - 1; i >= 0; i--)
            Destroy(agentParent.transform.GetChild(i).gameObject);

        foreach (AgentData agentData in data.agentData)
        {
            GameObject agent = Instantiate(agentPrefab, agentData.agentsPos,
                                           Quaternion.identity, agentParent.transform);
            AIStats aiStats = agent.GetComponent<AIStats>();
            aiStats.hunger    = agentData.hunger;
            aiStats.health    = agentData.health;
            aiStats.maxHealth = agentData.maxHealth;
        }
    }

    void LoadTilemap()
    {
        TilemapSave tData = SaveManager.loadedTilemap;
        if (tData == null)
        {
            Debug.LogWarning("Pas de TilemapSave, on garde la tilemap par défaut");
            return;
        }

        tilemap.ClearAllTiles();

        TileBase[] palette = SaveManager.instance.tilePalette;

        foreach (TileSaveData data in tData.tiles)
        {
            Vector3Int pos = new Vector3Int(data.x, data.y, 0);
            if (data.tileId < 0 || data.tileId >= palette.Length) continue;

            TileBase tile = palette[data.tileId];
            tilemap.SetTile(pos, tile);
        }

        tilemap.RefreshAllTiles();
        Debug.Log("Tilemap chargée");
    }
}
