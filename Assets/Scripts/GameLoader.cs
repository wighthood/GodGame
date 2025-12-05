using UnityEngine;

public class GameLoader : MonoBehaviour
{
    public GameObject agentPrefab;
    [SerializeField] private GameObject agentParent;
    public WorldGeneration worldGen;

    void Start()
    {
        GameData data = SaveManager.LoadedData;
        if (data == null)
        {
            Debug.LogWarning("Pas de données à charger, lancement nouvelle partie");
            return;
        }

        Camera.main.transform.position = data.cam;

        for (int i = agentParent.transform.childCount - 1; i >= 0; i--)
            Destroy(agentParent.transform.GetChild(i).gameObject);

        foreach (AgentData agentData in data.agentData)
        {
            GameObject agent = Instantiate(agentPrefab, agentData.agentsPos, Quaternion.identity, agentParent.transform);
            AIStats aiStats = agent.GetComponent<AIStats>();

            aiStats.hunger    = agentData.hunger;
            aiStats.health    = agentData.health;
            aiStats.maxHealth = agentData.maxHealth;
        }
    }
}